using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GridBuildingSystem : MonoBehaviour
{
    //[SerializeField] public List<BuildingScriptableObject> buildingSOList;
    [SerializeField] private BuildingScriptableObject buildingSO;
    private Grid<GridObject> grid;
    [SerializeField] public BuildingScriptableObject roadSO;

    [SerializeField] private PreviewSystem previewSystem;
    [SerializeField] private RoadManager roadManager;
    [SerializeField] private EconomyManager economyManager;

    [SerializeField] private AudioClip placeObjectSound;
    [SerializeField] private float placeObjVolume = 0.2f;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private float errorVolume = 0.2f;
    [SerializeField] private AudioClip deleteSound;
    [SerializeField] private float deleteVolume = 0.1f;

    [SerializeField] private List<PlacedObject> placedObjects;

    public static GridBuildingSystem instance;
    public bool AddingBuilding { get; set; }
    public bool RemovingBuilding { get; set; }
    public bool PlacingRoad { get; set; }

    private Vector2Int lastPosition; 
    private void Awake()
    {
        instance = this;
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new Grid<GridObject>(gridHeight, gridWidth, cellSize, (Grid<GridObject> g, int i, int j) => new GridObject(g, i, j));
        AddingBuilding = false;
        RemovingBuilding = false;
        PlacingRoad = false;
    }

    //public BuildingScriptableObject GetBuildingByIdx(int idx) {  return buildingSOList[idx]; }

    private Vector3 GetRotatedObjectPositionAt(int x, int z)
    {
        Vector2Int rotationOffset = buildingSO.GetRotationOffset(buildingSO.Direction);
        Debug.Log($"Object world position: {x} , {z}");
        return grid.GetWorldPosition(x, z);
    }
    private Vector3 GetRotatedObjectPositionAt(int x, int z, BuildingScriptableObject.Dir dir)
    {
        Vector2Int rotationOffset = buildingSO.GetRotationOffset(dir);
        return grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
    }
    public BuildingScriptableObject PlaceObject(Vector3 worldPosition)
    {
        Debug.Log("Replacing");
        //buildingSO = buildingSOList[buildingIdx];
        if(buildingSO == null) return null;

        if(!economyManager.CanAfford(buildingSO))
        {
            return buildingSO;
        }

        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        Vector2Int gridPos = new Vector2Int(x, z);
        lastPosition = new Vector2Int(x, z + 1); //workaround so it updates after placing
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(gridPos, buildingSO.Direction);
        Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);


        if (CanPlace(gridPositionList) && !buildingSO.moduleOnly)
        {
            PlacedObject placedObj = PlacedObject.Create(rotatedObjWorldPosition, gridPos, buildingSO.Direction, buildingSO, false);

            if (placedObj.modules.Count > 0)
            {
                int i = 0;
                foreach (Vector2Int position in gridPositionList)
                {
                    grid.GetGridObj(position.x, position.y).SetPlacedObject((i % 2 == 0 ? placedObj : placedObj.modules[i / 2])); // Only works for 3x3 institutions needs reworking for arbitrary sized inst (gridPositionList doesn't have info of height and width)
                    i++;
                }
            }
            else
            {
                foreach (Vector2Int position in gridPositionList)
                    grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);
            }
            if (buildingSO.name == "Zandweg")
            {
                roadManager.PlaceRoad(new Vector2Int(x, z), placedObj.gameObject.transform.GetChild(0).GetComponent<MeshFilter>());
            }
            placedObjects.Add(placedObj);
            roadManager.UpdateConnections(gridPos, false);
            economyManager.HandleNewPlacedBuilding(buildingSO);
            Debug.LogError("Not error1");
            placedObj.OnPlace();
            Debug.LogError("Not error2");

            SoundFXManager.Instance.PlaySoundFXClip(placeObjectSound, placedObj.transform, placeObjVolume);
        }
        else if (buildingSO.module && CanSubstitute(gridPositionList))
        {
            GridObject oldObject = grid.GetGridObj(worldPosition);
            economyManager.HandleRemovedBuilding(oldObject.GetPlacedObject().GetScriptableObject(), oldObject.GetPlacedObject());
            ReplaceModule(worldPosition);
            economyManager.HandleNewPlacedBuilding(buildingSO);
        }
        else
        {
            //TODO: "can't place" pop up message for player
            Debug.Log("Can't build");
            SoundFXManager.Instance.PlaySoundFXClip(errorSound, transform, errorVolume);
        }
        return buildingSO;
    }

    private void PlaceModule(Vector3 worldPosition)
    {
        PlaceObject(worldPosition); //Placeholder
    }

    private void ReplaceModule(Vector3 worldPosition)
    {
        GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());
        PlacedObject placedObject = gridObject.GetPlacedObject();

        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        Vector2Int gridPos = new Vector2Int(x, z);
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(gridPos, placedObject.GetDir());

        PlacedObject placedObj = placedObject.Replace(buildingSO);
        foreach (Vector2Int position in gridPositionList)
            grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);
        roadManager.UpdateConnections(gridPos, false);

        SoundFXManager.Instance.PlaySoundFXClip(placeObjectSound, placedObj.transform, placeObjVolume);
    }
    private void RemoveModule(Vector3 worldPosition)
    {
        RemoveObject(worldPosition); //Placeholder
    }

    //public BuildingScriptableObject PlaceRoad(Vector3 worldPosition)
    //{
    //    grid.GetXYZ(worldPosition, out int x, out int y, out int z);
    //    List<Vector2Int> gridPositionList = roadSO.GetGridPositionList(new Vector2Int(x, z), roadSO.Direction);
    //    Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);


    //    if (CanPlace(gridPositionList))
    //    {
    //        Debug.Log($"Placing road at {new Vector2Int(x, z)}");
    //        PlacedObject placedObj = PlacedObject.Create(rotatedObjWorldPosition, new Vector2Int(x, z), roadSO.Direction, roadSO, false);
    //        foreach (Vector2Int position in gridPositionList)
    //            grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);

    //        roadManager.PlaceRoad(new Vector2Int(x, z), placedObj.gameObject.transform.GetChild(0).GetComponent<MeshFilter>());

    //        SoundFXManager.Instance.PlaySoundFXClip(placeObjectSound, placedObj.transform, 0.2f);
    //    }
    //    else
    //    {
    //        //TODO: "can't place" pop up message for player
    //        Debug.Log("Can't build");
    //        SoundFXManager.Instance.PlaySoundFXClip(errorSound, transform, 1f);
    //    }
    //    return roadSO;
    //}

    public PlacedObject RemoveObject(Vector3 worldPosition)
    {
        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        roadManager.DisconnectObject(new Vector2Int(x, z));

        if (buildingSO == null) return null;
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);

        GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());

        PlacedObject placedObject = (gridObject == null ? null : gridObject.GetPlacedObject());
        if (placedObject != null)
        {
            economyManager.HandleRemovedBuilding(placedObject.GetScriptableObject(), placedObject);

            placedObject.Destructor();
            placedObjects.Remove(placedObject);

            //If object is a road, make sure to remove it from road list
            if (placedObject.CompareTag("Road"))
            {
                roadManager.RemoveRoad(new Vector2Int(x, z));
            }


            gridPositionList = placedObject.GetGridPositionList();


            foreach (Vector2Int position in gridPositionList)
            {
                grid.GetGridObj(position.x, position.y).ClearPlacedObject();
            }


            SoundFXManager.Instance.PlaySoundFXClip(deleteSound, placedObject.transform, deleteVolume);
            return placedObject;
        }
        return null;
    }

    public string ReturnObjectBody(Vector3 worldPosition)
    {
        string body = "";
        grid.GetXYZ(worldPosition, out int x, out int y, out int z);

        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);

        GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());
        PlacedObject placedObject = gridObject.GetPlacedObject();
        if (placedObject != null)
        {

            BuildingScriptableObject buildingSO = placedObject.GetScriptableObject();

            body = buildingSO.GetData().GetBody();

            return body;
        }
        return body;
    }
    public string ReturnObjectName(Vector3 worldPosition)
    {
        string body = "";
        grid.GetXYZ(worldPosition, out int x, out int y, out int z);

        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);

        GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());
        PlacedObject placedObject = gridObject.GetPlacedObject();
        if (placedObject != null)
        {

            BuildingScriptableObject buildingSO = placedObject.GetScriptableObject();

            body = buildingSO.name;

            return body;
        }
        return body;
    }

    public void RotateObject()
    {
        if (buildingSO.modular) return;
        previewSystem.StopPlacementPreview();
        buildingSO.Direction = BuildingScriptableObject.GetNextDir(buildingSO.Direction);
        Debug.Log("Rotation updated: " + buildingSO.Direction);
        previewSystem.StartPlacementPreview(buildingSO);
    }

    private void Update()
    {
        grid.GetXYZ(UtilitiesClass.GetMouseWorldPositionXZ(), out int x, out int y, out int z);
        Vector2Int newPosition = new Vector2Int(x, z);
        //Debug.Log($"Updating Preview: {(AddingBuilding || PlacingRoad)}, {newPosition != lastPosition}, {buildingSO != null}");
        if ( (AddingBuilding || PlacingRoad) && newPosition != lastPosition && buildingSO != null)
        {
            lastPosition = newPosition;
            List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);
            Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);

            Debug.Log($"Preview: Location = {x}, {z}, CanPlace = {CanPlace(gridPositionList)}, CanSub = {CanSubstitute(gridPositionList) && buildingSO.module}");

            previewSystem.UpdatePreview(rotatedObjWorldPosition, CanPlace(gridPositionList), CanSubstitute(gridPositionList) && buildingSO.module);
        }



        //if (Input.GetMouseButtonDown(1)) // shortcut
        //{
        //    Debug.Log("right click");
        //    RemoveObject(UtilitiesClass.GetMouseWorldPositionXZ());
        //}

        if (Input.GetKeyDown(KeyCode.R)) // shortcut
        {
            RotateObject();
        }

    }

    public List<PlacedObject> GetBuildingsOfType(string buildingType)
    {
        List<PlacedObject> buildingsOfType = new();

        foreach(PlacedObject obj in placedObjects)
        {
            if(obj.GetScriptableObject().name == buildingType) buildingsOfType.Add(obj);
        }

        return buildingsOfType;
    }

    public bool CanPlace(List<Vector2Int> gridPositionList)
    {
        if (buildingSO.moduleOnly) return false;
        bool canPlace = true;
        foreach (Vector2Int position in gridPositionList)
        {
            GridObject gridObject = grid.GetGridObj(position.x, position.y);
            //Debug.Log($"Found object at {position.x}, {position.y}");
            if (gridObject == null)
            {
                canPlace = false; break;
            }
            else
            {
                if (!gridObject.CanPlace())
                {
                    canPlace = false; break;
                }
            }
        }
        return canPlace;
    }

    private bool CanSubstitute(List<Vector2Int> gridPositionList)
    {
        bool canSub = true;
        foreach (Vector2Int position in gridPositionList)
        {
            GridObject gridObject = grid.GetGridObj(position.x, position.y);
            if (gridObject == null)
            {
                canSub = false; break;
            }
            else
            {
                if (!gridObject.CanPlace())
                {
                    if (!gridObject.GetPlacedObject().isModule)
                        canSub = false; 
                    break;
                } else
                {
                    canSub = false;
                    break; // nothing to sub
                }
                
            }
        }
        return canSub;
    }

    internal void StartPlacementPreview(BuildingScriptableObject _buildingSO)
    {
        buildingSO = _buildingSO;

        previewSystem.StartPlacementPreview(buildingSO);
    }
    internal void StopPlacementPreview()
    {
        //buildingSO = null;

        previewSystem.StopPlacementPreview();
    }

    internal void StartRoadPlacementPreview()
    {
        buildingSO = roadSO;
        previewSystem.StartRoadPlacementPreview(roadSO);
    }

    public Grid<GridObject> GetGrid()
    {
        return grid;
    }

}

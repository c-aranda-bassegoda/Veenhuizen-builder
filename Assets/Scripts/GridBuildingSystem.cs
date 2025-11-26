using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] public List<BuildingScriptableObject> buildingSOList;
    private BuildingScriptableObject buildingSO;
    private Grid<GridObject> grid;
    [SerializeField] public BuildingScriptableObject roadSO;

    [SerializeField] private PreviewSystem previewSystem;
    [SerializeField] private RoadManager roadManager;

    [SerializeField] private AudioClip placeObjectSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip deleteSound;
    public bool AddingBuilding { get; set; }
    public bool RemovingBuilding { get; set; }
    public bool PlacingRoad { get; set; }

    private Vector2Int lastPosition; 
    private void Awake()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new Grid<GridObject>(gridHeight, gridWidth, cellSize, (Grid<GridObject> g, int i, int j) => new GridObject(g, i, j));
        AddingBuilding = false;
        RemovingBuilding = false;
        PlacingRoad = false;
    }

    public BuildingScriptableObject GetBuildingByIdx(int idx) {  return buildingSOList[idx]; }

    private Vector3 GetRotatedObjectPositionAt(int x, int z)
    {
        Vector2Int rotationOffset = buildingSO.GetRotationOffset(buildingSO.Direction);
        return grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
    }
    public BuildingScriptableObject PlaceObject(Vector3 worldPosition, int buildingIdx)
    {
        buildingSO = buildingSOList[buildingIdx];

        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        Vector2Int gridPos = new Vector2Int(x, z);
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(gridPos, buildingSO.Direction);
        Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);


        if (CanPlace(gridPositionList))
        {
            PlacedObject placedObj = PlacedObject.Create(rotatedObjWorldPosition, gridPos, buildingSO.Direction, buildingSO);
            foreach (Vector2Int position in gridPositionList)
                grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);
            roadManager.UpdateRoads(gridPos, false);
            placedObj.OnPlace();

            SoundFXManager.Instance.PlaySoundFXClip(placeObjectSound, placedObj.transform, 0.2f);
        }
        else
        {
            //TODO: "can't place" pop up message for player
            Debug.Log("Can't build");
            SoundFXManager.Instance.PlaySoundFXClip(errorSound, transform, 1f);
        }
        return buildingSO;
    }

    public BuildingScriptableObject PlaceRoad(Vector3 worldPosition)
    {
        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        List<Vector2Int> gridPositionList = roadSO.GetGridPositionList(new Vector2Int(x, z), roadSO.Direction);
        Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);


        if (CanPlace(gridPositionList))
        {
            Debug.Log($"Placing road at {new Vector2Int(x, z)}");
            PlacedObject placedObj = PlacedObject.Create(rotatedObjWorldPosition, new Vector2Int(x, z), roadSO.Direction, roadSO);
            foreach (Vector2Int position in gridPositionList)
                grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);

            roadManager.PlaceRoad(new Vector2Int(x, z), placedObj.gameObject.transform.GetChild(0).GetComponent<MeshFilter>());

            SoundFXManager.Instance.PlaySoundFXClip(placeObjectSound, placedObj.transform, 0.2f);
        }
        else
        {
            //TODO: "can't place" pop up message for player
            Debug.Log("Can't build");
            SoundFXManager.Instance.PlaySoundFXClip(errorSound, transform, 1f);
        }
        return roadSO;
    }

    public PlacedObject RemoveObject(Vector3 worldPosition)
    {
        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);

        GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());
        PlacedObject placedObject = gridObject.GetPlacedObject();
        if (placedObject != null)
        {
            placedObject.Destructor();

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

            roadManager.CheckRoadConnectionOnDelete(new Vector2Int(x, z));

            SoundFXManager.Instance.PlaySoundFXClip(deleteSound, placedObject.transform, 0.1f);
            return placedObject;
        }
        return null;
    }

    public void RotateObject()
    {
        previewSystem.StopPlacementPreview();
        buildingSO.Direction = BuildingScriptableObject.GetNextDir(buildingSO.Direction);
        Debug.Log("Direction updated: " + buildingSO.Direction);
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

            previewSystem.UpdatePreview(rotatedObjWorldPosition, CanPlace(gridPositionList));
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

    public bool CanPlace(List<Vector2Int> gridPositionList)
    {
        bool canPlace = true;
        foreach (Vector2Int position in gridPositionList)
        {
            GridObject gridObject = grid.GetGridObj(position.x, position.y);
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

    internal void StartPlacementPreview(int buildingIdx)
    {
        buildingSO = buildingSOList[buildingIdx];

        previewSystem.StartPlacementPreview(buildingSO);
    }
    internal void StopPlacementPreview()
    {
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

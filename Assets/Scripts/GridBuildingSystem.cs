using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private List<BuildingScriptableObject> buildingSOList;
    private BuildingScriptableObject buildingSO;
    private Grid<GridObject> grid;
    [SerializeField] BuildingScriptableObject roadSO;

    [SerializeField] private PreviewSystem previewSystem;
    [SerializeField] private RoadManager roadManager;

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

    private Vector3 GetRotatedObjectPositionAt(int x, int z)
    {
        Vector2Int rotationOffset = buildingSO.GetRotationOffset(buildingSO.Direction);
        return grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
    }
    public void PlaceObject(Vector3 worldPosition, int buildingIdx)
    {
        buildingSO = buildingSOList[buildingIdx];

        grid.GetXYZ(worldPosition, out int x, out int y, out int z);
        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);
        Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);


        if (CanPlace(gridPositionList))
        {
            PlacedObject placedObj = PlacedObject.Create(rotatedObjWorldPosition, new Vector2Int(x, z), buildingSO.Direction, buildingSO);
            foreach (Vector2Int position in gridPositionList)
                grid.GetGridObj(position.x, position.y).SetPlacedObject(placedObj);
        }
        else
        {
            //TODO: "can't place" pop up message for player
            Debug.Log("Can't build");
        }
    }

    public void PlaceRoad(Vector3 worldPosition)
    {
        buildingSO = roadSO;

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
        }
        else
        {
            //TODO: "can't place" pop up message for player
            Debug.Log("Can't build");
        }
    }

    public void RemoveObject(Vector3 worldPosition)
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

        }
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
        if ( (AddingBuilding || PlacingRoad) && newPosition != lastPosition && (buildingSO != null && roadSO != null))
        {
            lastPosition = newPosition;
            List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x, z), buildingSO.Direction);
            Vector3 rotatedObjWorldPosition = GetRotatedObjectPositionAt(x, z);

            previewSystem.UpdatePreview(rotatedObjWorldPosition, CanPlace(gridPositionList));
        }



        if (Input.GetMouseButtonDown(1)) // shortcut
        {
            Debug.Log("right click");
            RemoveObject(UtilitiesClass.GetMouseWorldPositionXZ());
        }

        if (Input.GetKeyDown(KeyCode.R)) // shortcut
        {
            RotateObject();
        }

    }

    private bool CanPlace(List<Vector2Int> gridPositionList)
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
        previewSystem.StartRoadPlacementPreview(roadSO);
    }

    public Grid<GridObject> GetGrid()
    {
        return grid;
    }

}

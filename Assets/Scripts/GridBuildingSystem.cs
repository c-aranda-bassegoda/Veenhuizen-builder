using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private List<BuildingScriptableObject> buildingSOList;
    private BuildingScriptableObject buildingSO;
    private Grid<GridObject> grid;

    [SerializeField] private PreviewSystem previewSystem;

    private Vector2Int lastPosition;
    private void Awake()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new Grid<GridObject>(gridHeight, gridWidth, cellSize, (Grid<GridObject> g, int i, int j) => new GridObject(g, i, j));

        buildingSO = buildingSOList[0];

        previewSystem.StartPlacementPreview(buildingSO);
    }

    int buildingIdx = 0;
    private void Update()
    {
        grid.GetXYZ(UtilitiesClass.GetMouseWorldPositionXZ(), out int x, out int y, out int z);

        Vector2Int rotationOffset = buildingSO.GetRotationOffset(buildingSO.Direction);
        Vector3 rotatedObjWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

        List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x,z), buildingSO.Direction);

        previewSystem.UpdatePreview(rotatedObjWorldPosition, CanPlace(gridPositionList));

        if (Input.GetMouseButtonDown(0))
        {
            //grid.GetXYZ(UtilitiesClass.GetMouseWorldPositionXZ(), out int x, out int y, out int z);

            
            if (CanPlace(gridPositionList))
            {
                Debug.Log(buildingSO.ToString());
                //Vector2Int rotationOffset = buildingSO.GetRotationOffset(dir);
                //Vector3 rotatedObjWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

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

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("right click");
            GridObject gridObject = grid.GetGridObj(UtilitiesClass.GetMouseWorldPositionXZ());
            PlacedObject placedObject = gridObject.GetPlacedObject();
            if (placedObject != null)
            {
                placedObject.Destructor();

                gridPositionList = placedObject.GetGridPositionList();

                foreach (Vector2Int position in gridPositionList)
                {
                    grid.GetGridObj(position.x, position.y).ClearPlacedObject();
                }

            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            previewSystem.StopPlacementPreview();
            buildingSO.Direction = BuildingScriptableObject.GetNextDir(buildingSO.Direction);
            Debug.Log("Direction updated: " + buildingSO.Direction);
            previewSystem.StartPlacementPreview(buildingSO);
        }

        
        if(Input.GetKeyDown(KeyCode.N)) // As in Next building, TODO: UI so you can seect the building type
        {
            previewSystem.StopPlacementPreview();
            buildingIdx += 1;
            if (buildingIdx > buildingSOList.Count - 1)
                buildingIdx = 0;
            Debug.Log(buildingIdx);
            buildingSO = buildingSOList[buildingIdx];
            previewSystem.StartPlacementPreview(buildingSO);
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
                Debug.Log("not in grid");
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
}

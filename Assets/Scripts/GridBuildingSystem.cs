using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private BuildingScriptableObject buildingSO;
    private Grid<GridObject> grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new Grid<GridObject>(gridHeight, gridWidth, cellSize, (Grid<GridObject> g, int i, int j) => new GridObject(g, i, j));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.GetXYZ(UtilitiesClass.GetMouseWorldPositionXZ(), out int x, out int y, out int z);

            List<Vector2Int> gridPositionList = buildingSO.GetGridPositionList(new Vector2Int(x,z));
            
            bool canBuild = true;
            foreach (Vector2Int position in gridPositionList)
            {
                GridObject gridObject = grid.GetGridObj(position.x, position.y);
                if (gridObject == null) 
                { 
                    Debug.Log("not in grid");
                    canBuild = false; break;
                }
                else
                {
                    if (!gridObject.CanPlace())
                    {
                        canBuild = false; break;
                    }
                }
            }
            if (canBuild)
            {
                Transform newTransform = Instantiate(buildingSO.prefab, grid.GetWorldPosition(x, z), Quaternion.identity);
                foreach (Vector2Int position in gridPositionList)
                    grid.GetGridObj(position.x, position.y).SetTransform(newTransform);
            }
            else
            {
                //TODO: "can't place" pop up message for player
                Debug.Log("Can't build");
            }
            
        }
    }

}

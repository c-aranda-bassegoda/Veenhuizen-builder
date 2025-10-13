using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private Transform testTransform;
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
            Instantiate(testTransform,grid.GetWorldPosition(x,z),Quaternion.identity);
        }
    }

}

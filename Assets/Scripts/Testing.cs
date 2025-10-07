using UnityEngine;

public class Testing : MonoBehaviour
{
    private Grid<GridObject> grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid<GridObject>(4, 2, 10f, (Grid<GridObject> g, int i, int j) => new GridObject(g, i, j));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //grid.SetGridObj(UtilitiesClass.GetMouseWorldPositionXZ(), true);
            Vector3 position = UtilitiesClass.GetMouseWorldPositionXZ();
            GridObject gridObj = grid.GetGridObj(position);
            if (gridObj != null)
                gridObj.SetValue(true);
        }
    }
}

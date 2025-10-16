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

public class GridObject
{
    private bool value;
    private Grid<GridObject> grid;
    private int x, y;

    public GridObject(Grid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetValue(bool newValue)
    {
        value = newValue;
        // Notify grid that this cell changed
        grid.TriggerGridObjChanged(x, y);
    }

    public override string ToString() => value.ToString();
}


}

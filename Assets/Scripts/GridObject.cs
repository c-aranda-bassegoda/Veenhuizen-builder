using UnityEngine;

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


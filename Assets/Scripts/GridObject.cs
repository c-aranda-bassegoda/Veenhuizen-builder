using UnityEngine;

public class GridObject
{
    private Grid<GridObject> grid;
    private int x, z;

    public GridObject(Grid<GridObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return x + "," + z;
    }
}


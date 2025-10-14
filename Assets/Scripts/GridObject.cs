using UnityEngine;

public class GridObject
{
    private Grid<GridObject> grid;
    private int x, z;
    private Transform transform;

    public GridObject(Grid<GridObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public void SetTransform(Transform transform)
    {
        this.transform = transform;
        grid.TriggerGridObjChanged(x,z);
    }

    public bool CanPlace()
    {
        return transform == null;
    }

    public void ClearTransform()
    {
        transform = null;
    }
    public override string ToString()
    {
        return x + "," + z + "\n" + transform;
    }
}


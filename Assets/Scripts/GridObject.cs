using UnityEngine;

public class GridObject
{
    private Grid<GridObject> grid;
    private int x, z;
    private PlacedObject placedObj;

    public GridObject(Grid<GridObject> grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
    }

    public void SetPlacedObject(PlacedObject placedObj)
    {
        this.placedObj = placedObj;
        grid.TriggerGridObjChanged(x,z);
    }

    public PlacedObject GetPlacedObject()
    {
        return this.placedObj;
    }

    public bool CanPlace()
    {
        return placedObj == null;
    }

    public void ClearPlacedObject()
    {
        placedObj = null;
        grid.TriggerGridObjChanged(x, z);
    }
    public override string ToString()
    {
        return x + "," + z + "\n";
    }
}


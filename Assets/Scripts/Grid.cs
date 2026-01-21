using System;
using UnityEngine;

public class Grid<TGridObject>
{
    public event EventHandler<OnGridObjChangedEventArgs> OnGridObjChanged;
    public class OnGridObjChangedEventArgs : EventArgs
    {
        public int i;
        public int j;
    }

    private int width;
    private int height;
    private float cellSize;
    [SerializeField] bool debugging = true;
    public float GetCellSize() { return cellSize; }

    private TGridObject[,] gridArray;
    private TextMesh[,] textMesh;
    public Grid(int width, int height, float cellSize, Func<Grid<TGridObject>, int, int, TGridObject> createGridObj)
    {

        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new TGridObject[width, height];
        textMesh = new TextMesh[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                gridArray[i, j] = createGridObj(this, i, j);
            }
        }


        if (debugging)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    textMesh[i, j] = UtilitiesClass.CreateWorldText(gridArray[i, j]?.ToString(), null, GetWorldPosition(i, j) + new Vector3(cellSize / 2, 0, cellSize / 2));

                    Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i, j + 1), Color.green, 100f);
                    Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i + 1, j), Color.green, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.green, 100f);
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.green, 100f);
        }

        OnGridObjChanged += (object sender, OnGridObjChangedEventArgs e) =>
        {
            if (debugging)
                textMesh[e.i, e.j].text = gridArray[e.i, e.j]?.ToString();
        };

    }

    public Vector3 GetWorldPosition(int i, int j) {
        return new Vector3(i, 0, j) * cellSize;
    }

    public void GetXYZ(Vector3? worldPosition, out int x, out int y, out int z)
    {
        if (!worldPosition.HasValue)
        {
            x = 0;
            y = 0;
            z = 0;
            return;
        }

        x = Mathf.FloorToInt(worldPosition.Value.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.Value.y / cellSize);
        z = Mathf.FloorToInt(worldPosition.Value.z / cellSize);
    }

    public void TriggerGridObjChanged(int i, int j)
    {
        if (OnGridObjChanged != null)
            OnGridObjChanged(this, new OnGridObjChangedEventArgs { i = i, j = j });
    }

    public void SetGridObj(int i, int j, TGridObject value)
    {
        if(i>=0 && j>=0 && i < width && j < height)
        {
            gridArray[i, j] = value;
            //textMesh[i,j].text = value.ToString();
            if(OnGridObjChanged != null)
                OnGridObjChanged(this, new OnGridObjChangedEventArgs { i=i, j=j });
        }
    }

    public void SetGridObj(Vector3 worldPosition, TGridObject value)
    {
        int i, j, k;

        GetXYZ(worldPosition, out i, out k, out j);
        SetGridObj(i, j, value);
    }

    public TGridObject GetGridObj(int i,  int j)
    {
        if (i>=0 && j>=0 && i<width && j<height)
            return gridArray[i, j];
        else
            return default(TGridObject);
    }

    public TGridObject GetGridObj(Vector3 worldPosition)
    {
        int i,j,k;
        GetXYZ(worldPosition, out i, out k, out j);
        return GetGridObj(i, j);
    }

    public TGridObject GetGridObj(Vector3? worldPosition)
    {
        if (worldPosition == null) return default(TGridObject);
        int i, j, k;
        GetXYZ(worldPosition.Value, out i, out k, out j);
        return GetGridObj(i, j);
    }

}

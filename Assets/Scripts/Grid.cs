using UnityEngine;

public class Grid
{
    private int width;
    private int height;
    private float cellSize;

    private int[,] gridArray;
    private TextMesh[,] textMesh;
    public Grid(int width, int height, float cellSize = 10)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new int[width, height];
        textMesh = new TextMesh[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                textMesh[i,j] = UtilitiesClass.CreateWorldText(gridArray[i, j].ToString(), null, GetWorldPosition(i, j) + new Vector3(cellSize / 2, 0, cellSize / 2));

                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i, j + 1), Color.green, 100f);
                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i + 1, j), Color.green, 100f);
            }
        }
         Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.green, 100f);
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.green, 100f);
    }

    private Vector3 GetWorldPosition(int i, int j) {
        return new Vector3(i, 0, j) * cellSize;
    }

    private void GetXYZ(Vector3 worldPosition, out int x, out int y, out int z)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
        z = Mathf.FloorToInt(worldPosition.z / cellSize);
    }

    public void SetValue(int i, int j, int value)
    {
        if(i>=0 && j>=0 && i < width && j < height)
        {
            gridArray[i, j] = value;
            textMesh[i,j].text = value.ToString();
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int i, j, k;

        GetXYZ(worldPosition, out i, out k, out j);
        SetValue(i, j, value);
    }

}

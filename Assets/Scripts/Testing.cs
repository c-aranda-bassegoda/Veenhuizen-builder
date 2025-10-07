using UnityEngine;

public class Testing : MonoBehaviour
{
    private Grid grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new Grid(4, 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(UtilitiesClass.GetMouseWorldPositionXZ(), 56);
        }
    }
}

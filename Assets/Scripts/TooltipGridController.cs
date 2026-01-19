using System.Collections;
using UnityEngine;


public class TooltipGridController : MonoBehaviour
{

    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridBuildingSystem gridBuildingSystem;
    private Grid<GridObject> grid;
    [SerializeField] private float delay = 1f;

    private Coroutine hoverCoroutine;
    private TooltipBuildingData currentData;

    private void Start()
    {
        grid = gridBuildingSystem.GetGrid();
        Debug.Log("Grid assigned: " + (grid != null));
    }
    private void OnEnable()
    {
        inputManager.OnHover += HandleHover;
        inputManager.OnHoverExit += HandleExit;
    }

    private void OnDisable()
    {
        inputManager.OnHover -= HandleHover;
        inputManager.OnHoverExit -= HandleExit;
    }

    private void HandleHover(Vector3 worldPos)
    {
        GridObject obj = grid.GetGridObj(worldPos);
        if (obj == null) return;

        var placedObject = obj.GetPlacedObject();
        if (placedObject == null) return;

        var scriptableObject = placedObject.GetScriptableObject();
        if (scriptableObject == null) return;

        TooltipBuildingData data = scriptableObject.GetData();
        if (data == null) return;

        if (data == currentData) return;

        currentData = data;
        RestartHover();
    }

    private void RestartHover()
    {
        StopHover();
        Debug.Log("starting hover coroutine");
        hoverCoroutine = StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        TooltipSystem.instance.Show(currentData.GetBody(), currentData.happinessCount, currentData.controlCount, currentData.peopleCount, currentData.GetHeader());
    }

    private void HandleExit()
    {
        StopHover();
        Debug.Log("stopping hover coroutine");
        TooltipSystem.Hide();
        currentData = null;
    }

    private void StopHover()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
    }
}



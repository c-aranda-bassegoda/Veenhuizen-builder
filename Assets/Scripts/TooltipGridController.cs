using System.Collections;
using UnityEngine;


public class TooltipGridController : MonoBehaviour
{

    [SerializeField] private InputManager inputManager;
    [SerializeField] private GridBuildingSystem gridBuildingSystem;
    private Grid<GridObject> grid;
    [SerializeField] private float delay = 1f;

    private Coroutine hoverCoroutine;
    [SerializeField] private TooltipBuildingData currentData, previousData;
    [SerializeField] bool exiting, tooltipShowing;
    [SerializeField] float exitBuffer;

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
        if (obj == null)
        {
            currentData = null;
            return;
        }

        var placedObject = obj.GetPlacedObject();
        if (placedObject == null)
        {
            currentData = null;
            return;
        }
        var scriptableObject = placedObject.GetScriptableObject();
        if (scriptableObject == null)
        {
            currentData = null;
            return;
        }

        TooltipBuildingData data = scriptableObject.GetData();
        if (data == null)
        {
            currentData = null;
            return;
        }

        if (data.GetHeader() == "Empty")
        {
            placedObject = placedObject.parent;
            if (placedObject == null)
            {
                currentData = null;
                return;
            }
            scriptableObject = placedObject.GetScriptableObject();
            if (scriptableObject == null)
            {
                currentData = null;
                return;
            }
            data = scriptableObject.GetData();
            if (data == null)
            {
                currentData = null;
                return;
            }
        }

        if(placedObject.name == "Gesticht")
        {
            data = GetEntireInstutionData(placedObject, data);
        }

        if (IsSameData(data, currentData)) return;
        if (IsSameData(data, previousData) && previousData != null) return;

        currentData = data;
        if(!exiting) RestartHover();
    }

    private void RestartHover()
    {
        StartCoroutine(StopHover());
        Debug.Log("starting hover coroutine");
        hoverCoroutine = StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if(currentData != null)
        {
            TooltipSystem.instance.Show(currentData.GetBody(), currentData.happinessCount, currentData.controlCount, currentData.peopleCount, currentData.cost, currentData.GetHeader());
            tooltipShowing = true;
        }
    }

    private void HandleExit()
    {
        StartCoroutine(StopHover());
    }

    private IEnumerator StopHover()
    {
        if (!exiting && tooltipShowing)
        {
            exiting = true;
            previousData = currentData;
            yield return new WaitForSeconds(exitBuffer);

            if (!IsSameData(previousData, currentData))
            {
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                    hoverCoroutine = null;
                }
                TooltipSystem.Hide();
                tooltipShowing = false;
            }
            
            currentData = null;
            previousData = null;
            exiting = false;
        }
    }

    bool IsSameData(TooltipBuildingData data1, TooltipBuildingData data2)
    {
        if (data1 == null && data2 == null) return false;
        if (data1 == null) return false;
        if (data2 == null) return false;

        bool isSameData = true;

        if(data1.controlCount != data2.controlCount) isSameData = false;
        if(data1.peopleCount != data2.peopleCount) isSameData = false;
        if(data1.happinessCount != data2.happinessCount) isSameData = false;
        if(data1.cost != data2.cost) isSameData = false;
        if(data1.GetHeader() != data2.GetHeader()) isSameData = false;

        return isSameData;

    }

    TooltipBuildingData GetEntireInstutionData(PlacedObject institution, TooltipBuildingData data)
    {
        TooltipBuildingData baseData = data;

        foreach(PlacedObject module in institution.modules)
        {
            if(module == null) continue;
            BuildingScriptableObject moduleBSO = module.GetScriptableObject();
            if(moduleBSO == null) continue;
            TooltipBuildingData moduleData = moduleBSO.GetData();
            if(moduleData == null) continue;

            baseData.controlCount += moduleData.controlCount;
            baseData.peopleCount += moduleData.peopleCount;
            baseData.happinessCount += moduleData.happinessCount;
            baseData.cost += moduleData.cost;
        }

        return baseData;
    }
}



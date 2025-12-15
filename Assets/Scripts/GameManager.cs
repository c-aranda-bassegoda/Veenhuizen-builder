using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public InputManager inputManager;
    public GridBuildingSystem gridBuildingSystem;
    public UIController controller;
    public EconomyManager economyManager;
    int buildingIdx = -1;


    private void Start()
    {
        controller.OnPlaceBuilding += BuildingPlacementHandler;
        controller.OnRotate += BuildingRotateHandler;
        controller.OnDelete += BuildingDeletionHandler;
        controller.OnPlaceRoad += RoadPlacingHandler;
        inputManager.OnClicked += HandleMouseClick;
    }

    private void BuildingDeletionHandler()
    {
        gridBuildingSystem.RemovingBuilding = true;
        gridBuildingSystem.AddingBuilding = false;
        gridBuildingSystem.PlacingRoad = false;
        gridBuildingSystem.StopPlacementPreview();

        inputManager.OnClicked -= HandleMouseClick;
        inputManager.OnClicked += HandleMouseClick;
    }

    private void BuildingRotateHandler()
    {
        gridBuildingSystem.RotateObject();
    }

    private void BuildingPlacementHandler(BuildingScriptableObject buildingSO)
    {
        gridBuildingSystem.RemovingBuilding = false;
        gridBuildingSystem.AddingBuilding = true;
        gridBuildingSystem.PlacingRoad = false;
        gridBuildingSystem.StopPlacementPreview();
        gridBuildingSystem.StartPlacementPreview(buildingSO);

        //HandleStats(buildingIdx);

        inputManager.OnClicked -= HandleMouseClick;
        inputManager.OnClicked += HandleMouseClick;

        Debug.Log($"Building placement handler: {gridBuildingSystem.AddingBuilding}");
    }

    private void RoadPlacingHandler()
    {
        gridBuildingSystem.StopPlacementPreview();
        gridBuildingSystem.RemovingBuilding = false;
        gridBuildingSystem.AddingBuilding = false;
        gridBuildingSystem.PlacingRoad = true;
        gridBuildingSystem.StartRoadPlacementPreview();

        //HandleStats();

        inputManager.OnClicked -= HandleMouseClick;
        inputManager.OnClicked += HandleMouseClick;
        //gridBuildingSystem.PlaceRoad();
    }

    private void HandleMouseClick(Vector3 position)
    {
        BuildingScriptableObject objectSO = null;
        Debug.Log("Handling mouse click");
        Debug.Log($"Adding building: {gridBuildingSystem.AddingBuilding}");
        //if(gridBuildingSystem.PlacingRoad)
        //{
        //    objectSO = gridBuildingSystem.roadSO;
        //    //if (economyManager.HandleNewBuilding(objectSO))
        //    Debug.Log("placing road");
        //    gridBuildingSystem.PlaceRoad(position);
        //    uiManager.UpdateStats();
        //}

        if (gridBuildingSystem.RemovingBuilding)
        {
            PlacedObject objToRemove = gridBuildingSystem.RemoveObject(position);
            if (objToRemove != null) 
                objectSO = objToRemove.GetScriptableObject();
        }
        if (gridBuildingSystem.AddingBuilding)
        {
            //if (objectSO.name == "Road")
            //{
            //    gridBuildingSystem.PlaceRoad(position);
            //}
            //objectSO = gridBuildingSystem.GetBuildingByIdx(buildingIdx);
            //if (economyManager.HandleNewBuilding(objectSO))
            Debug.Log("Adding building");
            gridBuildingSystem.PlaceObject(position);
        }
        //controller.HideHousingPanel();
    }

    private void OnDestroy()
    {
        controller.OnPlaceBuilding -= BuildingPlacementHandler;
        inputManager.OnClicked -= HandleMouseClick;
    }
}

public static class GameEvents
{
    public static Action<float, float, float> OnStatsChanged;
    public static Action<float> OnMoneyChanged;
    public static Action<int, int> OnCalendarChanged; 
    public static Action OnShowProgressReport;
    public static Action OnResumeTime;
    public static Action<string> OnErrorMessage;
}
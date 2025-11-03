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

    private void BuildingPlacementHandler(int idx)
    {
        gridBuildingSystem.RemovingBuilding = false;
        gridBuildingSystem.AddingBuilding = true;
        gridBuildingSystem.PlacingRoad = false;
        gridBuildingSystem.StopPlacementPreview();
        buildingIdx = idx;
        gridBuildingSystem.StartPlacementPreview(buildingIdx);

        //HandleStats(buildingIdx);

        inputManager.OnClicked -= HandleMouseClick; 
        inputManager.OnClicked += HandleMouseClick;
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
        Debug.Log("Handling mouse click");
        if(gridBuildingSystem.PlacingRoad)
        {
            gridBuildingSystem.PlaceRoad(position);
            economyManager.HandleNewBuilding("Road"); // TODO: remove hardcoding, is it worth it?
        }

        if (buildingIdx < 0)
        {
            Debug.LogWarning("No building selected!");
            return;
        }
        if (gridBuildingSystem.RemovingBuilding)
        {
            string buildingName = gridBuildingSystem.RemoveObject(position);
            economyManager.HandleRemovedBuilding(buildingName);
        }
        if (gridBuildingSystem.AddingBuilding)
        {
            gridBuildingSystem.PlaceObject(position, buildingIdx);
            string buildingName = gridBuildingSystem.buildingSOList[buildingIdx].name;
            economyManager.HandleNewBuilding(buildingName);
        }
        controller.HideHousingPanel();
    }

    private void OnDestroy()
    {
        controller.OnPlaceBuilding -= BuildingPlacementHandler;
        inputManager.OnClicked -= HandleMouseClick;
    }
}

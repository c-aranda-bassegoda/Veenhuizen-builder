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
    int buildingIdx = -1;
    private Dictionary<string, int> buildingCount;

    private void Start()
    {
        controller.OnPlaceBuilding += BuildingPlacementHandler;
        controller.OnRotate += BuildingRotateHandler;
        controller.OnDelete += BuildingDeletionHandler;
        controller.OnPlaceRoad += RoadPlacingHandler;

        buildingCount = new Dictionary<string, int>();
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
            if (buildingCount.ContainsKey("road"))
            {
                buildingCount["road"] += 1;
                Debug.Log(buildingCount["road"]);
                Debug.Log("roads: " + buildingCount["road"]);
            } else
            {
                buildingCount.Add("road", 1);
            }
        }

        if (buildingIdx < 0)
        {
            Debug.LogWarning("No building selected!");
            return;
        }
        if (gridBuildingSystem.RemovingBuilding)
        {
            string buildingName = gridBuildingSystem.RemoveObject(position);
            if (buildingCount.ContainsKey(buildingName))
            {
                buildingCount[buildingName] -= 1;
                Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
            }
            else
            {
                Debug.LogError("No building named "+ buildingName);
            }
        }
        if (gridBuildingSystem.AddingBuilding)
        {
            gridBuildingSystem.PlaceObject(position, buildingIdx);
            string buildingName = gridBuildingSystem.buildingSOList[buildingIdx].name;
            if (buildingCount.ContainsKey(buildingName))
            {
                buildingCount[buildingName] += 1;
                Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
            }
            else
            {
                buildingCount.Add(buildingName, 1);
            }
        }
        controller.HideHousingPanel();
    }

    private void OnDestroy()
    {
        controller.OnPlaceBuilding -= BuildingPlacementHandler;
        inputManager.OnClicked -= HandleMouseClick;
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public InputManager inputManager;
    public GridBuildingSystem gridBuildingSystem;
    public UIController controller;
    int buildingIdx = -1;

    private void Start()
    {
        controller.OnPlaceBuilding += BuildingPlacementHandler;
        controller.OnRotate += BuildingRotateHandler;
        controller.OnDelete += BuildingDeletionHandler;
    }

    private void BuildingDeletionHandler()
    {
        gridBuildingSystem.RemovingBuilding = true;
        gridBuildingSystem.AddingBuilding = false;
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
        gridBuildingSystem.StopPlacementPreview();
        buildingIdx = idx;
        gridBuildingSystem.StartPlacementPreview(buildingIdx);

        inputManager.OnClicked -= HandleMouseClick; 
        inputManager.OnClicked += HandleMouseClick;
    }

    private void HandleMouseClick(Vector3 position)
    {
        if (buildingIdx < 0)
        {
            Debug.LogWarning("No building selected!");
            return;
        }
        if (gridBuildingSystem.RemovingBuilding)
            gridBuildingSystem.RemoveObject(position);
        if (gridBuildingSystem.AddingBuilding)
            gridBuildingSystem.PlaceObject(position, buildingIdx);
    }

    private void OnDestroy()
    {
        controller.OnPlaceBuilding -= BuildingPlacementHandler;
        inputManager.OnClicked -= HandleMouseClick;
    }
}

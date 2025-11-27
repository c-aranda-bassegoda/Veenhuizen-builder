using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public int GetBuildingCount(string name) 
    { 
        if (buildingCount.ContainsKey(name)) return buildingCount[name]; 
        else { Debug.LogError("No building " + name); return -1; } 
    }

    public float happy, control, money;
    [SerializeField] private List<BuildingScriptableObject> buildings;
    [SerializeField] private List<BuildingScriptableObject> placedBSOs;
    private List<PlacedObject> placedObjects;
    private Dictionary<string, int> maxCount;

    private void Start()
    {
        placedObjects = new();
        placedBSOs = new();

        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;

        maxCount = new Dictionary<string, int>();
        foreach (BuildingScriptableObject building in buildings)
        {
            maxCount.Add(building.name, building.maxPlacements);
            buildingCount.Add(building.name, 0);
        }
    }

    public bool CanAfford(BuildingScriptableObject buildingSO)
    {
        if (buildingSO.buildCost <= money) return true;
        else return false;
    }

    public void HandleNewPlacedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {
        placedBSOs.Add(newObject);
        money -= newObject.buildCost;
    }

    public void HandleNewConnectedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {

        if (newObject == null)
            Debug.LogError("No new object");

        if (placedObjects.Contains(building)) return;

        string buildingName = newObject.name;

        if (maxCount[buildingName] <= buildingCount[buildingName])
        {
            Debug.LogError("Can't place more buildings of type " + buildingName);
        }

        if (buildingCount.ContainsKey(buildingName))
        {
            buildingCount[buildingName] += 1;
            Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        }
        else
        {
            buildingCount.Add(buildingName, 1);
        }
        placedObjects.Add(building);
        happy += newObject.hapiness;
        control += newObject.control;
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString());
    }

    public void HandleRemovedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No new object");
        string buildingName = oldObject.name;

        if (!placedObjects.Contains(building)) return;

        if (buildingCount.ContainsKey(buildingName))
        {
            buildingCount[buildingName] -= 1;
            Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        }
        else
        {
            Debug.LogError("No building named " + buildingName);
        }
        placedObjects.Remove(building);
        happy -= oldObject.hapiness;
        control -= oldObject.control;
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString());
    }
}


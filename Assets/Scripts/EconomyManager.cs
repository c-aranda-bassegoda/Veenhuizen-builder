using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public int GetBuildingCount(string name) 
    { 
        if (buildingCount.ContainsKey(name)) return buildingCount[name]; 
        else { Debug.Log("No building " + name); return -1; } 
    }

    public float happy, control, money;
    //[SerializeField] private List<BuildingScriptableObject> buildings;
    [SerializeField] private List<BuildingScriptableObject> placedBSOs;
    private List<PlacedObject> placedObjects;
    private Dictionary<string, int> maxCount;
    public static EconomyManager instance;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        placedObjects = new();
        placedBSOs = new();

        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;

        maxCount = new Dictionary<string, int>();

        UIManager.instance.UpdateMoney(money);
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
        UIManager.instance.UpdateMoney(money);
    }

    public void HandleNewConnectedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {

        if (newObject == null)
            Debug.LogError("No new object");

        if (placedObjects.Contains(building)) return;

        string buildingName = newObject.name;

        if(!maxCount.ContainsKey(buildingName))
        {
            maxCount.Add(newObject.name, newObject.maxPlacements);
            buildingCount.Add(newObject.name, 0);
        }

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


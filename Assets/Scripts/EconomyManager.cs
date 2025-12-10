using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public int GetBuildingCount(string name) 
    { 
        if (buildingCount.ContainsKey(name)) return buildingCount[name]; 
        else { Debug.Log("No building " + name); return -1; } 
    }

    public float happy, control, money, food, ppl;
    //[SerializeField] private List<BuildingScriptableObject> buildings;
    [SerializeField] private List<BuildingScriptableObject> placedBuildingsSOs;
    [SerializeField] private List<PlacedObject> connectedObjects;
    [SerializeField]private Dictionary<string, int> maxCount;
    public static EconomyManager instance;
    [SerializeField] float secondsPerDay;
    int dayNumber;
    int yearNumber;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        connectedObjects = new();
        placedBuildingsSOs = new();

        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;
        ppl = 0;

        maxCount = new Dictionary<string, int>();

        UIManager.instance.UpdateMoney(money);

        StartCoroutine(Economy());
    }

    IEnumerator Economy()
    {
        yearNumber = 1;

        while(true)
        {
            dayNumber++;
            if(dayNumber >= 125)
            {
                dayNumber = 1;
                yearNumber++;
            }
            UIManager.instance.UpdateCalendar(dayNumber, yearNumber);
            HandleDayEcon();
            yield return new WaitForSeconds(secondsPerDay);
        }
    }

    void HandleDayEcon()
    {
        foreach (BuildingScriptableObject bso in placedBuildingsSOs)
        {
            money -= (bso.yearlyCost / 124);
            money += (bso.yearlyEarnings / 124);

            //food -= (bso.yearlyFoodCost / 124);
            //food += (bso.yearlyFoodEarnings / 124);
        }
        UIManager.instance.UpdateMoney(money);
    }

    public bool CanAfford(BuildingScriptableObject buildingSO)
    {
        if (buildingSO.buildCost <= money) return true;
        else return false;
    }

    public void HandleNewPlacedBuilding(BuildingScriptableObject newObject)
    {
        if (newObject == null)
            Debug.LogError("No new object");

        if (placedBuildingsSOs.Contains(newObject)) return;

        string buildingName = newObject.name;

        if (!maxCount.ContainsKey(buildingName))
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
        placedBuildingsSOs.Add(newObject);
        money -= newObject.buildCost;
        control += newObject.control;
        ppl += newObject.population;
        UIManager.instance.UpdateMoney(money);
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }

    public void HandleNewConnectedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {
        connectedObjects.Add(building);
        happy += newObject.hapiness;
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }

    public void HandleRemovedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No new object");
        string buildingName = oldObject.name;

        if (!connectedObjects.Contains(building)) return;

        if (buildingCount.ContainsKey(buildingName))
        {
            buildingCount[buildingName] -= 1;
            Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        }
        else
        {
            Debug.LogError("No building named " + buildingName);
        }
        happy -= oldObject.hapiness;
        control -= oldObject.control;
        ppl -= oldObject.population;
        placedBuildingsSOs.Remove(oldObject);
        connectedObjects.Remove(building);
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }
}


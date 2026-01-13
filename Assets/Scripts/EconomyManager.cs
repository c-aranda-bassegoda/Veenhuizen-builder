using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}
public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public int GetBuildingCount(string name) 
    { 
        if (buildingCount.ContainsKey(name)) return buildingCount[name]; 
        else { Debug.Log("No building " + name); return -1; } 
    }

    public float happy, control, money, food, ppl;
    [SerializeField] private bool timePaused = false;
    //[SerializeField] private List<BuildingScriptableObject> buildings;
    [SerializeField] private List<BuildingScriptableObject> placedBuildingsSOs;
    [SerializeField] private List<PlacedObject> connectedObjects;
    [SerializeField]private Dictionary<string, int> maxCount;
    [SerializeField] float secondsPerSeason;
    //[SerializeField] int daysPerSeason;
    [SerializeField] GameObject floatingTextPrefab;

    public Season currentSeason;
    int seasonsPassed;

    private void Start()
    {
        currentSeason = Season.Spring;
        connectedObjects = new();
        placedBuildingsSOs = new();

        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;
        ppl = 0;

        maxCount = new Dictionary<string, int>();

        GameEvents.OnMoneyChanged?.Invoke(money);

        StartCoroutine(Economy());
    }

    IEnumerator Economy()
    {
        seasonsPassed = 0;
        float timePassedInSeconds = 0;

        while(true)
        {
            while (timePaused)
                yield return null;

            timePassedInSeconds += Time.deltaTime;

            if(timePassedInSeconds >= secondsPerSeason)
            {
                timePassedInSeconds = 0;
                AdvanceSeason();
                PauseTime();

                if (seasonsPassed >= 4)
                {
                    GameEvents.OnGameFinished?.Invoke();
                    break;
                }
                else
                {
                    GameEvents.OnShowProgressReport?.Invoke();
                }
            }

            GameEvents.OnCalendarChanged?.Invoke(currentSeason, timePassedInSeconds, secondsPerSeason);

            yield return null;
        }
    }

    void AdvanceSeason()
    {
        seasonsPassed++;

        if (currentSeason == Season.Spring) currentSeason = Season.Summer;
        else if (currentSeason == Season.Summer) currentSeason = Season.Autumn;
        else if (currentSeason == Season.Autumn) currentSeason = Season.Winter;
        else if (currentSeason == Season.Winter) currentSeason = Season.Spring;
    }
    private void OnEnable()
    {
        GameEvents.OnResumeTime += ResumeTime;
    }

    private void OnDisable()
    {
        GameEvents.OnResumeTime -= ResumeTime;
    }

    public void PauseTime() { timePaused = true; }
    public void ResumeTime() { timePaused = false; }

    void HandleDayEcon()
    {
        foreach (BuildingScriptableObject bso in placedBuildingsSOs)
        {
            money -= (bso.yearlyCost / 124);
            money += (bso.yearlyEarnings / 124);

            //food -= (bso.yearlyFoodCost / 124);
            //food += (bso.yearlyFoodEarnings / 124);
        }
        GameEvents.OnMoneyChanged?.Invoke(money);
    }

    public bool CanAfford(BuildingScriptableObject buildingSO)
    {
        if (buildingSO.buildCost >= money)
        {
            PauseTime();
            GameEvents.OnErrorMessage("Can't afford building");
            return false;
        }

        //string buildingName = buildingSO.name;
        //if (!buildingCount.ContainsKey(buildingName))
        //    return true;
        //if (maxCount[buildingName] <= buildingCount[buildingName])
        //{
        //    PauseTime();
        //    GameEvents.OnErrorMessage("Can't place more buildings of type " + buildingName);
        //    return false;
        //}
        
        return true;
    }

    public void HandleNewPlacedBuilding(BuildingScriptableObject newObject)
    {
        if (newObject == null)
            Debug.LogError("No new object");

        string buildingName = newObject.name;

        //if (!maxCount.ContainsKey(buildingName))
        //{
        //    maxCount.Add(newObject.name, newObject.maxPlacements);
        //    buildingCount.Add(newObject.name, 0);
        //}

        //if (maxCount[buildingName] <= buildingCount[buildingName])
        //{
        //    Debug.LogError("Can't place more buildings of type " + buildingName);
        //}

        //if (buildingCount.ContainsKey(buildingName))
        //{
        //    buildingCount[buildingName] += 1;
        //    Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        //}
        //else
        //{
        //    buildingCount.Add(buildingName, 1);
        //}
        placedBuildingsSOs.Add(newObject);
        money -= newObject.buildCost;
        happy += newObject.hapiness;
        control += newObject.control;
        ppl += newObject.population;
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnMoneyChanged?.Invoke(money);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }

    public void HandleNewConnectedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {
        connectedObjects.Add(building);
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }

    public void HandleRemovedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No object");
        string buildingName = oldObject.name;

        //if (buildingCount.ContainsKey(buildingName))
        //{
        //    buildingCount[buildingName] -= 1;
        //    Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        //}
        //else
        //{
        //    Debug.LogError("No building named " + buildingName);
        //}

        //if (connectedObjects.Contains(building))
        //{
        //    happy -= oldObject.hapiness;
        //    control -= oldObject.control;
        //    ppl -= oldObject.population;
        //    connectedObjects.Remove(building);
        //    GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);
        //    NPCManager.instance.ChangeValues(control, happy, ppl);
        //}

        happy -= oldObject.hapiness;
        control -= oldObject.control;
        ppl -= oldObject.population;
        connectedObjects.Remove(building);
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);


        money += oldObject.buildCost;
        placedBuildingsSOs.Remove(oldObject);
        GameEvents.OnMoneyChanged?.Invoke(money);
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString() + "Population: " + ppl.ToString());
    }

    internal void HandleDisconnectedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No object");
        string buildingName = oldObject.name;
        if (!connectedObjects.Contains(building)) return;

    }


    // Displays cost with floating text
    internal void ShowTransaction(Vector3 position, Transform transform, BuildingScriptableObject placedObject, bool paid, bool replaced, BuildingScriptableObject replacedObject = null)
    {
        float cost = placedObject.buildCost;
        if (replaced && replacedObject != null)
        {
            Debug.Log(placedObject.name + " " + replacedObject.name);
            cost = replacedObject.buildCost - cost;
        } else if (paid) cost = -cost;

        if (cost == 0) return; 

        Vector3 offset = new Vector3(0.0f, 9.0f, 0.0f);
        var textGO =  Instantiate(floatingTextPrefab, position + offset, Quaternion.identity, transform);


        textGO.GetComponent<TMP_Text>().text = (cost < 0? "": "+") + cost.ToString();

        textGO.GetComponent<TMP_Text>().color = (cost < 0 ? Color.red: Color.green);
    }
}


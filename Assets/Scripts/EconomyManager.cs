using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public float happy, control;
    [SerializeField] private List<BuildingConditions> maxBuildingCount;
    private Dictionary<string, int> maxCount;

    private void Start()
    {
        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;

        maxCount = new Dictionary<string, int>();
        foreach (BuildingConditions condition in maxBuildingCount)
        {
            maxCount.Add(condition.building,condition.maxCount);
            buildingCount.Add(condition.building, 0);
        }
    }

    public bool HandleNewBuilding(BuildingScriptableObject newObject)
    {

        if (newObject == null)
            Debug.LogError("No new object");
        string buildingName = newObject.name;

        if (maxCount[buildingName] <= buildingCount[buildingName])
        {
            Debug.Log("Can't place more buildings of type " + buildingName);
            return false;
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
        happy += newObject.hapiness;
        control += newObject.control;
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString());

        return true;
    }

    public void HandleRemovedBuilding(BuildingScriptableObject oldObject)
    {
        if (oldObject == null)
            Debug.LogError("No new object");
        string buildingName = oldObject.name;

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
        Debug.Log("Happy: " + happy.ToString() + " Control: " + control.ToString());
    }
}

[System.Serializable]
public class BuildingConditions
{
    public string building;
    public int maxCount;
}

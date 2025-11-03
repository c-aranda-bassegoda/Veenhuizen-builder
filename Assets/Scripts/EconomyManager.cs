using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public GameObject happyOut, controlOut;

    private void Start()
    {
        buildingCount = new Dictionary<string, int>();
    }

    public void HandleNewBuilding(string buildingName)
    {
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

    public void HandleRemovedBuilding(string buildingName)
    {
        if (buildingCount.ContainsKey(buildingName))
        {
            buildingCount[buildingName] -= 1;
            Debug.Log(buildingName + "s: " + buildingCount[buildingName]);
        }
        else
        {
            Debug.LogError("No building named " + buildingName);
        }
    }
}

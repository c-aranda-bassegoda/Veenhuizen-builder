using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public float happy, control;
    [SerializeField] private List<BuildingScriptableObject> listSO;

    private void Start()
    {
        buildingCount = new Dictionary<string, int>();
        happy = 0;
        control = 0;
    }

    public void HandleNewBuilding(BuildingScriptableObject newObject)
    {
        if (newObject == null)
            Debug.LogError("No new object");
        string buildingName = newObject.name;
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

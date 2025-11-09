using System;
using UnityEditor;
using UnityEngine;

public class TooltipBuildingData : MonoBehaviour
{
    [SerializeField]
    private BuildingScriptableObject buildingSO;
    [SerializeField]
    private EconomyManager economyManager;
    private string body = "";

    private void Start()
    {
        body = "Happiness: " + buildingSO.hapiness + "\n";
        body += "Control: " + buildingSO.control + "\n";
        body += "Max placements: " + buildingSO.maxPlacements + "\n";
    }

    internal string GetBody()
    {
        string dynamicBody = buildingSO.name + "s left: " + (buildingSO.maxPlacements - economyManager.GetBuildingCount(buildingSO.name)).ToString();
        return body + dynamicBody;
    }

    internal string GetHeader()
    {
        return buildingSO.name;
    }
}

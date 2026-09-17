using System;
using UnityEditor;
using UnityEngine;

public class TooltipBuildingData 
{ 
    [SerializeField]
    private BuildingScriptableObject buildingSO;
    //[SerializeField]
    //private EconomyManager economyManager;
    private string body = "";
    public int happinessCount, controlCount, peopleCount, cost;
    public TooltipBuildingData(BuildingScriptableObject buildingSO)
    {
        this.buildingSO = buildingSO;
        body = buildingSO.description + "\n";
        body += "Happiness: " + buildingSO.hapiness + "\n";
        body += "Control: " + buildingSO.control + "\n";
        body += "Building Cost: " + buildingSO.buildCost + "\n";

        happinessCount = buildingSO.happinessSymbolCount;
        controlCount = buildingSO.controlSymbolCount;
        peopleCount = (int)buildingSO.population;
        cost = (int)buildingSO.buildCost;
    }

    internal string GetBody()
    {
        //string dynamicBody = buildingSO.name + "s left: " + (buildingSO.maxPlacements - economyManager.GetBuildingCount(buildingSO.name)).ToString();
        return body;
    }

    internal string GetHeader()
    {
        return buildingSO.name;
    }
}

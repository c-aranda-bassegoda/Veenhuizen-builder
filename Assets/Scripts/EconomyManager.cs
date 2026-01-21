using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum GameTip
{
    MorePeople,
    MoreHappy,
    MoreControl,
    MoreFarms,
    NoTip

}
public class EconomyManager : MonoBehaviour
{
    private Dictionary<string, int> buildingCount;
    public int GetBuildingCount(string name) 
    { 
        if (buildingCount.ContainsKey(name)) return buildingCount[name]; 
        else { Debug.Log("No building " + name); return -1; } 
    }

    public float happy, control, money, extraMoneyEachSeason, food, ppl, morality;
    [SerializeField] private bool timePaused = false;
    //[SerializeField] private List<BuildingScriptableObject> buildings;
    [SerializeField] private List<BuildingScriptableObject> placedBuildingsSOs;
    [SerializeField] private List<PlacedObject> connectedObjects;
    [SerializeField] private Dictionary<string, int> maxCount;
    [SerializeField] float secondsPerSeason;
    float timePassedInSeconds;
    //[SerializeField] int daysPerSeason;
    [SerializeField] GameObject floatingTextPrefab;
    [SerializeField] GameObject controlSymbolPrefab;
    [SerializeField] GameObject happySymbolPrefab;
    [SerializeField] SeasonManager seasonManager;
    [SerializeField] public Slider moralitySlider;

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

    public GameTip GetGameTip()
    {
        if((happy >= ppl) && (control >= ppl))
        {
            if (NPCManager.instance.totalWorkingPeople < ppl)
            {
                return GameTip.MoreFarms;
            }
            else return GameTip.MorePeople;
        }

        //More people than happiness, less happiness than control
        if (happy < ppl)
        {
            if(happy <= control)
            {
                return GameTip.MoreHappy;
            }
        }

        //More people than control, less control than happiness
        if(control < ppl)
        {
            return GameTip.MoreControl;
        }

        return GameTip.NoTip;
    }

    IEnumerator Economy()
    {
        seasonsPassed = 0;
        timePassedInSeconds = 0;

        while(true)
        {
            while (timePaused)
                yield return null;

            timePassedInSeconds += Time.deltaTime;

            if(timePassedInSeconds >= secondsPerSeason)
            {
                timePassedInSeconds = 0;
                AdvanceSeason();
                seasonManager.ChangeSeason(currentSeason);
                PauseTime();

                if (seasonsPassed >= 4)
                {
                    GameEvents.OnGameFinished?.Invoke();
                    break;
                }
                else
                {
                    //CursorManager.instance.ChangeCursorTexture1();
                    money += extraMoneyEachSeason;
                    GameEvents.OnMoneyChanged?.Invoke(money);
                    GameEvents.OnShowProgressReport?.Invoke(GetGameTip());
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

    public void SetNewMorality(float _morality)
    {
        morality = _morality;
        
        moralitySlider.maxValue = NPCManager.instance.people * 4;
        moralitySlider.value = morality;
    }

    public void HandleNewPlacedBuilding(BuildingScriptableObject newObject)
    {
        if (newObject == null)
            Debug.LogError("No new object");

        string buildingName = newObject.name;

        placedBuildingsSOs.Add(newObject);
        money -= newObject.buildCost;
        happy += newObject.hapiness;
        control += newObject.control;
        ppl += newObject.population;
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnMoneyChanged?.Invoke(money);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);
    }

    public void HandleNewConnectedBuilding(BuildingScriptableObject newObject, PlacedObject building)
    {
        connectedObjects.Add(building);
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);
    }

    public void HandleRemovedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No object");
        string buildingName = oldObject.name;

        happy -= oldObject.hapiness;
        control -= oldObject.control;
        ppl -= oldObject.population;
        connectedObjects.Remove(building);
        NPCManager.instance.ChangeValues(control, happy, ppl);
        GameEvents.OnStatsChanged?.Invoke(happy, control, ppl);

        Debug.Log($"Deleted {oldObject.name}");

        money += oldObject.buildCost;
        placedBuildingsSOs.Remove(oldObject);
        GameEvents.OnMoneyChanged?.Invoke(money);
    }

    internal void HandleDisconnectedBuilding(BuildingScriptableObject oldObject, PlacedObject building)
    {
        if (oldObject == null)
            Debug.LogError("No object");
        string buildingName = oldObject.name;
        if (!connectedObjects.Contains(building)) return;

    }

    public void SkipToNextSeason()
    {
        timePassedInSeconds = secondsPerSeason;
    }


    // Displays cost with floating text
    internal void ShowTransaction(Vector3 position, Transform transform, BuildingScriptableObject placedObject, bool paid, bool replaced, Vector3 offset, BuildingScriptableObject replacedObject = null)
    {
        float cost = placedObject.buildCost;
        if (replaced && replacedObject != null)
        {
            Debug.Log(placedObject.name + " " + replacedObject.name);
            cost = replacedObject.buildCost - cost;
        } else if (paid) cost = -cost;

        if (cost == 0) return; 

        offset += new Vector3(0.0f, 9.0f, 0.0f);
        var textGO =  Instantiate(floatingTextPrefab, position + offset, Quaternion.identity, transform);


        textGO.GetComponent<TMP_Text>().text = (cost < 0? "": "+") + cost.ToString();

        textGO.GetComponent<TMP_Text>().color = (cost < 0 ? Color.red: Color.green);
    }

    internal void ShowInfluence(Vector3 position, Transform transform, BuildingScriptableObject placedObject, bool replaced, Vector3 offset, BuildingScriptableObject replacedObject = null)
    {
        float happy = placedObject.hapiness;
        float control = placedObject.control;
        
        if (replaced && replacedObject != null)
        {
            Debug.Log(placedObject.name + " " + replacedObject.name);
            control -= replacedObject.control;
            happy -= replacedObject.hapiness;
        }
        offset += new Vector3(0, 9.0f, 0);

        if (control != 0)  
            StartCoroutine(SpawnInfluenceSymbols(position + offset, transform, (int)control, controlSymbolPrefab));


        if (happy != 0)
            StartCoroutine(SpawnInfluenceSymbols(position + offset, transform, (int)happy, happySymbolPrefab));
    }

    private IEnumerator SpawnInfluenceSymbols(Vector3 position, Transform transform, int amount, GameObject symbol)
    {
        float delay = 0.2f;
        Vector3 offset;

        for (int i = 0; i < Mathf.Abs(amount); i++)
        {
            yield return new WaitForSeconds(delay);
            offset = new Vector3(UnityEngine.Random.Range(-5, 5), 0, UnityEngine.Random.Range(-5, 5));
            var obj = Instantiate(symbol, position + offset, Quaternion.identity, transform);
            obj.GetComponent<SpriteRenderer>().color = (amount < 0 ? Color.red : Color.white);
        }
    }

}


using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] EconomyManager economyManager;
    [SerializeField] float controlPerWorkingNPC;
    [SerializeField] List<NavmeshNpc> npcList;
    List<NavmeshNpc> allNpcs = new();
    Dictionary<string, NavmeshNpc> npcMap;
    public int totalWorkingPeople;

    public static NPCManager instance;

    [SerializeField] float control, happy;
    int people;

    List<PlacedObject> placedObjects = new();

    Dictionary<PlacedObject, int> workingPeoplePerBuilding = new();

    private void Awake()
    {
        instance = this;
        npcMap = new();

        foreach(NavmeshNpc npc in npcList)
        {
            npcMap.Add(npc.name, npc);
        }
    }

    public void RegisterNpc(NavmeshNpc npc, bool _add)
    {
        if(_add) allNpcs.Add(npc);
        else allNpcs.Remove(npc);
    }

    public void RegisterBuilding(PlacedObject newBuilding, bool _add)
    {
        if(newBuilding.personAmount > 0)
        {
            if (_add) placedObjects.Insert(0, newBuilding);
            else if (placedObjects.Contains(newBuilding)) placedObjects.Remove(newBuilding);
        }
        UpdateWorkingPeople();
    }

    public NavmeshNpc GetRandomNpcOfType(string _type = null)
    {
        if (_type == null)
        {
            int i = Random.Range(0, npcList.Count);
            return npcList[i];
        }
        else if (_type == "man" || _type == "woman" || _type == "boy" || _type == "girl")
        {
            int i = Random.Range(1, 4);
            return npcMap[_type + i.ToString()];
        }
        else if (_type == "guard" || _type == "teacher")
        {
            int i = Random.Range(1, 3);
            return npcMap[_type + i.ToString()];
        }
        else if (_type == "monk")
        {
            return npcMap[_type];
        }
        throw new System.Exception($"No npc type with {_type} type");

    }

    void UpdateWorkingPeople()
    {
        int newTotalWorkingPeople = GetWorkingPplAmount();
        bool foundNpc = false;

        Debug.Log($"Actual working people: {totalWorkingPeople}");
        //Working people increased
        if(newTotalWorkingPeople > totalWorkingPeople)
        {
            while (totalWorkingPeople < newTotalWorkingPeople)
            {
                foundNpc = false;
                foreach (PlacedObject obj in placedObjects)
                {
                    int newWorkingPeople = obj.SendPeopleToWork(newTotalWorkingPeople - totalWorkingPeople);
                    Debug.Log($"Sending new people to work: {newWorkingPeople}");
                    totalWorkingPeople += newWorkingPeople;

                    if(newWorkingPeople > 0) foundNpc = true;

                    if (totalWorkingPeople == newTotalWorkingPeople) break;
                    else if (totalWorkingPeople > newTotalWorkingPeople) Debug.LogError("More people working than available");
                }
                if (!foundNpc) break;
            }
        }
        //Working people decreased
        else if (newTotalWorkingPeople < totalWorkingPeople)
        {
            while (totalWorkingPeople > newTotalWorkingPeople)
            {
                foundNpc = false;
                foreach (PlacedObject obj in placedObjects)
                {
                    int newNotWorkingPeople = obj.GetPeopleFromWork(totalWorkingPeople - newTotalWorkingPeople);
                    totalWorkingPeople -= newNotWorkingPeople;

                    if (newNotWorkingPeople > 0) foundNpc = true;

                    if (totalWorkingPeople == newTotalWorkingPeople) break;
                    else if (totalWorkingPeople < newTotalWorkingPeople) Debug.LogError("Less people working than should");
                }
                if (!foundNpc) break;
            }
        }

        GameEvents.OnWorkingChanged?.Invoke(totalWorkingPeople);
    }

    public void ChangeValues(float _control, float _happy, float _people)
    {
        control = _control;
        happy = _happy;
        people = (int)_people;
        //UpdateWorkingPeople();

        economyManager.SetNewMorality(CalculateNewMorality(control, happy));
    }

    public float CalculateNewMorality(float _control, float _happiness)
    {
        float currentControl = control, currentHappiness = happy;
        float morality = 0;

        foreach(NavmeshNpc npc in allNpcs)
        {
            bool isWorking = false;

            if(npc.destinationBuilding.name == "Akker")
            {
                isWorking = true;
            }

            if (_happiness >= totalWorkingPeople)
            {
                npc.isHappy = true;
            }
            else
            {
                int currentHappyPeople = 0;
                foreach (NavmeshNpc _npc in allNpcs)
                {
                    if (_npc.isHappy) currentHappyPeople++;
                }
                if (_happiness > currentHappyPeople)
                {
                    npc.isHappy = true;
                }
                else if (_happiness == currentHappyPeople)
                {
                    //do nothing
                }
                else
                {
                    npc.isHappy = false;
                }
            }

            npc.CheckHappiness();

            if (npc.isHappy) morality++;
            if (isWorking) morality++;
            if (npc.isHappy && isWorking) morality += 2;
        }

        return morality;
    }

    public int GetWorkingPplAmount()
    {
        int workingPeople = Mathf.Min(people, Mathf.FloorToInt(control / controlPerWorkingNPC));
        Debug.Log($"New working people: {workingPeople}");
        return workingPeople;
    }
}

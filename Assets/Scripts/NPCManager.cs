using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] EconomyManager economyManager;
    [SerializeField] float controlPerWorkingNPC;
    int totalWorkingPeople;

    public static NPCManager instance;

    [SerializeField] float control, happy;
    int people;

    List<PlacedObject> placedObjects = new();

    Dictionary<PlacedObject, int> workingPeoplePerBuilding = new();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterBuilding(PlacedObject newBuilding)
    {
        placedObjects.Insert(0, newBuilding);
        UpdateWorkingPeople();
    }

    void UpdateWorkingPeople()
    {
        int newTotalWorkingPeople = GetWorkingPplAmount();
        bool foundNpc = false;

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

                    if(newNotWorkingPeople > 0) foundNpc = true;

                    if (totalWorkingPeople == newTotalWorkingPeople) break;
                    else if (totalWorkingPeople < newTotalWorkingPeople) Debug.LogError("Less people working than should");
                }
                if (!foundNpc) break;
            }
        }

        totalWorkingPeople = newTotalWorkingPeople;
    }

    public void ChangeValues(float _control, float _happy, float _people)
    {
        control = _control;
        happy = _happy;
        people = (int)_people;

        UpdateWorkingPeople();
    }

    public int GetWorkingPplAmount()
    {
        int workingPeople = Mathf.Min(people, Mathf.FloorToInt(control / controlPerWorkingNPC));
        Debug.Log($"New working people: {workingPeople}");
        return workingPeople;
    }
}

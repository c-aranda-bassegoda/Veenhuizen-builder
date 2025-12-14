using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] EconomyManager economyManager;
    [SerializeField] float controlPerWorkingNPC;
    int totalWorkingPeople;

    public static NPCManager instance;

    float control, happy;
    int people;

    List<PlacedObject> placedObjects;

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

        //Working people increased
        if(newTotalWorkingPeople > totalWorkingPeople)
        {
            while (totalWorkingPeople < newTotalWorkingPeople)
            {
                foreach (PlacedObject obj in placedObjects)
                {
                    totalWorkingPeople += obj.SendPeopleToWork(newTotalWorkingPeople - totalWorkingPeople);

                    if (totalWorkingPeople == newTotalWorkingPeople) break;
                    else if (totalWorkingPeople > newTotalWorkingPeople) Debug.LogError("More people working than available");
                }
            }
        }
        //Working people decreased
        else if (newTotalWorkingPeople < totalWorkingPeople)
        {
            while (totalWorkingPeople > newTotalWorkingPeople)
            {
                foreach (PlacedObject obj in placedObjects)
                {
                    totalWorkingPeople -= obj.GetPeopleFromWork(totalWorkingPeople - newTotalWorkingPeople);

                    if (totalWorkingPeople == newTotalWorkingPeople) break;
                    else if (totalWorkingPeople < newTotalWorkingPeople) Debug.LogError("Less people working than should");
                }
            }
        }

        totalWorkingPeople = newTotalWorkingPeople;
    }

    public void ChangeValues(float _control, float _happy, float _people)
    {
        control = _control;
        happy = _happy;
        people = (int)_people;
    }

    public int GetWorkingPplAmount()
    {
        int workingPeople = Mathf.Min(people, Mathf.FloorToInt(control / controlPerWorkingNPC));
        return workingPeople;
    }
}

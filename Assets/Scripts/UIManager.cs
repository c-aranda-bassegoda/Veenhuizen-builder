using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI peopleText;
    [SerializeField] TextMeshProUGUI happynessText;
    [SerializeField] TextMeshProUGUI workingPeopleText;

    [SerializeField] TextMeshProUGUI moneyAmountText;
    [SerializeField] TextMeshProUGUI foodAmountText;

    [Header("Calendar")]
    [SerializeField] TextMeshProUGUI dayNumberText;
    [SerializeField] TextMeshProUGUI seasonText;
    [SerializeField] TextMeshProUGUI yearNumberText;
    private int workingPpl = 0;



    private void OnEnable()
    {
        GameEvents.OnStatsChanged += UpdateStats;
        GameEvents.OnMoneyChanged += UpdateMoney;
        GameEvents.OnCalendarChanged += UpdateCalendar;
    }

    private void OnDisable()
    {
        GameEvents.OnStatsChanged -= UpdateStats;
        GameEvents.OnMoneyChanged -= UpdateMoney;
        GameEvents.OnCalendarChanged -= UpdateCalendar;
        GameEvents.OnWorkingChanged -= UpdateWorkers;
    }

    private void UpdateWorkers(int totWorking)
    {
        workingPeopleText.text = totWorking.ToString();
        workingPpl = totWorking;
    }

    public void UpdateStats(float happy, float control, float ppl)
    {
        happynessText.text = ((int)(workingPpl > happy ? happy : workingPpl)).ToString();
        //workingPeopleText.text = ((int)(ppl > control? control : ppl)).ToString();
        peopleText.text = ((int) ppl).ToString();
    }



    public void UpdateMoney(float newAmount)
    {
        moneyAmountText.text = ((int)newAmount).ToString();
    }

    public void UpdateCalendar(int dayNumber, int seasonNumber)
    {
        dayNumberText.text = dayNumber.ToString();
        switch (seasonNumber)
        {
            case 1: yearNumberText.text = "Spring"; break;
            case 2: yearNumberText.text = "Summer"; break;
            case 3: yearNumberText.text = "Autumn"; break;
            case 4: yearNumberText.text = "Winter"; break;
            default: 
                break;
        }
        //yearNumberText.text = seasonNumber.ToString();
        //if(dayNumber <= 31)
        //{
        //    dayNumberText.text = dayNumber.ToString();
        //    seasonText.text = "Spring";
        //}
        //else if (dayNumber <= 62)
        //{
        //    dayNumberText.text = (dayNumber - 31).ToString();
        //    seasonText.text = "Summer";
        //}
        //else if (dayNumber <= 93)
        //{
        //    dayNumberText.text = (dayNumber - 62).ToString();
        //    seasonText.text = "Autumn";
        //}
        //else if (dayNumber <= 124)
        //{
        //    dayNumberText.text = (dayNumber - 93).ToString();
        //    seasonText.text = "Winter";
        //}
    }

    public void UpdateFood(int foodAmount)
    {
        foodAmountText.text = foodAmount.ToString();
    }
}

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

    [SerializeField] GameObject skipToNextSeason;

    [Header("Calendar")]
    //[SerializeField] TextMeshProUGUI dayNumberText;
    //[SerializeField] TextMeshProUGUI seasonText;
    //[SerializeField] TextMeshProUGUI yearNumberText;
    [SerializeField] Transform sliderPin;
    private int workingPpl = 0;
    float _timePassed, _timePerSeason;
    Season _season;



    private void OnEnable()
    {
        GameEvents.OnStatsChanged += UpdateStats;
        GameEvents.OnMoneyChanged += UpdateMoney;
        GameEvents.OnCalendarChanged += UpdateCalendar;
        GameEvents.OnWorkingChanged += UpdateWorkers;
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

        if(skipToNextSeason != null)
        {
            if((newAmount < 70000) && (_season != Season.Spring || _timePassed >= (_timePerSeason / 2)))
            {
                skipToNextSeason.SetActive(true);
            }
            else
            {
                skipToNextSeason.SetActive(false);
            }
        }
    }

    public void UpdateCalendar(Season season, float timePassed, float timePerSeason)
    {
        //between 90 and -90
        _timePassed = timePassed;
        _timePerSeason = timePerSeason;
        _season = season;

        float pinAngle = 90f;

        float seasonDegrees = 0f;

        switch(season)
        {
            case Season.Spring:
                seasonDegrees = 0f;
            break;
            case Season.Summer:
                seasonDegrees = 45;
            break;
            case Season.Autumn:
                seasonDegrees = 90;
            break;
            case Season.Winter:
                seasonDegrees = 135f;
            break;
        }

        pinAngle -= seasonDegrees;

        float secondsDegrees = 0;

        if(timePassed != 0)
        {
            secondsDegrees = (timePassed / timePerSeason) * 45;
        }

        pinAngle -= secondsDegrees;

        sliderPin.transform.rotation = Quaternion.Euler(0f, 0f, pinAngle);

    }

    public void UpdateFood(int foodAmount)
    {
        foodAmountText.text = foodAmount.ToString();
    }
}

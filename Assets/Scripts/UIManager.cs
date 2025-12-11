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
    }

    public void UpdateStats(float happy, float control, float ppl)
    {
        happynessText.text = ((int)happy).ToString();
        workingPeopleText.text = ((int)(ppl > control? control : ppl)).ToString();
        peopleText.text = ((int) ppl).ToString();
    }



    public void UpdateMoney(float newAmount)
    {
        moneyAmountText.text = ((int)newAmount).ToString();
    }

    public void UpdateCalendar(int dayNumber, int yearNumber)
    {
        yearNumberText.text = yearNumber.ToString();
        if(dayNumber <= 31)
        {
            dayNumberText.text = dayNumber.ToString();
            seasonText.text = "Spring";
        }
        else if (dayNumber <= 62)
        {
            dayNumberText.text = (dayNumber - 31).ToString();
            seasonText.text = "Summer";
        }
        else if (dayNumber <= 93)
        {
            dayNumberText.text = (dayNumber - 62).ToString();
            seasonText.text = "Autumn";
        }
        else if (dayNumber <= 124)
        {
            dayNumberText.text = (dayNumber - 93).ToString();
            seasonText.text = "Winter";
        }
    }

    public void UpdateFood(int foodAmount)
    {
        foodAmountText.text = foodAmount.ToString();
    }
}

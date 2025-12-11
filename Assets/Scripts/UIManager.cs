using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject happyOut, controlOut;
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
        happyOut.GetComponentInChildren<TMP_Text>().text = happy.ToString();
        controlOut.GetComponentInChildren<TMP_Text>().text = control.ToString();
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

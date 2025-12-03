using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject happyOut, controlOut;
    [SerializeField] public EconomyManager manager;
    [SerializeField] TextMeshProUGUI moneyAmountText;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateStats()
    {
        happyOut.GetComponentInChildren<TMP_Text>().text = manager.happy.ToString();
        controlOut.GetComponentInChildren<TMP_Text>().text = manager.control.ToString();
    }

    public void UpdateMoney(float newAmount)
    {
        moneyAmountText.text = newAmount.ToString();
    }

}

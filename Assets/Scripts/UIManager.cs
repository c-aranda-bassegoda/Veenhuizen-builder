using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject happyOut, controlOut;
    [SerializeField] public EconomyManager manager;

    private void Start()
    {
    }

    public void UpdateStats()
    {
        happyOut.GetComponentInChildren<TMP_Text>().text = manager.happy.ToString();
        controlOut.GetComponentInChildren<TMP_Text>().text = manager.control.ToString();
    }

}

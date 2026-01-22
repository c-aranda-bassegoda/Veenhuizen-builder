using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    public Tooltip tooltip;
    [SerializeField] GameObject happinessImage, controlImage;
    public static TooltipSystem instance;
    Dictionary<string, Vector2> tooltipSizes = new();
    private void Awake()
    {
        instance = this;
        current = this;

        tooltipSizes = new()
        {
            ["Kerk"] = new Vector2(250, 90),
            ["Wachthuisje"] = new Vector2(150, 75),
            ["Boerderij"] = new Vector2(110, 60),
            ["Akker"] = new Vector2(100, 60),
            ["Zandweg"] = new Vector2(105, 60),

            ["Gesticht"] = new Vector2(100, 105),

            ["Ziekenzaal"] = new Vector2(135, 75),
            ["Badhuis"] = new Vector2(90, 75),
            ["Opzienerswoning"] = new Vector2(200, 75),
            ["Werkplaats"] = new Vector2(145, 90),
            ["School"] = new Vector2(80, 90),
            ["Slaapzaal"] = new Vector2(120, 90),

        };


    }

    public void Show(string body, int happinessCount, int controlCount, int peopleCount, int money, string header = "")
    {
        Debug.Log("Show Tooltip");
        if(happinessCount > 0)
        {
            current.tooltip.happinessHeader.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < happinessCount; i++) Instantiate(happinessImage, current.tooltip.happinessHeader.transform);
        }
        else current.tooltip.happinessHeader.transform.parent.gameObject.SetActive(false);

        if (controlCount > 0)
        {
            current.tooltip.controlHeader.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < controlCount; i++) Instantiate(controlImage, current.tooltip.controlHeader.transform);
        }
        else current.tooltip.controlHeader.transform.parent.gameObject.SetActive(false);

        if (peopleCount > 0)
        {
            current.tooltip.peopleText.text = peopleCount.ToString();
            current.tooltip.peopleText.gameObject.transform.parent.gameObject.SetActive(true);
        }
        else current.tooltip.peopleText.transform.parent.gameObject.SetActive(false);

        current.tooltip.moneyText.text = money.ToString();
        current.tooltip.SetText(body, header);

        current.tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = tooltipSizes[header];

        if(header == "Gesticht")
        {
            int maxValue =  Mathf.Max(happinessCount, controlCount);
            if((maxValue * 20) > tooltipSizes[header].x) current.tooltip.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(maxValue * 20, tooltipSizes[header].y);
        }

        current.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        foreach (Transform child in current.tooltip.happinessHeader.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in current.tooltip.controlHeader.transform)
        {
            Destroy(child.gameObject);
        }
        current.tooltip.gameObject.SetActive(false);
    }
}

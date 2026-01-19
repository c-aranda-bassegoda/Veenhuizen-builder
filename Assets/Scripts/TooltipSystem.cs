using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    public Tooltip tooltip;
    [SerializeField] GameObject happinessImage, controlImage;
    public static TooltipSystem instance;
    private void Awake()
    {
        instance = this;
        current = this;
    }

    public void Show(string body, int happinessCount, int controlCount, int peopleCount, string header = "")
    {
        Debug.Log("Show Tooltip");
        for (int i = 0; i < happinessCount; i++) Instantiate(happinessImage, current.tooltip.happinessHeader.transform);
        for (int i = 0; i < controlCount; i++) Instantiate(controlImage, current.tooltip.controlHeader.transform);

        if(peopleCount > 0)
        {
            current.tooltip.peopleText.text = peopleCount.ToString();
            current.tooltip.peopleText.gameObject.SetActive(true);
        }
        else current.tooltip.peopleText.gameObject.SetActive(false);

        current.tooltip.SetText(body, header);
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

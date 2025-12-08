using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    public Tooltip tooltip;
    private void Awake()
    {
        current = this;
    }

    public static void Show(string body, string header = "")
    {
        current.tooltip.SetText(body, header);
        current.tooltip.gameObject.SetActive(true);
    }
    public static void Hide()
    {
        current.tooltip.gameObject.SetActive(false);
    }
}

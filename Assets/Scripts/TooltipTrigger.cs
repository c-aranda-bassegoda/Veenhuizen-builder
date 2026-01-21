using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Coroutine hoverCoroutine;
    [SerializeField] public float delay = 0.5f;
    public string header;
    [TextArea(4, 10)]
    public string body;

    [SerializeField]
    public TooltipBuildingData buildingData = null;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
            return;

        hoverCoroutine = StartCoroutine(ExecuteAfterDelay());
        Debug.Log($"Hovering over: {gameObject.name}");
    }

    private IEnumerator ExecuteAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if(buildingData != null)
        {
            body = buildingData.GetBody();
            header = buildingData.GetHeader();
        }
        TooltipSystem.instance.Show(body, buildingData.happinessCount, buildingData.controlCount, buildingData.peopleCount, buildingData.cost, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("pointer exit");
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        TooltipSystem.Hide();
    }
    
}

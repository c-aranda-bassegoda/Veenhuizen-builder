using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] UIController uiController;
    [SerializeField] BuildingScriptableObject buildingSO;
    [SerializeField] TooltipTrigger tooltipTrigger;
    Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);

        // Create tooltip data from the buildingSO and assign it
        if (buildingSO != null && tooltipTrigger != null)
        {
            tooltipTrigger.buildingData = new TooltipBuildingData(buildingSO);
        }
    }

    // Update is called once per frame
    public void OnButtonClick()
    {
        SoundFXManager.Instance.PlaySoundFXClip(uiController.clickSound, transform, 1f);
        uiController.rotateButton.interactable = true;
        uiController.ResetButtonColor();
        uiController.ModifyOutline(button);
        if (button.transform.childCount > 0)
        {
            Transform layout = button.transform.GetChild(0);
            uiController.HideSidebarFold(layout, button.transform);
        }
        uiController.OnPlaceBuilding?.Invoke(buildingSO);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] UIController uiController;
    [SerializeField] BuildingScriptableObject buildingSO;
    TooltipTrigger tooltipTrigger;
    Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tooltipTrigger = GetComponent<TooltipTrigger>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            uiController.cursorManager.ChangeCursorTexture1();
            OnButtonClick();
            });

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
        
        //Transform layout = button.transform.GetChild(0);
        uiController.HideOtherSidebars(button.transform);
        
        uiController.OnPlaceBuilding?.Invoke(buildingSO);
    }
}

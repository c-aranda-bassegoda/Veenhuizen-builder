using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] UIController uiController;
    [SerializeField] BuildingScriptableObject buildingSO;
    Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    public void OnButtonClick()
    {
        SoundFXManager.Instance.PlaySoundFXClip(uiController.clickSound, transform, 1f);
        uiController.rotateButton.interactable = true;
        uiController.ResetButtonColor();
        uiController.ModifyOutline(button);
        uiController.OnPlaceBuilding?.Invoke(buildingSO);
    }
}

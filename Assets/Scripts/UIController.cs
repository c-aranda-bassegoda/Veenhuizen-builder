using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action<BuildingScriptableObject> OnPlaceBuilding;
    public Action OnDelete, OnRotate, OnPlaceRoad;
    public Button  deleteButton, rotateButton;
    public Button reportCloseButton;
    public GameObject panelHousing, interruptionPanel;
    public GameObject progressReport, gameOverMssg, endYearReportMsg, SeeVillageButtom, backToMenuButton;
    public GameObject errorPopUp;
    public GameObject buildMenu;
    public GameObject helpMenu;
    public Button errorCloseButton; 
    public TextMeshProUGUI errorMessage;
    public CursorManager cursorManager;
    [SerializeField] TextMeshProUGUI prWorkingText;
    [SerializeField] Slider prMoralitySlider;
    [SerializeField] EconomyManager economyManager;

    [SerializeField] List<Transform> expandMenus;


    [SerializeField] public AudioClip clickSound;

    public Color outlineColor;
    List<Button> buttons;
    List<Button> buildingButtons;
    GridBuildingSystem gridBuildingSystem;

    [SerializeField] string happinessTip, controlTip, morePeopleTip, moreFarmsTip;
    [SerializeField] TextMeshProUGUI tipText;
    [SerializeField] GameObject tipObj;

    private void Start()
    {
        gridBuildingSystem = GridBuildingSystem.instance;
        if(cursorManager != null) cursorManager.ChangeCursorTexture1();
        buttons = new List<Button> {deleteButton, rotateButton, reportCloseButton, errorCloseButton};
        buildingButtons = new List<Button>();
        foreach (Button button in buildMenu.GetComponentsInChildren<Button>())
        {
            if(button.GetComponent<BuildingButton>() != null) buildingButtons.Add(button);
        }
        rotateButton.interactable = false;
        ResetButtonColor();
        
        /*foreach (Button button in buildingButtons)
        {
            button.onClick.AddListener(() =>
            {
                ResetButtonColor();
                ModifyOutline(button);
                Transform layout = button.transform.GetChild(0);
                if(layout != null) 
                    HideSidebarFold(layout, button.transform);
            }
            );
        }*/

        deleteButton.onClick.AddListener(() =>
        {
            cursorManager.ChangeCursorTexture2();
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            rotateButton.interactable = false;
            //ResetButtonColor();
            //ModifyOutline(deleteButton);
            OnDelete?.Invoke();
            //HideHousingPanel();
        });
        rotateButton.onClick.AddListener(() =>
        {
            cursorManager.ChangeCursorTexture1();
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            OnRotate?.Invoke();
            //HideHousingPanel();
        });
        reportCloseButton.onClick.AddListener(() =>
        {
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            progressReport.SetActive(false); // or whichever panel you want to hide
            interruptionPanel.SetActive(false);
            GameEvents.OnResumeTime?.Invoke();
        });
        errorCloseButton.onClick.AddListener(() =>
        {
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            errorPopUp.SetActive(false); // or whichever panel you want to hide
            interruptionPanel.SetActive(false);
            GameEvents.OnResumeTime?.Invoke();
        });
    }

    public void Update()
    {
        if (TimelineManager.Instance != null) return;
        if(Input.GetMouseButtonDown(1))
        {
            cursorManager.ChangeCursorTexture1();
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            rotateButton.interactable = false;

            gridBuildingSystem.RemovingBuilding = false;
            gridBuildingSystem.AddingBuilding = false;
            gridBuildingSystem.PlacingRoad = false;
            gridBuildingSystem.StopPlacementPreview();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnShowProgressReport += ShowProgressReportPanel;
        GameEvents.OnErrorMessage += ShowErrorPanel;
    }


    private void OnDisable()
    {
        GameEvents.OnShowProgressReport -= ShowProgressReportPanel;
        GameEvents.OnErrorMessage -= ShowErrorPanel;
    }

    private void ShowErrorPanel(string mssg)
    {
        errorMessage.text = mssg;
        errorPopUp.SetActive(true);
        interruptionPanel.SetActive(true);

    }

    public void SkipTutorial()
    {
        SceneManager.LoadSceneAsync("GridPlacementScene");
    }

    private void ShowProgressReportPanel(GameTip tip)
    {
        cursorManager.ChangeCursorTexture1();

        prMoralitySlider.maxValue = economyManager.moralitySlider.maxValue;
        prMoralitySlider.value = economyManager.moralitySlider.value; ;

        prWorkingText.text = NPCManager.instance.totalWorkingPeople.ToString() + " / " + NPCManager.instance.people.ToString();

        tipText.gameObject.SetActive(true);
        switch(tip)
        {
            case GameTip.MorePeople:
                tipText.text = morePeopleTip;
                break;
            case GameTip.MoreHappy:
                tipText.text = happinessTip;
                break;
            case GameTip.MoreControl:
                tipText.text = controlTip;
                break;
            case GameTip.MoreFarms:
                tipText.text = moreFarmsTip;
                break;
            case GameTip.NoTip:
                tipObj.SetActive(false);
                break;
        }

        progressReport.SetActive(true);
        interruptionPanel.SetActive(true);
    }


    public void ShowEndGameReportPanel()
    {
        if(cursorManager != null) cursorManager.ChangeCursorTexture1();
        if(progressReport != null) progressReport.SetActive(true);
        if (gameOverMssg != null)
        {
            gameOverMssg.SetActive(false);
            endYearReportMsg.SetActive(true);
        }
        if(SeeVillageButtom != null) SeeVillageButtom.SetActive(true);
        if(reportCloseButton != null) reportCloseButton.gameObject.SetActive(false);
        if(interruptionPanel != null) interruptionPanel.SetActive(true);
    }

    public void ShowVillage()
    {
        progressReport.SetActive(false);
        buildMenu.SetActive(false);
        helpMenu.SetActive(false);
        backToMenuButton.SetActive(true);
        interruptionPanel.SetActive(false);
    }

    public void ModifyOutline(Button button)
    {
        //foreach(Button btn in buttons)
        //{
        //    Outline buttonOutline = btn.GetComponent<Outline>();
        //    if(buttonOutline != null)
        //        buttonOutline.effectColor = Color.black;
        //}
        //foreach (Button btn in buildingButtons)
        //{
        //    Outline buttonOutline = btn.GetComponent<Outline>();
        //    if (buttonOutline != null)
        //        buttonOutline.effectColor = Color.black;
        //}
        var outline = button.GetComponent<Outline>();
        if (outline == null) { 
            Debug.Log("no outline");
            return;
        }
        //outline.effectColor = outlineColor;
        outline.effectColor = Color.white;
        outline.enabled = true;
    }

    public void ResetButtonColor()
    {
        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<Outline>() != null)
                button.GetComponent<Outline>().enabled = false;
        }
        foreach (Button button in buildingButtons)
        {
            if (button != null && button.GetComponent<Outline>() != null)
                button.GetComponent<Outline>().enabled = false;
            if(button.transform.childCount > 0)
            {
                Transform layout = button.transform.parent.GetChild(1);
                if (layout.gameObject.activeSelf)
                {
                    //HideSidebarFold(layout, button.transform);
                }
                foreach (Button btn in layout.gameObject.GetComponentsInChildren<Button>())
                {
                    if (btn != null && btn.GetComponent<Outline>() != null)
                        btn.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void RestartTutorial()
    {
        SceneManager.LoadSceneAsync("TutorialScene");
    }

    public void HideSidebarFold(Transform layout, Transform button)
    {
        Debug.Log($"Hiding Sidebar: {layout.gameObject.name}");
        //Disable all building options
        layout.gameObject.SetActive(false);
        //Enable the expand icon
        button.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    public void HideOtherSidebars(Transform button)
    {
        foreach(Transform layout in expandMenus)
        {
            if(!button.IsChildOf(layout.transform.parent))
            {
                //Debug.Log($"Hiding sidebar {layout.gameObject.name}, {button.gameObject.name} not a child");
                HideSidebarFold(layout, layout.transform.parent.GetChild(0));
            }
        }
    }

    public void ShowSidebarFold(Transform button)
    {
        Transform layout = button.transform.parent.GetChild(1);
        //Enable the building options
        layout.gameObject.SetActive(true);

        //Disable the expand icon
        button.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));

        HideOtherSidebars(button);
    }
    public void ToggleSidebarFold(Transform button)
    {

        Transform layout = button.transform.parent.GetChild(1);

        Debug.Log($"Toggling Sidebar: {layout.gameObject.name}, {!layout.gameObject.activeSelf}");
        //menu is unfolded
        if (layout.gameObject.activeSelf)
        {
            HideSidebarFold(layout, button);
        }
        //menu is folded
        else
        {
            //Enable the building options
            layout.gameObject.SetActive(true);

            //Disable the expand icon
            button.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        }
        HideOtherSidebars(button);
    }
}

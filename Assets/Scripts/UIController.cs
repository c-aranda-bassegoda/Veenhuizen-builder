using System;
using System.Collections.Generic;
using NUnit.Framework;
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
    public GameObject progressReport;

    [SerializeField] public AudioClip clickSound;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {deleteButton, rotateButton, reportCloseButton};
        rotateButton.interactable = false;
        ResetButtonColor();
        
        deleteButton.onClick.AddListener(() =>
        {
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
            rotateButton.interactable = false;
            ResetButtonColor();
            ModifyOutline(deleteButton);
            OnDelete?.Invoke();
            //HideHousingPanel();
        });
        rotateButton.onClick.AddListener(() =>
        {
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
    }

    private void OnEnable()
    {
        GameEvents.OnShowProgressReport += ShowProgressReportPanel;
    }

    private void OnDisable()
    {
        GameEvents.OnShowProgressReport -= ShowProgressReportPanel;
    }

    private void ShowProgressReportPanel()
    {
        progressReport.SetActive(true);
        interruptionPanel.SetActive(true);
    }

    public void ModifyOutline(Button button)
    {
        foreach(Button btn in buttons)
        {
            Outline buttonOutline = button.GetComponent<Outline>();
            buttonOutline.effectColor = Color.black;
        }
        var outline = button.GetComponent<Outline>();
        if (outline == null)
            Debug.Log("no outline");
        //outline.effectColor = outlineColor;
        outline.effectColor = Color.blue;
        outline.enabled = true;
    }

    public void ResetButtonColor()
    {
        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<Outline>() != null)
                button.GetComponent<Outline>().enabled = false;
        }
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void ToggleSidebarFold(Transform button)
    {
        Transform layout = button.GetChild(0);
        //menu is unfolded
        if (layout.gameObject.activeSelf)
        {
            //Disable all building options
            layout.gameObject.SetActive(false);

            //Enable the expand icon
            button.GetChild(1).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        //menu is folded
        else
        {
            //Enable the building options
            layout.gameObject.SetActive(true);

            //Disable the expand icon
            button.GetChild(1).gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
    }
}

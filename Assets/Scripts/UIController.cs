using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action<int> OnPlaceBuilding;
    public Action OnDelete, OnRotate, OnPlaceRoad;
    public Button houseButton, farmButton, roadButton, instButton, schoolButton, deleteButton, rotateButton, housingButton, hidePanelButton;
    public GameObject panelHousing;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {housingButton, houseButton, farmButton, roadButton, instButton, schoolButton,  deleteButton, rotateButton, hidePanelButton};
        rotateButton.interactable = false;
        ResetButtonColor();
        /*
        housingButton.onClick.AddListener(() =>
        {
            ShowHousingPanel();
            ModifyOutline(housingButton);
        });
        hidePanelButton.onClick.AddListener(() =>
        {
            HideHousingPanel();
            if (houseButton.GetComponent<Outline>().enabled != true || instButton.GetComponent<Outline>().enabled != true)
                housingButton.GetComponent<Outline>().enabled = false;

        });
        */
        houseButton.onClick.AddListener(()=>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(houseButton);
            ModifyOutline(housingButton);
            OnPlaceBuilding?.Invoke(0);
            HideHousingPanel();
        });
        farmButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(farmButton);
            OnPlaceBuilding?.Invoke(1);
            HideHousingPanel();
        });
        roadButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(roadButton);
            OnPlaceRoad?.Invoke();
            HideHousingPanel();
        });
        instButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(instButton);
            //ModifyOutline(housingButton);
            OnPlaceBuilding?.Invoke(2);
            HideHousingPanel();
        });
        schoolButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(schoolButton);
            //ModifyOutline(housingButton);
            OnPlaceBuilding?.Invoke(3);
            HideHousingPanel();
        });
        deleteButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = false;
            ResetButtonColor();
            ModifyOutline(deleteButton);
            OnDelete?.Invoke();
            HideHousingPanel();
        });
        rotateButton.onClick.AddListener(() =>
        {
            OnRotate?.Invoke();
            HideHousingPanel();
        });
    }

    public void HideHousingPanel()
    {
        housingButton.gameObject.SetActive(true);
        TooltipSystem.Hide();
        panelHousing.SetActive(false);
    }

    private void ShowHousingPanel()
    {
        housingButton.gameObject.SetActive(false);
        panelHousing.SetActive(true);
    }

    private void ModifyOutline(Button button)
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

    private void ResetButtonColor()
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
}

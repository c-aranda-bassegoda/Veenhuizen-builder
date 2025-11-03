using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action<int> OnPlaceBuilding;
    public Action OnDelete, OnRotate, OnPlaceRoad;
    public Button houseButton, farmButton, roadButton, instButton, deleteButton, rotateButton, housingButton, hidePanelButton;
    public GameObject panelHousing;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {housingButton, houseButton, farmButton, roadButton, instButton,  deleteButton, rotateButton, hidePanelButton};
        rotateButton.interactable = false;

        housingButton.onClick.AddListener(() =>
        {
            ShowHousingPanel();
            ModifyOutline(housingButton);
        });
        hidePanelButton.onClick.AddListener(() =>
        {
            HideHousingPanel();
            if (houseButton.GetComponent<Outline>().enabled == false)
                housingButton.GetComponent<Outline>().enabled = false;

        });
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
            ModifyOutline(housingButton);
            OnPlaceBuilding?.Invoke(2);
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
        panelHousing.SetActive(false);
    }

    private void ShowHousingPanel()
    {
        housingButton.gameObject.SetActive(false);
        panelHousing.SetActive(true);
    }

    private void ModifyOutline(Button button)
    {
        var outline = button.GetComponent<Outline>();
        if (outline == null)
            Debug.Log("no outline");
        outline.effectColor = outlineColor;
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
}

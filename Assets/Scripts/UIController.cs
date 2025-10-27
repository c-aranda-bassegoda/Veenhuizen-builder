using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action<int> OnPlaceBuilding;
    public Action OnDelete, OnRotate;
    public Button houseButton, farmButton, instButton, deleteButton, rotateButton, housingButton, placeholder;
    public GameObject panelHousing;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {housingButton, houseButton, farmButton, instButton,  deleteButton, rotateButton, placeholder};
        rotateButton.interactable = false;

        housingButton.onClick.AddListener(() =>
        {
            ShowHousingPanel();

        });
        houseButton.onClick.AddListener(()=>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(houseButton);
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
        instButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(instButton);
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

    private void HideHousingPanel()
    {
        housingButton.gameObject.SetActive(true);
        placeholder.gameObject.SetActive(false);
        panelHousing.SetActive(false);
    }

    private void ShowHousingPanel()
    {
        housingButton.gameObject.SetActive(false);
        placeholder.gameObject.SetActive(true);
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

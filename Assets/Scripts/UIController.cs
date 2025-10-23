using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action<int> OnPlaceBuilding;
    public Action OnDelete, OnRotate;
    public Button houseButton, farmButton, instButton, deleteButton, rotateButton;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {houseButton, farmButton, instButton,  deleteButton, rotateButton};
        rotateButton.interactable = false;

        houseButton.onClick.AddListener(()=>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(houseButton);
            OnPlaceBuilding?.Invoke(0);
        });
        farmButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(farmButton);
            OnPlaceBuilding?.Invoke(1);
        });
        instButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = true;
            ResetButtonColor();
            ModifyOutline(instButton);
            OnPlaceBuilding?.Invoke(2);
        });
        deleteButton.onClick.AddListener(() =>
        {
            rotateButton.interactable = false;
            ResetButtonColor();
            ModifyOutline(deleteButton);
            OnDelete?.Invoke();
        });
        rotateButton.onClick.AddListener(() =>
        {
            OnRotate?.Invoke();
        });
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
            button.GetComponent<Outline>().enabled = false;
        }
    }
}

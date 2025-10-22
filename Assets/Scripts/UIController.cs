using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Action OnHousePlacement, OnDelete;
    public Button houseButton, deleteButton;

    public Color outlineColor;
    List<Button> buttons;

    private void Start()
    {
        buttons = new List<Button> {houseButton, deleteButton};

        houseButton.onClick.AddListener(()=>
        {
            ResetButtonColor();
            ModifyOutline(houseButton);
            OnHousePlacement?.Invoke();
        });
        deleteButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(deleteButton);
            OnDelete?.Invoke();
        });
    }

    private void ModifyOutline(Button button)
    {
        var outline = button.GetComponent<Outline>();
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

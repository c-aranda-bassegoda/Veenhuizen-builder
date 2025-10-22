using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public InputManager inputManager;

    private void Start()
    {
        inputManager.OnClicked += HandleMouseClick;
    }

    private void HandleMouseClick(Vector3 position)
    {
        Debug.Log(position);
    }
}

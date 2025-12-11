using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//TODO: Refactoring :(
public class InputManager : MonoBehaviour
{
    public event Action<Vector3> OnClicked, OnMouseHold;
    public event Action OnMouseUp, OnExit;
    private Vector2 cameraMovementVector;

    [SerializeField] Camera mainCamera;

    public Vector2 CameraMovementVector { get { return cameraMovementVector; } }

    private void Update()
    {
        CheckClickDownEvent();
        CheckClickUpEvent();
        CheckClickHoldEvent();
        CheckArrowInput();
    }

    private void CheckArrowInput()
    {
        cameraMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

    }

    private void CheckClickHoldEvent()
    {
        if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            var position = RaycastGround();
            if (position != null)
            {
                OnMouseHold?.Invoke(position.Value);
            }
        }
    }

    private Vector3? RaycastGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // y=0 plane
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return null;
    }

    private void CheckClickUpEvent()
    {
        if(Input.GetMouseButtonUp(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            OnMouseUp?.Invoke();
        }
    }

    private void CheckClickDownEvent()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            var position = RaycastGround();
            if (position != null)
            {
                OnClicked?.Invoke(position.Value);
            }
        }
    }
}

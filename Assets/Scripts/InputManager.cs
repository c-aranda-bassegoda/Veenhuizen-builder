using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//TODO: Refactoring :(
public class InputManager : MonoBehaviour
{
    public event Action<Vector3> OnClicked, OnMouseHold, OnHover;
    public event Action OnMouseUp, OnExit, OnHoverExit;
    private Vector2 cameraMovementVector;
    private Vector3? lastHoverPosition; //can be null

    [SerializeField] Camera mainCamera;
    [SerializeField] private float hoverExitDistanceThreshold = 0.05f;

    public Vector2 CameraMovementVector { get { return cameraMovementVector; } }

    private void Update()
    {
        CheckClickDownEvent();
        CheckClickUpEvent();
        CheckClickHoldEvent();
        CheckArrowInput();
        CheckHoverEvent();
    }

    private void CheckHoverEvent()
    {
        if (EventSystem.current.IsPointerOverGameObject()) //ignore UI
        {
            ClearHover();
            return;
        }

        var position = RaycastGround();
        if (position == null)
        {
            ClearHover();
            return;
        }
        // If lastHoverPosition exists and we moved farther than threshold, trigger exit
        if (lastHoverPosition != null && Vector3.Distance(lastHoverPosition.Value, position.Value) > hoverExitDistanceThreshold)
        {
            OnHoverExit?.Invoke();
            lastHoverPosition = null;
        }

        if (lastHoverPosition == null)
        {
            lastHoverPosition = position;
            OnHover?.Invoke(lastHoverPosition.Value);
        }
    }
    private void ClearHover()
    {
        if (lastHoverPosition == null) return;

        lastHoverPosition = null;
        OnHoverExit?.Invoke();
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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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

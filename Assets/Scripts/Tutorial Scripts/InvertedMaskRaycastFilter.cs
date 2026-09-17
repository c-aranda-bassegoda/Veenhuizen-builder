using UnityEngine;

public class InvertedMaskRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform holeRect;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // Block clicks outside the hole
        return !RectTransformUtility.RectangleContainsScreenPoint(
            holeRect,
            sp,
            eventCamera
        );
    }
}

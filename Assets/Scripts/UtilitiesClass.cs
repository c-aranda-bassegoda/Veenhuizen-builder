using UnityEngine;

public class UtilitiesClass
{
    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, TextAnchor textAnchor = TextAnchor.MiddleCenter)
    {
        GameObject gameObj = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObj.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(90,0,0); // on xz-plane
        TextMesh textMesh = gameObj.GetComponent<TextMesh>();
        textMesh.fontSize = fontSize;
        textMesh.text = text;
        textMesh.color = Color.green;
        textMesh.anchor = textAnchor;

        return textMesh;
    }

    public static Vector3? GetMouseWorldPositionXZ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return null;
    }
}

using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture1;
    [SerializeField] private Texture2D cursorTexture2;

    private Texture2D previousTexture;
    private Vector2 previousHotspot;

    private Vector2 cursorHotspot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousTexture = cursorTexture1;
        previousHotspot = new Vector2(0, 0);
    }

    public void ChangeCursorTexture2()
    {
        SaveCurrentCursor();
        cursorHotspot = new Vector2(cursorTexture2.width/2, cursorTexture2.height/2);
        SetCursor(cursorTexture2, cursorHotspot, CursorMode.Auto);
    }

    public void ChangeCursorTexture1()
    {
        SaveCurrentCursor();
        cursorHotspot = new Vector2(0, 0);
        SetCursor(cursorTexture1, cursorHotspot, CursorMode.Auto);
    }

    public void RestorePreviousCursor()
    {
        Cursor.SetCursor(previousTexture, previousHotspot, CursorMode.Auto);
    }

    private void SaveCurrentCursor()
    {
        previousTexture = currentTexture;
        previousHotspot = currentHotspot;
    }

    // Track current cursor
    private Texture2D currentTexture;
    private Vector2 currentHotspot;

    private void SetCursor(Texture2D texture, Vector2 hotspot, CursorMode mode)
    {
        Cursor.SetCursor(texture, hotspot, mode);

        currentTexture = texture;
        currentHotspot = hotspot;
    }
}

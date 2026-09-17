using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture1;
    [SerializeField] private Texture2D cursorTexture2;

    public static CursorManager instance;

    private Texture2D previousTexture;
    private Vector2 previousHotspot;

    private Vector2 cursorHotspot;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeCursorTexture1();
        SaveCurrentCursor();
        previousTexture = cursorTexture1;
        previousHotspot = new Vector2(0, 0);
    }

    public void ChangeCursorTexture2()
    {
        SaveCurrentCursor();
        cursorHotspot = new Vector2(cursorTexture2.width/2, cursorTexture2.height/2);
        SetCursorTexture(cursorTexture2, cursorHotspot, CursorMode.Auto);
    }

    public void ChangeCursorTexture1()
    {
        SaveCurrentCursor();
        cursorHotspot = new Vector2(0, 0);
        SetCursorTexture(cursorTexture1, cursorHotspot, CursorMode.Auto);
    }

    public void RestorePreviousCursor()
    {
        Debug.Log($"Restoring cursor to: {previousTexture.name}");
        Cursor.SetCursor(previousTexture, previousHotspot, CursorMode.Auto);
    }

    public void SaveCurrentCursor()
    {
        previousTexture = currentTexture;
        if(previousTexture != null) Debug.Log($"Saving cursor: {previousTexture.name}");
        previousHotspot = currentHotspot;
    }

    // Track current cursor
    private Texture2D currentTexture;
    private Vector2 currentHotspot;

    private void SetCursorTexture(Texture2D texture, Vector2 hotspot, CursorMode mode)
    {
        Cursor.SetCursor(texture, hotspot, mode);

        currentTexture = texture;
        currentHotspot = hotspot;
    }
}

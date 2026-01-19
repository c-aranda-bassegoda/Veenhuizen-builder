using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI bodyField;
    public LayoutElement layoutElement;
    public int characterWrapLimit;

    public GameObject happinessHeader, controlHeader;
    public TextMeshProUGUI peopleText;

    public RectTransform rectTransform;
    public float offset = 100f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetText(string body, string header = "")
    {
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }
        //bodyField.text = body;
        
        //int headerLength = headerField.text.Length;
        //int bodyLength = bodyField.text.Length;

        //layoutElement.enabled = (headerLength > characterWrapLimit /*|| bodyLength > characterWrapLimit*/);
    }
    void Update()
    {
        //if (Application.isEditor)
        //{
        //    int headerLength = headerField.text.Length;
        //    int bodyLength = bodyField.text.Length;

        //    layoutElement.enabled = (headerLength > characterWrapLimit ||  bodyLength > characterWrapLimit); 
        //}

        Vector2 position = Input.mousePosition;

        float pivotX = (position.x) / Screen.width;
        float pivotY = (position.y) / Screen.height;
        if (pivotY >= 0)
            pivotY += (offset / Screen.height);
        else 
            pivotY -= (offset / Screen.height);

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        position += new Vector2(200, 0);
        transform.position = position;
    }
}

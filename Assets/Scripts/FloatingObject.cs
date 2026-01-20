
using UnityEngine;

public class FloatingCanvasObject: MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float floatSpeed = 3f;


    private CanvasGroup canvasGroup;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //canvasGroup = GetComponent<CanvasGroup>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float fadeAmount = Time.deltaTime / lifetime;
        spriteRenderer.color -= new Color(0, 0, 0, fadeAmount);
    }
}

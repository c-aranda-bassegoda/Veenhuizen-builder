using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float floatSpeed = 3f;

    private TextMeshPro text;
    private Color originalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshPro>();
        originalColor = text.color;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float fadeAmount = Time.deltaTime / lifetime;
        text.color -= new Color(0, 0, 0, fadeAmount);
    }
}

using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class FloatingCombatText : MonoBehaviour
{
    private TMP_Text label;
    private Camera mainCamera;
    private Vector3 velocity;
    private Color startColor;
    private float lifetime;
    private float age;

    public bool IsActive => gameObject.activeSelf;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        mainCamera = Camera.main;
    }

    public void Play(Vector3 worldPosition, string text, Color color, float size, float duration)
    {
        if (label == null)
        {
            label = GetComponent<TMP_Text>();
        }

        mainCamera = Camera.main;
        transform.position = worldPosition;
        transform.rotation = ResolveCameraRotation();
        velocity = new Vector3(Random.Range(-0.35f, 0.35f), 1.45f, Random.Range(-0.18f, 0.18f));
        startColor = color;
        lifetime = Mathf.Max(0.1f, duration);
        age = 0f;

        label.text = text;
        label.color = color;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (age >= lifetime)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position += velocity * Time.deltaTime;
        transform.rotation = ResolveCameraRotation();

        float normalizedAge = Mathf.Clamp01(age / lifetime);
        Color color = startColor;
        color.a = 1f - normalizedAge;
        label.color = color;
    }

    private Quaternion ResolveCameraRotation()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        return mainCamera != null
            ? Quaternion.LookRotation(transform.position - mainCamera.transform.position)
            : Quaternion.identity;
    }
}

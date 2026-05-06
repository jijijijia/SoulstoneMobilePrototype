using UnityEngine;

public class MenuCharacterIdle : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private float floatingSpeed = 1.5f;
    [SerializeField] private float floatingAmount = 0.03f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Update()
    {
        float rotationOffset = Mathf.Sin(Time.time * rotationSpeed * 0.05f) * rotationAmount;
        float heightOffset = Mathf.Sin(Time.time * floatingSpeed) * floatingAmount;

        transform.position = startPosition + new Vector3(0f, heightOffset, 0f);
        transform.rotation = startRotation * Quaternion.Euler(0f, rotationOffset, 0f);
    }
}

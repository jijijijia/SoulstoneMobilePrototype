using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -10f);
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private bool useInitialSceneOffset = true;
    [Header("Shake")]
    [SerializeField] private float shakeDecay = 8f;

    private float shakeIntensity;

    public static CameraFollow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (useInitialSceneOffset && target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        if (shakeIntensity > 0f)
        {
            Vector2 circle = Random.insideUnitCircle * shakeIntensity;
            desiredPosition += new Vector3(circle.x, 0f, circle.y);
            shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0f, shakeDecay * Time.deltaTime);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }

    public static void Shake(float intensity)
    {
        if (Instance != null)
        {
            Instance.shakeIntensity = Mathf.Max(Instance.shakeIntensity, intensity);
        }
    }
}

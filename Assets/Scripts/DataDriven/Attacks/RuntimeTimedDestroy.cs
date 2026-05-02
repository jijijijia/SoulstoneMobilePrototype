using UnityEngine;

public class RuntimeTimedDestroy : MonoBehaviour
{
    private float lifetime = 0.2f;
    private float timer;

    public void Initialize(float duration)
    {
        lifetime = Mathf.Max(0.01f, duration);
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

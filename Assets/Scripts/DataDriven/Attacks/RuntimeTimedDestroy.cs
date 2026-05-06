using UnityEngine;

public class RuntimeTimedDestroy : MonoBehaviour, IPoolable
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
            PoolManager.Release(gameObject);
        }
    }

    public void OnTakenFromPool()
    {
        timer = 0f;
    }

    public void OnReturnedToPool()
    {
        timer = 0f;
    }
}

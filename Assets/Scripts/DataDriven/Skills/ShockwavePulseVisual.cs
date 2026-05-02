using UnityEngine;

public class ShockwavePulseVisual : MonoBehaviour, IPoolable
{
    private float lifeTimer;
    private float duration;
    private bool activePulse;

    private void Update()
    {
        if (!activePulse)
        {
            return;
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= duration)
        {
            PoolManager.Release(gameObject);
        }
    }

    public void Play(Vector3 worldPosition, float radius, float pulseDuration)
    {
        transform.position = worldPosition + Vector3.up * 0.05f;
        transform.localScale = new Vector3(radius * 2f, 0.08f, radius * 2f);
        duration = pulseDuration;
        lifeTimer = 0f;
        activePulse = true;
    }

    public void OnTakenFromPool()
    {
        lifeTimer = 0f;
        activePulse = false;
    }

    public void OnReturnedToPool()
    {
        lifeTimer = 0f;
        activePulse = false;
    }
}

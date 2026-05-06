using UnityEngine;

public class EnemyDamageZone : MonoBehaviour, IPoolable
{
    private CharacterSystem player;
    private float radius;
    private float radiusSqr;
    private float duration;
    private float tickInterval;
    private float damagePerTick;
    private StatusEffectData[] statuses;
    private float lifeTimer;
    private float tickTimer;
    private bool isActive;

    public void Initialize(CharacterSystem playerTarget, float zoneRadius, float zoneDuration, float zoneTickInterval, float damage, StatusEffectData[] appliedStatuses)
    {
        player = playerTarget;
        radius = Mathf.Max(0.05f, zoneRadius);
        radiusSqr = radius * radius;
        duration = Mathf.Max(0.05f, zoneDuration);
        tickInterval = Mathf.Max(0.05f, zoneTickInterval);
        damagePerTick = damage;
        statuses = appliedStatuses;
        lifeTimer = 0f;
        tickTimer = tickInterval;
        isActive = true;
        transform.localScale = new Vector3(radius * 2f, 0.08f, radius * 2f);
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        lifeTimer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            ApplyTick();
        }

        if (lifeTimer >= duration)
        {
            PoolManager.Release(gameObject);
        }
    }

    private void ApplyTick()
    {
        if (player == null || player.IsDead)
        {
            return;
        }

        Vector3 delta = player.transform.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > radiusSqr)
        {
            return;
        }

        player.TakeDamage(damagePerTick);
        player.StatusController?.ApplyStatuses(statuses);
    }

    public void OnTakenFromPool()
    {
        ResetState();
    }

    public void OnReturnedToPool()
    {
        ResetState();
    }

    private void ResetState()
    {
        player = null;
        statuses = null;
        lifeTimer = 0f;
        tickTimer = 0f;
        isActive = false;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class RuntimeDamageZone : MonoBehaviour, IPoolable
{
    private readonly List<EnemyAgent> enemies = new();

    private float radius;
    private float duration;
    private float tickInterval;
    private int damagePerTick;
    private StatusEffectData[] statuses;
    private float lifeTimer;
    private float tickTimer;
    private bool isActive;
    private ShockwavePulseVisual pulseVisual;

    public void Initialize(float zoneRadius, float zoneDuration, float zoneTickInterval, int damage, StatusEffectData[] appliedStatuses)
    {
        radius = Mathf.Max(0.05f, zoneRadius);
        duration = Mathf.Max(0.05f, zoneDuration);
        tickInterval = Mathf.Max(0.05f, zoneTickInterval);
        damagePerTick = Mathf.Max(0, damage);
        statuses = appliedStatuses;
        lifeTimer = 0f;
        tickTimer = tickInterval;
        isActive = true;

        transform.localScale = new Vector3(radius * 2f, 0.08f, radius * 2f);
        pulseVisual = pulseVisual != null ? pulseVisual : GetComponent<ShockwavePulseVisual>();

        if (pulseVisual != null)
        {
            pulseVisual.Play(transform.position, radius, duration);
        }
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
        EnemyRegistry.GetEnemiesInRadius(transform.position, radius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            if (damagePerTick > 0)
            {
                enemy.TakeDamage(damagePerTick);
            }

            enemy.ApplyStatuses(statuses);
        }
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
        enemies.Clear();
        statuses = null;
        lifeTimer = 0f;
        tickTimer = 0f;
        isActive = false;
    }
}

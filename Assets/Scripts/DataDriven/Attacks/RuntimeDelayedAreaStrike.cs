using System.Collections.Generic;
using UnityEngine;

public class RuntimeDelayedAreaStrike : MonoBehaviour
{
    private readonly List<EnemyAgent> enemies = new();

    private GameObject warningVisual;
    private StatusEffectData[] statuses;
    private float radius;
    private float delay;
    private int damage;
    private float timer;
    private bool resolved;

    public void Initialize(Vector3 position, float strikeRadius, float strikeDelay, int strikeDamage, StatusEffectData[] strikeStatuses, GameObject visualPrefab)
    {
        transform.position = position;
        radius = Mathf.Max(0.05f, strikeRadius);
        delay = Mathf.Max(0.01f, strikeDelay);
        damage = Mathf.Max(0, strikeDamage);
        statuses = strikeStatuses;
        timer = 0f;
        resolved = false;

        warningVisual = visualPrefab != null
            ? PoolManager.Spawn(visualPrefab, position, Quaternion.identity)
            : PoolManager.Spawn(DefaultRuntimePrefabFactory.GetShockwavePrefab(), position, Quaternion.identity);

        ShockwavePulseVisual pulse = warningVisual.GetComponent<ShockwavePulseVisual>();

        if (pulse != null)
        {
            pulse.Play(position, radius, delay);
        }
        else
        {
            warningVisual.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
        }
    }

    private void Update()
    {
        if (resolved)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer < delay)
        {
            return;
        }

        resolved = true;
        ApplyStrike();

        if (warningVisual != null)
        {
            PoolManager.Release(warningVisual);
        }

        Destroy(gameObject);
    }

    private void ApplyStrike()
    {
        EnemyRegistry.GetEnemiesInRadius(transform.position, radius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            if (damage > 0)
            {
                enemy.TakeDamage(damage);
            }

            enemy.ApplyStatuses(statuses);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class RuntimeCircularSweepHitbox : MonoBehaviour
{
    private readonly List<EnemyAgent> enemies = new();
    private readonly HashSet<EnemyAgent> hitEnemies = new();

    private Transform owner;
    private StatusEffectData[] statuses;
    private float radius;
    private float duration;
    private float arcWidth;
    private bool clockwise;
    private int damage;
    private float elapsed;
    private Vector3 startForward;

    public void Initialize(Transform ownerTransform, float sweepRadius, float sweepDuration, float sweepArcWidth, bool sweepClockwise, int sweepDamage, StatusEffectData[] sweepStatuses)
    {
        owner = ownerTransform;
        radius = Mathf.Max(0.1f, sweepRadius);
        duration = Mathf.Max(0.02f, sweepDuration);
        arcWidth = Mathf.Clamp(sweepArcWidth, 5f, 180f);
        clockwise = sweepClockwise;
        damage = Mathf.Max(0, sweepDamage);
        statuses = sweepStatuses ?? System.Array.Empty<StatusEffectData>();
        startForward = ResolveForward(owner);
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        float sweepAngle = Mathf.Lerp(-180f, 180f, progress) * (clockwise ? 1f : -1f);
        Vector3 currentDirection = Quaternion.AngleAxis(sweepAngle, Vector3.up) * startForward;

        ApplyCurrentArc(currentDirection);

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyCurrentArc(Vector3 currentDirection)
    {
        Vector3 origin = owner.position;
        EnemyRegistry.GetEnemiesInRadius(origin, radius, enemies);
        float halfArc = arcWidth * 0.5f;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null || hitEnemies.Contains(enemy))
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - origin;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude <= 0.001f || Vector3.Angle(currentDirection, toEnemy.normalized) > halfArc)
            {
                continue;
            }

            enemy.TakeDamage(damage);
            enemy.ApplyStatuses(statuses);
            hitEnemies.Add(enemy);
        }
    }

    private static Vector3 ResolveForward(Transform source)
    {
        if (source == null)
        {
            return Vector3.forward;
        }

        Vector3 forward = source.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }
}

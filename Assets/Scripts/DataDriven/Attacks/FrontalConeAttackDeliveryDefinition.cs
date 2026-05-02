using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Frontal Cone", fileName = "FrontalConeAttackDelivery")]
public class FrontalConeAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private float angle = 90f;
    [SerializeField] private float visualLifetime = 0.2f;
    [SerializeField] private float visualHeight = 0.08f;

    private readonly List<EnemyAgent> enemies = new();

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        Vector3 origin = context.Owner.transform.position;
        Vector3 forward = ResolveForward(context, targets);
        float resolvedRadius = context.SkillContext.ResolveAreaRadius(radius);
        float halfAngle = Mathf.Clamp(angle, 1f, 360f) * 0.5f;
        StatusEffectData[] statuses = payload.ToStatusArray();

        EnemyRegistry.GetEnemiesInRadius(origin, resolvedRadius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - origin;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude <= 0.001f || Vector3.Angle(forward, toEnemy.normalized) > halfAngle)
            {
                continue;
            }

            enemy.TakeDamage(payload.Damage);
            enemy.ApplyStatuses(statuses);
        }

        SpawnVisual(context, origin + forward * (resolvedRadius * 0.5f), resolvedRadius);
    }

    private static Vector3 ResolveForward(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets)
    {
        if (targets != null && targets.Count > 0)
        {
            Vector3 toTarget = targets[0].Position - context.Owner.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.001f)
            {
                return toTarget.normalized;
            }
        }

        Vector3 forward = context.Owner.transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    private void SpawnVisual(AttackRuntimeContext context, Vector3 position, float resolvedRadius)
    {
        GameObject prefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visual = PoolManager.Spawn(prefab, new Vector3(position.x, visualHeight, position.z), Quaternion.identity);
        ShockwavePulseVisual pulse = visual.GetComponent<ShockwavePulseVisual>();

        if (pulse != null)
        {
            pulse.Play(visual.transform.position, resolvedRadius * 0.5f, visualLifetime);
        }
        else
        {
            PoolManager.Release(visual);
        }
    }
}

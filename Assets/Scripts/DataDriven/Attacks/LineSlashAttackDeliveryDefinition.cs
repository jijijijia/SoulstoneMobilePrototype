using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Line Slash", fileName = "LineSlashAttackDelivery")]
public class LineSlashAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float length = 4.5f;
    [SerializeField] private float width = 1.1f;
    [SerializeField] private float forwardOffset = 2.2f;
    [SerializeField] private int strikeCount = 2;
    [SerializeField] private float lateralStep = 0.75f;
    [SerializeField] private float visualLifetime = 0.18f;
    [SerializeField] private float visualHeight = 0.1f;

    private readonly List<EnemyAgent> enemies = new();

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        Vector3 origin = context.Owner.transform.position;
        Vector3 forward = ResolveForward(context, targets);
        Vector3 right = new(forward.z, 0f, -forward.x);
        float resolvedLength = context.SkillContext.ResolveAreaRadius(length);
        float resolvedWidth = Mathf.Max(0.1f, context.SkillContext.ResolveAreaRadius(width));
        int resolvedStrikeCount = Mathf.Max(1, strikeCount);
        StatusEffectData[] statuses = payload.ToStatusArray();

        for (int i = 0; i < resolvedStrikeCount; i++)
        {
            float sideOffset = (i - (resolvedStrikeCount - 1) * 0.5f) * lateralStep;
            Vector3 center = origin + forward * forwardOffset + right * sideOffset;
            ApplyStrike(center, forward, resolvedLength, resolvedWidth, payload, statuses);
            SpawnVisual(context, center, resolvedWidth);
        }
    }

    private void ApplyStrike(Vector3 center, Vector3 forward, float resolvedLength, float resolvedWidth, AttackResolvedPayload payload, StatusEffectData[] statuses)
    {
        float searchRadius = resolvedLength * 0.5f + resolvedWidth;
        EnemyRegistry.GetEnemiesInRadius(center, searchRadius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - center;
            toEnemy.y = 0f;
            float forwardDistance = Vector3.Dot(toEnemy, forward);

            if (Mathf.Abs(forwardDistance) > resolvedLength * 0.5f)
            {
                continue;
            }

            Vector3 closestPointOnLine = center + forward * forwardDistance;
            Vector3 fromLine = enemy.transform.position - closestPointOnLine;
            fromLine.y = 0f;

            if (fromLine.sqrMagnitude > resolvedWidth * resolvedWidth)
            {
                continue;
            }

            enemy.TakeDamage(payload.Damage);
            enemy.ApplyStatuses(statuses);
        }
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

    private void SpawnVisual(AttackRuntimeContext context, Vector3 position, float resolvedWidth)
    {
        GameObject prefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visual = PoolManager.Spawn(prefab, new Vector3(position.x, visualHeight, position.z), Quaternion.identity);
        ShockwavePulseVisual pulse = visual.GetComponent<ShockwavePulseVisual>();

        if (pulse != null)
        {
            pulse.Play(visual.transform.position, resolvedWidth, visualLifetime);
        }
        else
        {
            PoolManager.Release(visual);
        }
    }
}

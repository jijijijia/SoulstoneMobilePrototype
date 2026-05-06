using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Beam", fileName = "BeamAttackDelivery")]
public class BeamAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float length = 8f;
    [SerializeField] private float width = 0.75f;
    [SerializeField] private float visualLifetime = 0.18f;
    [SerializeField] private float visualHeight = 0.2f;

    private readonly List<EnemyAgent> enemies = new();

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        Vector3 origin = context.Owner.transform.position;
        Vector3 direction = ResolveDirection(context, targets);
        float resolvedLength = Mathf.Max(0.1f, context.SkillContext.ResolveAreaRadius(length));
        float resolvedWidth = Mathf.Max(0.05f, context.SkillContext.ResolveAreaRadius(width));
        StatusEffectData[] statuses = payload.ToStatusArray();

        EnemyRegistry.GetEnemiesInRadius(origin, resolvedLength, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            Vector3 toEnemy = enemy.transform.position - origin;
            toEnemy.y = 0f;
            float forwardDistance = Vector3.Dot(toEnemy, direction);

            if (forwardDistance < 0f || forwardDistance > resolvedLength)
            {
                continue;
            }

            Vector3 closestPoint = direction * forwardDistance;
            float lateralDistanceSqr = (toEnemy - closestPoint).sqrMagnitude;

            if (lateralDistanceSqr > resolvedWidth * resolvedWidth)
            {
                continue;
            }

            enemy.TakeDamage(payload.Damage);
            enemy.ApplyStatuses(statuses);
        }

        SpawnVisual(origin, direction, resolvedLength, resolvedWidth);
    }

    private static Vector3 ResolveDirection(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets)
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

    private void SpawnVisual(Vector3 origin, Vector3 direction, float resolvedLength, float resolvedWidth)
    {
        GameObject visual = PoolManager.Spawn(DefaultRuntimePrefabFactory.GetBeamVisualPrefab(), origin, Quaternion.identity);
        visual.name = "RuntimeBeamVisual";
        visual.transform.position = origin + direction * (resolvedLength * 0.5f) + Vector3.up * visualHeight;
        visual.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        visual.transform.localScale = new Vector3(resolvedWidth * 2f, 0.04f, resolvedLength);

        RuntimeTimedDestroy destroyer = visual.GetComponent<RuntimeTimedDestroy>();

        if (destroyer == null)
        {
            destroyer = visual.AddComponent<RuntimeTimedDestroy>();
            PoolManager.MarkPoolableCacheDirty(visual);
        }

        destroyer.Initialize(visualLifetime);
    }
}

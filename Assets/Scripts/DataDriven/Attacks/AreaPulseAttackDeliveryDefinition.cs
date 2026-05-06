using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Area Pulse", fileName = "AreaPulseAttackDelivery")]
public class AreaPulseAttackDeliveryDefinition : AttackDeliveryDefinition
{
    private readonly List<EnemyAgent> enemies = new();

    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float visualLifetime = 0.35f;
    [SerializeField] private float visualHeight = 0.1f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context == null || targets == null || targets.Count == 0)
        {
            return;
        }

        float resolvedRadius = context.SkillContext.ResolveAreaRadius(radius);
        StatusEffectData[] statuses = payload.ToStatusArray();

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 center = targets[i].Position;
            center.y = visualHeight;

            SpawnVisual(context, center, resolvedRadius);

            enemies.Clear();
            EnemyRegistry.GetEnemiesInRadius(targets[i].Position, resolvedRadius, enemies);

            foreach (EnemyAgent enemy in enemies)
            {
                enemy.TakeDamage(payload.Damage);
                enemy.ApplyStatuses(statuses);
            }
        }
    }

    private void SpawnVisual(AttackRuntimeContext context, Vector3 position, float resolvedRadius)
    {
        GameObject prefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visual = PoolManager.Spawn(prefab, position, Quaternion.identity);
        ShockwavePulseVisual pulse = visual.GetComponent<ShockwavePulseVisual>();

        if (pulse != null)
        {
            pulse.Play(position, resolvedRadius, visualLifetime);
        }
        else
        {
            PoolManager.Release(visual);
        }
    }
}

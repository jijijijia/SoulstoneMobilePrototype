using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Circular Melee", fileName = "CircularMeleeAttackDelivery")]
public class CircularMeleeAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 2.8f;
    [SerializeField] private bool spawnPulseVisual;
    [SerializeField] private float visualLifetime = 0.12f;
    [SerializeField] private float visualHeight = 0.1f;

    private readonly List<EnemyAgent> enemies = new();

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        Vector3 origin = context.Owner.transform.position;
        float resolvedRadius = context.SkillContext.ResolveAreaRadius(radius);
        StatusEffectData[] statuses = payload.ToStatusArray();

        EnemyRegistry.GetEnemiesInRadius(origin, resolvedRadius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null)
            {
                continue;
            }

            enemy.TakeDamage(payload.Damage);
            enemy.ApplyStatuses(statuses);
        }

        if (spawnPulseVisual)
        {
            SpawnVisual(context, origin, resolvedRadius);
        }
    }

    private void SpawnVisual(AttackRuntimeContext context, Vector3 origin, float resolvedRadius)
    {
        GameObject prefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        Vector3 position = new(origin.x, visualHeight, origin.z);
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

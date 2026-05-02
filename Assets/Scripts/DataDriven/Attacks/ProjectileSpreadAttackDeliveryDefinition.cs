using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Projectile Spread", fileName = "ProjectileSpreadAttackDelivery")]
public class ProjectileSpreadAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float spreadAngle = 45f;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 2.5f;
    [SerializeField] private float spawnHeight = 0.75f;
    [Header("Advanced Projectile Behaviour")]
    [SerializeField] private int pierceCount;
    [SerializeField] private int ricochetCount;
    [SerializeField] private float ricochetSearchRadius = 6f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        Vector3 baseDirection = ResolveBaseDirection(context, targets);
        float speed = context.SkillContext.ApplySkillModifiers(
            projectileSpeed + context.SkillContext.OwnerStats.GetValue(StatType.ProjectileSpeed),
            StatType.ProjectileSpeed);

        float lifetime = context.SkillContext.ApplySkillModifiers(
            projectileLifetime + context.SkillContext.OwnerStats.GetValue(StatType.ProjectileLifetime),
            StatType.ProjectileLifetime);

        int count = Mathf.Max(1, projectileCount);
        float totalSpread = Mathf.Max(0f, spreadAngle);
        float startAngle = count == 1 ? 0f : -totalSpread * 0.5f;
        float angleStep = count == 1 ? 0f : totalSpread / (count - 1);
        Vector3 spawnPosition = context.Owner.transform.position + Vector3.up * spawnHeight;
        StatusEffectData[] statuses = payload.ToStatusArray();

        GameObject projectilePrefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetModularProjectilePrefab();

        for (int i = 0; i < count; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(startAngle + angleStep * i, Vector3.up);
            Vector3 direction = rotation * baseDirection;
            GameObject projectileObject = PoolManager.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
            ModularProjectile projectile = projectileObject.GetComponent<ModularProjectile>();

            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<ModularProjectile>();
            }

            projectile.Initialize(direction, speed, lifetime, payload.Damage, statuses, pierceCount, ricochetCount, ricochetSearchRadius);
        }
    }

    private static Vector3 ResolveBaseDirection(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets)
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
}

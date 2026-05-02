using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Projectile", fileName = "ProjectileAttackDelivery")]
public class ProjectileAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 2.5f;
    [SerializeField] private float spawnHeight = 0.75f;
    [Header("Advanced Projectile Behaviour")]
    [SerializeField] private int pierceCount;
    [SerializeField] private int ricochetCount;
    [SerializeField] private float ricochetSearchRadius = 6f;

    public override void Deliver(AttackRuntimeContext context, System.Collections.Generic.IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || targets == null || targets.Count == 0)
        {
            return;
        }

        float speed = context.SkillContext.ApplySkillModifiers(
            projectileSpeed + context.SkillContext.OwnerStats.GetValue(StatType.ProjectileSpeed),
            StatType.ProjectileSpeed);

        float lifetime = context.SkillContext.ApplySkillModifiers(
            projectileLifetime + context.SkillContext.OwnerStats.GetValue(StatType.ProjectileLifetime),
            StatType.ProjectileLifetime);

        GameObject projectilePrefab = context.SkillContext.SkillData.VisualPrefab != null
            ? context.SkillContext.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetModularProjectilePrefab();
        StatusEffectData[] statuses = payload.ToStatusArray();

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 spawnPosition = context.Owner.transform.position + Vector3.up * spawnHeight;
            GameObject projectileObject = PoolManager.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
            Vector3 targetPosition = targets[i].Position;
            targetPosition.y = spawnPosition.y;
            Vector3 direction = (targetPosition - spawnPosition).normalized;

            ModularProjectile projectile = projectileObject.GetComponent<ModularProjectile>();

            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<ModularProjectile>();
            }

            projectile.Initialize(direction, speed, lifetime, payload.Damage, statuses, pierceCount, ricochetCount, ricochetSearchRadius);
        }
    }
}

using UnityEngine;

public class DataDrivenProjectileSkill : SkillBehaviourBase
{
    private float attackInterval;
    private float attackRange;
    private float projectileSpeed;
    private float projectileLifetime;
    private float timer;

    private void Update()
    {
        if (Context == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer < attackInterval)
        {
            return;
        }

        EnemyAgent target = FindClosestEnemy();

        if (target == null)
        {
            return;
        }

        timer = 0f;
        PlayOwnerAttackVisual();

        int executionCount = Context.GetExecutionCount();

        for (int i = 0; i < executionCount; i++)
        {
            EnemyAgent resolvedTarget = i == 0 ? target : FindClosestEnemy();

            if (resolvedTarget == null)
            {
                continue;
            }

            int projectileDamage = Context.ResolveDamage(Context.SkillData.GetParameter("damage", 20f), 6f, Rank, 1f);
            SpawnProjectile(resolvedTarget.transform, projectileDamage);
        }
    }

    protected override void ApplyRank()
    {
        SkillData data = Context.SkillData;
        attackInterval = Context.ResolveCooldown(data.GetParameter("interval", 0.8f), 0.05f, Rank);
        attackRange = Context.ApplySkillModifiers(
            data.GetParameter("range", 10f) + (Rank - 1) * 0.75f + Context.OwnerStats.GetValue(StatType.AttackRange),
            StatType.AttackRange);
        projectileSpeed = Context.ApplySkillModifiers(
            data.GetParameter("projectileSpeed", 12f) + Context.OwnerStats.GetValue(StatType.ProjectileSpeed),
            StatType.ProjectileSpeed);
        projectileLifetime = Context.ApplySkillModifiers(
            data.GetParameter("projectileLifetime", 2f) + Context.OwnerStats.GetValue(StatType.ProjectileLifetime),
            StatType.ProjectileLifetime);
    }

    private EnemyAgent FindClosestEnemy()
    {
        return EnemyRegistry.GetClosestEnemy(transform.position, attackRange);
    }

    private void SpawnProjectile(Transform target, int projectileDamage)
    {
        GameObject projectilePrefab = Context.SkillData.VisualPrefab != null
            ? Context.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetProjectilePrefab();

        GameObject projectileObject = PoolManager.Spawn(
            projectilePrefab,
            transform.position + Vector3.up * 0.75f,
            Quaternion.identity);

        Vector3 targetPosition = target.position;
        targetPosition.y = projectileObject.transform.position.y;
        Vector3 direction = (targetPosition - projectileObject.transform.position).normalized;

        DataDrivenProjectile projectile = projectileObject.GetComponent<DataDrivenProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<DataDrivenProjectile>();
            PoolManager.MarkPoolableCacheDirty(projectileObject);
        }

        projectile.Initialize(direction, projectileSpeed, projectileLifetime, projectileDamage, Context.SkillData.AppliedStatuses);
    }
}

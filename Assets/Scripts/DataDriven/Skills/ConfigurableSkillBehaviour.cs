using System.Collections.Generic;
using UnityEngine;

public class ConfigurableSkillBehaviour : SkillBehaviourBase
{
    private sealed class AttackState
    {
        public ConfigurableSkillAttackData Data;
        public float Cooldown;
        public float Timer;
    }

    private readonly List<AttackState> attackStates = new();
    private readonly List<EnemyAgent> enemyBuffer = new();
    private readonly List<AttackTargetData> targetBuffer = new();

    private void Update()
    {
        if (Context?.SkillData?.ConfigurableAttacks == null || attackStates.Count == 0)
        {
            return;
        }

        for (int i = 0; i < attackStates.Count; i++)
        {
            AttackState state = attackStates[i];
            state.Timer += Time.deltaTime;

            if (state.Timer < state.Cooldown)
            {
                continue;
            }

            if (Execute(state))
            {
                state.Timer = 0f;
            }
        }
    }

    protected override void ApplyRank()
    {
        RebuildStates();
    }

    private void RebuildStates()
    {
        attackStates.Clear();

        if (Context?.SkillData?.ConfigurableAttacks == null)
        {
            return;
        }

        foreach (ConfigurableSkillAttackData attack in Context.SkillData.ConfigurableAttacks)
        {
            if (attack == null)
            {
                continue;
            }

            attackStates.Add(new AttackState
            {
                Data = attack,
                Cooldown = ResolveCooldown(attack),
                Timer = 0f
            });
        }
    }

    private float ResolveCooldown(ConfigurableSkillAttackData attack)
    {
        return Context.ResolveCooldown(attack.baseCooldown, attack.cooldownReductionPerRank, Rank);
    }

    private bool Execute(AttackState state)
    {
        int executionCount = Context.GetExecutionCount();
        bool executedAny = false;

        for (int i = 0; i < executionCount; i++)
        {
            targetBuffer.Clear();
            ResolveTargets(state.Data, targetBuffer);

            if (targetBuffer.Count == 0)
            {
                continue;
            }

            int damage = ResolveDamage(state.Data);

            switch (state.Data.deliveryMode)
            {
                case ConfigurableSkillDeliveryMode.Projectile:
                    FireProjectiles(state.Data, targetBuffer, damage);
                    break;
                case ConfigurableSkillDeliveryMode.AreaPulse:
                    FireAreaPulse(state.Data, targetBuffer, damage);
                    break;
                default:
                    return false;
            }

            executedAny = true;
        }

        return executedAny;
    }

    private int ResolveDamage(ConfigurableSkillAttackData attack)
    {
        return Context.ResolveDamage(attack.baseDamage, attack.damagePerRank, Rank, attack.ownerDamageMultiplier);
    }

    private void ResolveTargets(ConfigurableSkillAttackData attack, List<AttackTargetData> results)
    {
        float range = Context.ApplySkillModifiers(
            attack.maxDistance + Context.OwnerStats.GetValue(StatType.AttackRange),
            StatType.AttackRange);

        switch (attack.targetingMode)
        {
            case ConfigurableSkillTargetingMode.NearestEnemy:
                enemyBuffer.Clear();
                EnemyRegistry.GetClosestEnemies(transform.position, range, enemyBuffer, Mathf.Max(1, attack.targetCount));
                AppendEnemies(results, enemyBuffer);
                break;

            case ConfigurableSkillTargetingMode.RandomEnemies:
                enemyBuffer.Clear();
                EnemyRegistry.GetRandomEnemies(transform.position, range, enemyBuffer, Mathf.Max(1, attack.targetCount));
                AppendEnemies(results, enemyBuffer);
                break;

            case ConfigurableSkillTargetingMode.SelfPosition:
                results.Add(new AttackTargetData
                {
                    Enemy = null,
                    Position = transform.position
                });
                break;
        }
    }

    private static void AppendEnemies(List<AttackTargetData> results, List<EnemyAgent> enemies)
    {
        foreach (EnemyAgent enemy in enemies)
        {
            results.Add(new AttackTargetData
            {
                Enemy = enemy,
                Position = enemy.transform.position
            });
        }
    }

    private void FireProjectiles(ConfigurableSkillAttackData attack, IReadOnlyList<AttackTargetData> targets, int damage)
    {
        float projectileSpeed = Context.ApplySkillModifiers(
            attack.projectileSpeed + Context.OwnerStats.GetValue(StatType.ProjectileSpeed),
            StatType.ProjectileSpeed);
        float projectileLifetime = Context.ApplySkillModifiers(
            attack.projectileLifetime + Context.OwnerStats.GetValue(StatType.ProjectileLifetime),
            StatType.ProjectileLifetime);

        GameObject projectilePrefab = Context.SkillData.VisualPrefab != null
            ? Context.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetModularProjectilePrefab();

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * attack.spawnHeight;
            GameObject projectileObject = PoolManager.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
            Vector3 targetPosition = targets[i].Position;
            targetPosition.y = spawnPosition.y;
            Vector3 direction = (targetPosition - spawnPosition).normalized;

            ModularProjectile projectile = projectileObject.GetComponent<ModularProjectile>();

            if (projectile == null)
            {
                projectile = projectileObject.AddComponent<ModularProjectile>();
            }

            projectile.Initialize(direction, projectileSpeed, projectileLifetime, damage, Context.SkillData.AppliedStatuses);
        }
    }

    private void FireAreaPulse(ConfigurableSkillAttackData attack, IReadOnlyList<AttackTargetData> targets, int damage)
    {
        float radius = Context.ResolveAreaRadius(attack.areaRadius);

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 center = targets[i].Position;
            SpawnAreaVisual(center, radius, attack.areaVisualLifetime);
            enemyBuffer.Clear();
            EnemyRegistry.GetEnemiesInRadius(targets[i].Position, radius, enemyBuffer);

            foreach (EnemyAgent enemy in enemyBuffer)
            {
                enemy.TakeDamage(damage);
                enemy.ApplyStatuses(Context.SkillData.AppliedStatuses);
            }
        }
    }

    private void SpawnAreaVisual(Vector3 center, float radius, float duration)
    {
        GameObject visualPrefab = Context.SkillData.VisualPrefab != null
            ? Context.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visualObject = PoolManager.Spawn(visualPrefab, center, Quaternion.identity);
        ShockwavePulseVisual pulseVisual = visualObject.GetComponent<ShockwavePulseVisual>();

        if (pulseVisual == null)
        {
            pulseVisual = visualObject.AddComponent<ShockwavePulseVisual>();
        }

        pulseVisual.Play(center, radius, duration);
    }
}

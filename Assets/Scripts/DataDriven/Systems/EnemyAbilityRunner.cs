using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyAbilityRunner
{
    private readonly EnemyProjectileShooter projectileShooter;
    private readonly List<EnemyAgent> nearbyEnemiesBuffer = new();
    private readonly List<(EnemyAgent agent, string sourceId, float expiresAt)> activeBuffs = new();

    private EnemyData enemyData;
    private RuntimeStats runtimeStats;
    private SpawnSystem spawnSystem;
    private float timer;

    private bool hasPendingAction;
    private float pendingTimer;
    private float pendingDelay;
    private bool pendingIsDirectDamage;
    private Vector3 pendingCenter;
    private float pendingRadius;
    private float pendingDamage;
    private CharacterSystem pendingPlayerTarget;
    private RuntimeSummonedMinion pendingMinionTarget;

    public EnemyAbilityRunner(EnemyProjectileShooter projectileShooter)
    {
        this.projectileShooter = projectileShooter;
    }

    public EnemyAbilityDeliveryType DeliveryType => enemyData != null ? enemyData.AbilityDeliveryType : EnemyAbilityDeliveryType.MeleeContact;
    public float PreferredDistance => enemyData != null ? enemyData.PreferredDistance : 1.4f;
    public float RangeTolerance => enemyData != null ? enemyData.AbilityRangeTolerance : 0.35f;
    public bool UsesRangedRange => enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile;

    public void Initialize(EnemyData data, RuntimeStats stats, SpawnSystem ownerSpawnSystem)
    {
        enemyData = data;
        runtimeStats = stats;
        spawnSystem = ownerSpawnSystem;
        timer = 0f;
        hasPendingAction = false;
    }

    public void Reset()
    {
        enemyData = null;
        runtimeStats = null;
        spawnSystem = null;
        timer = 0f;
        hasPendingAction = false;
        pendingTimer = 0f;
        pendingRadius = 0f;
        pendingDamage = 0f;
        pendingPlayerTarget = null;
        pendingMinionTarget = null;
        nearbyEnemiesBuffer.Clear();
        activeBuffs.Clear();
    }

    public void ResetCooldown()
    {
        timer = 0f;
    }

    public void Tick()
    {
        if (hasPendingAction)
        {
            pendingTimer += Time.deltaTime;

            if (pendingTimer >= pendingDelay)
            {
                hasPendingAction = false;
                ResolvePendingAction();
            }
        }

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (Time.time >= activeBuffs[i].expiresAt)
            {
                EnemyAgent agent = activeBuffs[i].agent;

                if (agent != null && !agent.IsDead)
                {
                    agent.RemoveStatModifiers(activeBuffs[i].sourceId);
                }

                activeBuffs.RemoveAt(i);
            }
        }
    }

    public bool TickAndTryExecute(Transform source, Transform combatTarget, CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget)
    {
        if (source == null || combatTarget == null || enemyData == null || runtimeStats == null)
        {
            return false;
        }

        timer += Time.deltaTime;

        if (timer < enemyData.AbilityCooldown)
        {
            return false;
        }

        timer = 0f;
        float damage = runtimeStats.GetValue(StatType.ContactDamage) * enemyData.AbilityDamageMultiplier;

        switch (enemyData.AbilityDeliveryType)
        {
            case EnemyAbilityDeliveryType.Projectile:
            case EnemyAbilityDeliveryType.SlowProjectile:
                FireProjectile(source, combatTarget, damage);
                break;
            case EnemyAbilityDeliveryType.AreaPulse:
                ExecuteAreaPulse(source, combatTarget, playerTarget, summonTarget, damage);
                break;
            case EnemyAbilityDeliveryType.DashStrike:
                ExecuteDashStrike(source, combatTarget, playerTarget, summonTarget, damage);
                break;
            case EnemyAbilityDeliveryType.DelayedAreaMarker:
                ExecuteDelayedAreaMarker(source, combatTarget, playerTarget, summonTarget, damage);
                break;
            case EnemyAbilityDeliveryType.LastingPoisonPool:
                ExecuteLastingPoisonPool(source, combatTarget, playerTarget, damage);
                break;
            case EnemyAbilityDeliveryType.SummonMinions:
                ExecuteSummonMinions(source);
                break;
            case EnemyAbilityDeliveryType.AllyBuff:
                ExecuteAllyBuff(source);
                break;
            default:
                DealDamageToCombatTarget(playerTarget, summonTarget, damage);
                break;
        }

        return true;
    }

    public void ExecuteDeathExplosion(Transform source, CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget, float damage)
    {
        if (enemyData == null || source == null)
        {
            return;
        }

        ExecuteAreaPulse(source, source, playerTarget, summonTarget, damage);
    }

    private void ExecuteDashStrike(Transform source, Transform combatTarget, CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget, float damage)
    {
        EnemyAbilityConfig config = enemyData.AbilityConfig;
        float telegraphDuration = config != null ? config.TelegraphDuration : 0.5f;

        Vector3 targetPos = combatTarget.position;
        targetPos.y = source.position.y;
        SpawnAreaVisual(targetPos, enemyData.AbilityAreaRadius * 0.5f);

        pendingIsDirectDamage = true;
        pendingPlayerTarget = playerTarget;
        pendingMinionTarget = summonTarget;
        pendingCenter = targetPos;
        pendingRadius = enemyData.AbilityAreaRadius * 0.5f;
        pendingDamage = damage;
        pendingDelay = telegraphDuration;
        pendingTimer = 0f;
        hasPendingAction = true;
    }

    private void ExecuteDelayedAreaMarker(Transform source, Transform combatTarget, CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget, float damage)
    {
        EnemyAbilityConfig config = enemyData.AbilityConfig;
        float delayDuration = config != null ? config.DelayDuration : 1.2f;

        Vector3 center = combatTarget.position;
        center.y = source.position.y;
        SpawnAreaVisual(center, enemyData.AbilityAreaRadius);

        pendingIsDirectDamage = false;
        pendingPlayerTarget = playerTarget;
        pendingMinionTarget = summonTarget;
        pendingCenter = center;
        pendingRadius = enemyData.AbilityAreaRadius;
        pendingDamage = damage;
        pendingDelay = delayDuration;
        pendingTimer = 0f;
        hasPendingAction = true;
    }

    private void ExecuteLastingPoisonPool(Transform source, Transform combatTarget, CharacterSystem playerTarget, float damage)
    {
        EnemyAbilityConfig config = enemyData.AbilityConfig;

        if (config == null)
        {
            return;
        }

        Vector3 center = combatTarget.position;
        center.y = source.position.y;

        GameObject visualPrefab = enemyData.AbilityAreaVisualPrefab != null
            ? enemyData.AbilityAreaVisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject zoneObject = PoolManager.Spawn(visualPrefab, center, Quaternion.identity);

        if (zoneObject == null)
        {
            return;
        }

        EnemyDamageZone zone = zoneObject.GetComponent<EnemyDamageZone>();

        if (zone == null)
        {
            zone = zoneObject.AddComponent<EnemyDamageZone>();
            PoolManager.MarkPoolableCacheDirty(zoneObject);
        }

        zone.Initialize(playerTarget, config.AreaRadius, config.PoolDuration, config.PoolTickInterval, damage, enemyData.AbilityStatuses);
    }

    private void ExecuteSummonMinions(Transform source)
    {
        EnemyAbilityConfig config = enemyData.AbilityConfig;

        if (config == null || config.SummonEnemyData == null || spawnSystem == null || source == null)
        {
            return;
        }

        int count = config.SummonCount;
        float spawnRadius = Mathf.Max(1.5f, config.AreaRadius);

        for (int i = 0; i < count; i++)
        {
            spawnSystem.SpawnSpecificEnemy(config.SummonEnemyData, source.position, spawnRadius);
        }
    }

    private void ExecuteAllyBuff(Transform source)
    {
        EnemyAbilityConfig config = enemyData.AbilityConfig;

        if (config == null)
        {
            return;
        }

        EnemyRegistry.GetEnemiesInRadius(source.position, config.BuffRadius, nearbyEnemiesBuffer);

        StatModifierData[] mods = new[]
        {
            new StatModifierData
            {
                statType = config.BuffStatType,
                additive = 0f,
                multiplier = config.BuffMultiplier
            }
        };

        float expiresAt = Time.time + config.BuffDuration;

        for (int i = 0; i < nearbyEnemiesBuffer.Count; i++)
        {
            EnemyAgent ally = nearbyEnemiesBuffer[i];

            if (ally == null || ally.IsDead)
            {
                continue;
            }

            string sourceId = GetBuffSourceId(source, ally, config);
            RemoveTrackedBuff(sourceId);
            ally.AddStatModifiers(sourceId, mods);
            activeBuffs.Add((ally, sourceId, expiresAt));
        }
    }

    private void ResolvePendingAction()
    {
        if (pendingIsDirectDamage)
        {
            DealDamageToPendingTargetIfInsideRadius();
        }
        else
        {
            float radius = pendingRadius > 0f ? pendingRadius : enemyData != null ? enemyData.AbilityAreaRadius : 2.4f;
            float radiusSqr = radius * radius;

            if (pendingMinionTarget != null && pendingMinionTarget.IsAlive
                && IsInsideRadius(pendingMinionTarget.transform.position, pendingCenter, radiusSqr))
            {
                pendingMinionTarget.TakeDamage(pendingDamage);
            }
            else if (pendingPlayerTarget != null && !pendingPlayerTarget.IsDead
                && IsInsideRadius(pendingPlayerTarget.transform.position, pendingCenter, radiusSqr))
            {
                pendingPlayerTarget.TakeDamage(pendingDamage);
                pendingPlayerTarget.StatusController?.ApplyStatuses(enemyData?.AbilityStatuses);
            }
        }

        pendingPlayerTarget = null;
        pendingMinionTarget = null;
        pendingRadius = 0f;
    }

    private void DealDamageToPendingTargetIfInsideRadius()
    {
        float radius = pendingRadius > 0f ? pendingRadius : 1.2f;
        float radiusSqr = radius * radius;

        if (pendingMinionTarget != null && pendingMinionTarget.IsAlive
            && IsInsideRadius(pendingMinionTarget.transform.position, pendingCenter, radiusSqr))
        {
            pendingMinionTarget.TakeDamage(pendingDamage);
            return;
        }

        if (pendingPlayerTarget != null && !pendingPlayerTarget.IsDead
            && IsInsideRadius(pendingPlayerTarget.transform.position, pendingCenter, radiusSqr))
        {
            pendingPlayerTarget.TakeDamage(pendingDamage);
            pendingPlayerTarget.StatusController?.ApplyStatuses(enemyData?.AbilityStatuses);
        }
    }

    private void RemoveTrackedBuff(string sourceId)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].sourceId != sourceId)
            {
                continue;
            }

            EnemyAgent agent = activeBuffs[i].agent;

            if (agent != null && !agent.IsDead)
            {
                agent.RemoveStatModifiers(sourceId);
            }

            activeBuffs.RemoveAt(i);
        }
    }

    private static string GetBuffSourceId(Transform source, EnemyAgent ally, EnemyAbilityConfig config)
    {
        string abilityId = !string.IsNullOrWhiteSpace(config.AbilityId)
            ? config.AbilityId
            : config.name;

        return $"ally_buff_{source.GetInstanceID()}_{ally.GetInstanceID()}_{abilityId}";
    }

    private void FireProjectile(Transform source, Transform combatTarget, float damage)
    {
        Vector3 targetPosition = combatTarget.position;
        targetPosition.y = source.position.y;

        projectileShooter.Fire(
            source.position,
            targetPosition,
            enemyData,
            damage);
    }

    private void ExecuteAreaPulse(Transform source, Transform combatTarget, CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget, float damage)
    {
        Vector3 center = combatTarget.position;
        center.y = source.position.y;
        float radius = enemyData.AbilityAreaRadius;
        float radiusSqr = radius * radius;

        SpawnAreaVisual(center, radius);

        if (summonTarget != null && summonTarget.IsAlive && IsInsideRadius(summonTarget.transform.position, center, radiusSqr))
        {
            summonTarget.TakeDamage(damage);
            return;
        }

        if (playerTarget != null && !playerTarget.IsDead && IsInsideRadius(playerTarget.transform.position, center, radiusSqr))
        {
            playerTarget.TakeDamage(damage);
            playerTarget.StatusController?.ApplyStatuses(enemyData.AbilityStatuses);
        }
    }

    private void DealDamageToCombatTarget(CharacterSystem playerTarget, RuntimeSummonedMinion summonTarget, float damage)
    {
        if (summonTarget != null && summonTarget.IsAlive)
        {
            summonTarget.TakeDamage(damage);
            return;
        }

        playerTarget?.TakeDamage(damage);
        playerTarget?.StatusController?.ApplyStatuses(enemyData.AbilityStatuses);
    }

    private void SpawnAreaVisual(Vector3 center, float radius)
    {
        GameObject visualPrefab = enemyData.AbilityAreaVisualPrefab != null
            ? enemyData.AbilityAreaVisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visualObject = PoolManager.Spawn(visualPrefab, center, Quaternion.identity);

        if (visualObject == null)
        {
            return;
        }

        ShockwavePulseVisual pulseVisual = visualObject.GetComponent<ShockwavePulseVisual>();

        if (pulseVisual == null)
        {
            pulseVisual = visualObject.AddComponent<ShockwavePulseVisual>();
            PoolManager.MarkPoolableCacheDirty(visualObject);
        }

        pulseVisual.Play(center, radius, enemyData.AbilityAreaVisualLifetime);
    }

    private static bool IsInsideRadius(Vector3 position, Vector3 center, float radiusSqr)
    {
        Vector3 delta = position - center;
        delta.y = 0f;
        return delta.sqrMagnitude <= radiusSqr;
    }
}

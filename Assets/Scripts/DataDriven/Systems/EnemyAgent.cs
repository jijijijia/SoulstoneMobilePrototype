using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAgent : MonoBehaviour, IPoolable
{
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float meleeLaneOffsetStrength = 1.25f;
    [SerializeField] private float meleePreferredDistanceVariance = 0.35f;
    [SerializeField] private float obstacleProbeDistance = 2.5f;
    [SerializeField] private float obstacleProbeRadiusMultiplier = 0.45f;
    [SerializeField] private float obstacleProbeHeight = 0.8f;
    [SerializeField] private float summonAggroRadius = 8f;
    [SerializeField] private float summonStrongFocusDistanceRatio = 0.5f;
    [SerializeField] private float summonSoftFocusChance = 0.5f;
    [SerializeField] private float aggroReevaluationInterval = 0.45f;
    [SerializeField] private float directPlayerAggroDuration = 2f;
    [SerializeField] private float[] avoidanceAngles = { 25f, -25f, 50f, -50f, 80f, -80f, 115f, -115f };

    private CharacterController characterController;
    private RuntimeStats runtimeStats;
    private EnemyData enemyData;
    private CharacterSystem target;
    private RuntimeSummonedMinion summonTarget;
    private SpawnSystem spawnSystem;
    private float currentHealth;
    private Collider[] hitColliders;
    private StatusController statusController;
    private EnemyVisualProfile visualProfile;
    private Transform importedVisualRoot;
    private EnemyMovementController movementController;
    private CombatVisualAnimator combatVisualAnimator;
    private readonly EnemyProjectileShooter projectileShooter = new();
    private EnemyAbilityRunner abilityRunner;
    private bool isDespawning;
    private float nextAggroEvaluationTime;
    private float lastDirectPlayerDamageTime = -999f;

    public EnemyData Data => enemyData;
    public bool IsDead => isDespawning || currentHealth <= 0f;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => runtimeStats != null ? runtimeStats.GetValue(StatType.MaxHealth) : 0f;
    public StatusController StatusController => statusController;

    public void AddStatModifiers(string sourceId, StatModifierData[] modifiers) => runtimeStats?.AddModifiers(sourceId, modifiers);
    public void RemoveStatModifiers(string sourceId) => runtimeStats?.RemoveModifiers(sourceId);

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitColliders = GetComponents<Collider>();
        importedVisualRoot = transform.Find("ImportedVisual");
        visualProfile = new EnemyVisualProfile(transform, GetComponentsInChildren<Renderer>(true));
        abilityRunner = new EnemyAbilityRunner(projectileShooter);
        movementController = new EnemyMovementController(
            transform,
            characterController,
            gravity,
            meleeLaneOffsetStrength,
            meleePreferredDistanceVariance,
            obstacleProbeDistance,
            obstacleProbeRadiusMultiplier,
            obstacleProbeHeight,
            avoidanceAngles);
        combatVisualAnimator = GetComponent<CombatVisualAnimator>();

        if (combatVisualAnimator == null)
        {
            combatVisualAnimator = gameObject.AddComponent<CombatVisualAnimator>();
        }

        statusController = GetComponent<StatusController>();

        if (statusController == null)
        {
            statusController = gameObject.AddComponent<StatusController>();
        }

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == null || hitCollider == characterController)
            {
                continue;
            }

            hitCollider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        EnemyRegistry.Unregister(this);
    }

    public void Initialize(EnemyData data, CharacterSystem playerTarget, SpawnSystem ownerSpawnSystem, float healthMultiplier, float damageMultiplier)
    {
        enemyData = data;
        target = playerTarget;
        spawnSystem = ownerSpawnSystem;
        runtimeStats = new RuntimeStats();
        runtimeStats.Initialize(data.BaseStats);
        runtimeStats.SetBaseValue(StatType.MaxHealth, runtimeStats.GetValue(StatType.MaxHealth) * healthMultiplier);
        runtimeStats.SetBaseValue(StatType.ContactDamage, runtimeStats.GetValue(StatType.ContactDamage) * damageMultiplier);
        currentHealth = runtimeStats.GetValue(StatType.MaxHealth);
        isDespawning = false;
        movementController.Initialize(enemyData, runtimeStats);
        abilityRunner.Initialize(enemyData, runtimeStats, spawnSystem);
        statusController.Initialize(this, runtimeStats);
        visualProfile.Apply(enemyData);
        NormalizeImportedVisualScale();
        ConfigureVisualAnimator();
    }

    private void NormalizeImportedVisualScale()
    {
        if (importedVisualRoot == null)
        {
            importedVisualRoot = transform.Find("ImportedVisual");
        }

        if (importedVisualRoot == null)
        {
            return;
        }

        Vector3 rootScale = transform.localScale;
        float visualUniformScale = Mathf.Max(Mathf.Abs(rootScale.x), Mathf.Abs(rootScale.z), 0.01f);

        importedVisualRoot.localScale = new Vector3(
            Mathf.Approximately(rootScale.x, 0f) ? visualUniformScale : visualUniformScale / rootScale.x,
            Mathf.Approximately(rootScale.y, 0f) ? visualUniformScale : visualUniformScale / rootScale.y,
            Mathf.Approximately(rootScale.z, 0f) ? visualUniformScale : visualUniformScale / rootScale.z);
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        RefreshCombatTarget();
        abilityRunner.Tick();
        MoveToTarget();
        combatVisualAnimator?.SetMoveVelocity(movementController.LastHorizontalVelocity);
        EnemyRegistry.UpdatePosition(this);
        TryAttack();
        ClampInsideMap();
        EnemyRegistry.UpdatePosition(this);
    }

    public void TakeDamage(float damage)
    {
        TakeDamageInternal(damage, true);
    }

    public void TakeDamageFromSummon(float damage)
    {
        TakeDamageInternal(damage, false);
    }

    private void TakeDamageInternal(float damage, bool forcePlayerAggro)
    {
        if (isDespawning || enemyData == null)
        {
            return;
        }

        if (forcePlayerAggro)
        {
            lastDirectPlayerDamageTime = Time.time;
            summonTarget = null;
            nextAggroEvaluationTime = Time.time + aggroReevaluationInterval;
        }

        float resolvedDamage = ResolveIncomingDamage(damage);
        currentHealth -= resolvedDamage;
        CombatFeedbackEvents.RaiseDamageTaken(new CombatFeedbackEvent(
            transform,
            transform.position,
            resolvedDamage,
            isPlayerTarget: false));

        if (currentHealth <= 0f)
        {
            isDespawning = true;

            if (enemyData?.AbilityDeliveryType == EnemyAbilityDeliveryType.DeathExplosion)
            {
                float deathDamage = runtimeStats.GetValue(StatType.ContactDamage) * enemyData.AbilityDamageMultiplier;
                abilityRunner.ExecuteDeathExplosion(transform, target, summonTarget, deathDamage);
            }

            if (spawnSystem != null && enemyData != null)
            {
                spawnSystem.SpawnExperienceGem(transform.position, enemyData.ExperienceReward);
                spawnSystem.NotifyEnemyKilled(this);
            }
            PoolManager.Release(gameObject);
        }
    }

    public void ApplyStatuses(StatusEffectData[] statuses)
    {
        statusController?.ApplyStatuses(statuses);
    }

    private void MoveToTarget()
    {
        Transform combatTarget = GetCombatTargetTransform();
        movementController.MoveToTarget(
            combatTarget,
            target != null ? target.transform : null,
            summonTarget != null ? summonTarget.transform : null);
    }

    private void TryAttack()
    {
        Transform combatTarget = GetCombatTargetTransform();

        if (combatTarget == null)
        {
            return;
        }

        Vector3 flatTargetPosition = combatTarget.position;
        flatTargetPosition.y = transform.position.y;
        float distance = Vector3.Distance(transform.position, flatTargetPosition);

        if (abilityRunner.UsesRangedRange)
        {
            if (distance > abilityRunner.PreferredDistance + abilityRunner.RangeTolerance)
            {
                abilityRunner.ResetCooldown();
                return;
            }
        }
        else
        {
            float contactAttackDistance = movementController.GetContactAttackDistance(combatTarget);

            if (distance > contactAttackDistance)
            {
                abilityRunner.ResetCooldown();
                return;
            }
        }

        if (abilityRunner.TickAndTryExecute(transform, combatTarget, target, summonTarget))
        {
            combatVisualAnimator?.PlayAttack();
        }
    }

    private void ConfigureVisualAnimator()
    {
        if (combatVisualAnimator == null)
        {
            return;
        }

        if (importedVisualRoot == null)
        {
            importedVisualRoot = transform.Find("ImportedVisual");
        }

        combatVisualAnimator.ConfigureVisualRoot(importedVisualRoot != null ? importedVisualRoot : transform);
    }

    private void RefreshCombatTarget()
    {
        if (target == null)
        {
            summonTarget = null;
            return;
        }

        if (Time.time < nextAggroEvaluationTime && IsCurrentSummonTargetStillValid())
        {
            return;
        }

        nextAggroEvaluationTime = Time.time + aggroReevaluationInterval;
        summonTarget = ResolveSummonAggroTarget();
    }

    private RuntimeSummonedMinion ResolveSummonAggroTarget()
    {
        if (WasRecentlyDirectlyAttackedByPlayer())
        {
            return null;
        }

        RuntimeSummonedMinion closestSummon = RuntimeSummonRegistry.GetClosestSummon(transform.position, summonAggroRadius);

        if (closestSummon == null || !closestSummon.IsAlive)
        {
            return null;
        }

        float playerDistance = GetFlatDistance(target.transform.position);
        float summonDistance = GetFlatDistance(closestSummon.transform.position);

        if (playerDistance <= summonDistance)
        {
            return null;
        }

        if (summonDistance < playerDistance * Mathf.Clamp01(summonStrongFocusDistanceRatio))
        {
            return closestSummon;
        }

        return Random.value < Mathf.Clamp01(summonSoftFocusChance)
            ? closestSummon
            : null;
    }

    private bool IsCurrentSummonTargetStillValid()
    {
        if (summonTarget == null || !summonTarget.IsAlive || WasRecentlyDirectlyAttackedByPlayer())
        {
            return false;
        }

        float playerDistance = GetFlatDistance(target.transform.position);
        float summonDistance = GetFlatDistance(summonTarget.transform.position);
        return summonDistance < playerDistance && summonDistance <= summonAggroRadius;
    }

    private bool WasRecentlyDirectlyAttackedByPlayer()
    {
        return Time.time - lastDirectPlayerDamageTime <= directPlayerAggroDuration;
    }

    private float GetFlatDistance(Vector3 position)
    {
        Vector3 delta = position - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private Transform GetCombatTargetTransform()
    {
        if (summonTarget != null && summonTarget.IsAlive)
        {
            return summonTarget.transform;
        }

        return target != null ? target.transform : null;
    }

    private void ClampInsideMap()
    {
        if (spawnSystem == null)
        {
            return;
        }

        Vector3 clampedPosition = spawnSystem.ClampToMap(transform.position);

        if ((clampedPosition - transform.position).sqrMagnitude > 0.0001f)
        {
            characterController.enabled = false;
            transform.position = clampedPosition;
            characterController.enabled = true;
        }
    }

    public void OnTakenFromPool()
    {
        isDespawning = false;
    }

    public void OnReturnedToPool()
    {
        isDespawning = false;
        target = null;
        summonTarget = null;
        nextAggroEvaluationTime = 0f;
        lastDirectPlayerDamageTime = -999f;
        spawnSystem = null;
        enemyData = null;
        runtimeStats = null;
        movementController.Reset();
        abilityRunner.Reset();
        statusController?.ResetState();
        visualProfile.Reset();
    }

    private float ResolveIncomingDamage(float rawDamage)
    {
        float armor = runtimeStats != null ? runtimeStats.GetValue(StatType.Armor) : CombatMath.BaseEnemyArmor;
        return Mathf.Max(0f, rawDamage * CombatMath.ResolveArmorDamageMultiplier(armor));
    }
}

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
    [SerializeField] private float[] avoidanceAngles = { 25f, -25f, 50f, -50f, 80f, -80f, 115f, -115f };

    private CharacterController characterController;
    private RuntimeStats runtimeStats;
    private EnemyData enemyData;
    private CharacterSystem target;
    private SpawnSystem spawnSystem;
    private float currentHealth;
    private Vector3 velocity;
    private float damageTimer;
    private Collider[] hitColliders;
    private Renderer[] renderers;
    private StatusController statusController;
    private Vector3 initialScale;
    private Color[] defaultColors;
    private bool isDespawning;
    private float laneOffset;
    private float personalPreferredDistance;

    public EnemyData Data => enemyData;
    public bool IsDead => isDespawning || currentHealth <= 0f;
    public StatusController StatusController => statusController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        hitColliders = GetComponents<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);
        statusController = GetComponent<StatusController>();

        if (statusController == null)
        {
            statusController = gameObject.AddComponent<StatusController>();
        }

        initialScale = transform.localScale;
        defaultColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].sharedMaterial != null && renderers[i].sharedMaterial.HasProperty("_Color"))
            {
                defaultColors[i] = renderers[i].sharedMaterial.color;
            }
            else
            {
                defaultColors[i] = Color.white;
            }
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
        damageTimer = 0f;
        velocity = Vector3.zero;
        isDespawning = false;
        InitializeMovementProfile();
        statusController.Initialize(this, runtimeStats);
        ApplyVisualProfile();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        MoveToTarget();
        TryAttack();
        ClampInsideMap();
    }

    public void TakeDamage(float damage)
    {
        if (isDespawning || enemyData == null)
        {
            return;
        }

        currentHealth -= ResolveIncomingDamage(damage);

        if (currentHealth <= 0f)
        {
            isDespawning = true;

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
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        float desiredDistance = personalPreferredDistance > 0f
            ? personalPreferredDistance
            : (enemyData != null ? enemyData.PreferredDistance : 1.4f);

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile && distance < desiredDistance * 0.65f)
        {
            Vector3 retreatDirection = (-direction).normalized;
            Vector3 navigableRetreat = ResolveNavigableDirection(retreatDirection, distance);
            transform.forward = navigableRetreat;
            characterController.Move(navigableRetreat * runtimeStats.GetValue(StatType.MoveSpeed) * Time.deltaTime);
        }
        else if (distance > desiredDistance)
        {
            Vector3 moveDirection = GetMoveDirection(direction, distance, desiredDistance);
            Vector3 navigableDirection = ResolveNavigableDirection(moveDirection, distance - desiredDistance);
            transform.forward = navigableDirection;
            characterController.Move(navigableDirection * runtimeStats.GetValue(StatType.MoveSpeed) * Time.deltaTime);
        }

        if (characterController.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void TryAttack()
    {
        Vector3 flatTargetPosition = target.transform.position;
        flatTargetPosition.y = transform.position.y;
        float distance = Vector3.Distance(transform.position, flatTargetPosition);
        float preferredDistance = personalPreferredDistance > 0f
            ? personalPreferredDistance
            : (enemyData != null ? enemyData.PreferredDistance : 1.4f);
        float attackInterval = enemyData != null ? enemyData.AttackInterval : 1f;

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile)
        {
            if (distance > preferredDistance + 0.35f)
            {
                damageTimer = 0f;
                return;
            }
        }
        else
        {
            if (distance > preferredDistance + 0.1f)
            {
                damageTimer = 0f;
                return;
            }
        }

        damageTimer += Time.deltaTime;

        if (damageTimer < attackInterval)
        {
            return;
        }

        damageTimer = 0f;

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.RangedProjectile)
        {
            FireProjectile(flatTargetPosition);
        }
        else
        {
            target.TakeDamage(runtimeStats.GetValue(StatType.ContactDamage));
        }
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
        damageTimer = 0f;
        velocity = Vector3.zero;
        isDespawning = false;
    }

    public void OnReturnedToPool()
    {
        damageTimer = 0f;
        velocity = Vector3.zero;
        isDespawning = false;
        target = null;
        spawnSystem = null;
        enemyData = null;
        runtimeStats = null;
        personalPreferredDistance = 0f;
        laneOffset = 0f;
        statusController?.ResetState();
        ResetVisualProfile();
    }

    private void InitializeMovementProfile()
    {
        float basePreferredDistance = enemyData != null ? enemyData.PreferredDistance : 1.4f;

        if (enemyData != null && enemyData.AttackType == EnemyAttackType.MeleeContact)
        {
            laneOffset = Random.Range(-meleeLaneOffsetStrength, meleeLaneOffsetStrength);
            personalPreferredDistance = Mathf.Max(
                0.6f,
                basePreferredDistance + Random.Range(-meleePreferredDistanceVariance, meleePreferredDistanceVariance));
            return;
        }

        laneOffset = 0f;
        personalPreferredDistance = basePreferredDistance;
    }

    private Vector3 GetMoveDirection(Vector3 targetDirection, float distance, float desiredDistance)
    {
        Vector3 moveDirection = targetDirection.normalized;

        if (enemyData == null || enemyData.AttackType != EnemyAttackType.MeleeContact || Mathf.Abs(laneOffset) <= 0.01f)
        {
            return moveDirection;
        }

        Vector3 tangent = Vector3.Cross(Vector3.up, moveDirection).normalized;
        float laneWeight = Mathf.Clamp01((distance - desiredDistance) / 4f);
        Vector3 offsetTarget = target.transform.position + tangent * laneOffset * laneWeight;
        Vector3 offsetDirection = offsetTarget - transform.position;
        offsetDirection.y = 0f;

        return offsetDirection.sqrMagnitude > 0.001f
            ? offsetDirection.normalized
            : moveDirection;
    }

    private Vector3 ResolveNavigableDirection(Vector3 desiredDirection, float remainingDistance)
    {
        if (desiredDirection.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        Vector3 flattenedDirection = desiredDirection.normalized;
        float probeDistance = Mathf.Clamp(remainingDistance, 0.75f, obstacleProbeDistance);

        if (IsPathClear(flattenedDirection, probeDistance))
        {
            return flattenedDirection;
        }

        for (int i = 0; i < avoidanceAngles.Length; i++)
        {
            Vector3 candidateDirection = Quaternion.Euler(0f, avoidanceAngles[i], 0f) * flattenedDirection;

            if (IsPathClear(candidateDirection, probeDistance))
            {
                return candidateDirection.normalized;
            }
        }

        return flattenedDirection;
    }

    private bool IsPathClear(Vector3 direction, float probeDistance)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        Vector3 origin = transform.position + Vector3.up * obstacleProbeHeight;
        float probeRadius = Mathf.Max(0.1f, characterController.radius * obstacleProbeRadiusMultiplier);

        if (!Physics.SphereCast(origin, probeRadius, direction.normalized, out RaycastHit hit, probeDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        if (hit.collider == null)
        {
            return true;
        }

        Transform hitRoot = hit.collider.transform.root;

        if (hitRoot == transform.root)
        {
            return true;
        }

        if (target != null && hitRoot == target.transform.root)
        {
            return true;
        }

        return false;
    }

    private void ApplyVisualProfile()
    {
        if (enemyData == null)
        {
            return;
        }

        transform.localScale = initialScale * enemyData.VisualScaleMultiplier;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];

            if (targetRenderer == null || targetRenderer.material == null || !targetRenderer.material.HasProperty("_Color"))
            {
                continue;
            }

            targetRenderer.material.color = enemyData.TintColor;
        }
    }

    private void ResetVisualProfile()
    {
        transform.localScale = initialScale;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];

            if (targetRenderer == null || targetRenderer.material == null || !targetRenderer.material.HasProperty("_Color"))
            {
                continue;
            }

            targetRenderer.material.color = i < defaultColors.Length ? defaultColors[i] : Color.white;
        }
    }

    private void FireProjectile(Vector3 targetPosition)
    {
        GameObject projectilePrefab = DefaultRuntimePrefabFactory.GetEnemyProjectilePrefab();
        Vector3 spawnPosition = transform.position + Vector3.up * 0.8f;
        GameObject projectileObject = PoolManager.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);
        projectileObject.transform.localScale = Vector3.one * (enemyData != null ? enemyData.ProjectileScale : 0.4f);

        Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();

        if (projectileRenderer != null && projectileRenderer.material != null && projectileRenderer.material.HasProperty("_Color"))
        {
            projectileRenderer.material.color = enemyData != null ? enemyData.ProjectileColor : Color.green;
        }

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<EnemyProjectile>();
        }

        Vector3 direction = (targetPosition - spawnPosition).normalized;
        projectile.Initialize(
            direction,
            enemyData != null ? enemyData.ProjectileSpeed : 10f,
            enemyData != null ? enemyData.ProjectileLifetime : 4f,
            runtimeStats.GetValue(StatType.ContactDamage),
            enemyData != null ? enemyData.AttackStatuses : null);
    }

    private float ResolveIncomingDamage(float rawDamage)
    {
        float armor = runtimeStats != null ? runtimeStats.GetValue(StatType.Armor) : CombatMath.BaseEnemyArmor;
        return Mathf.Max(0f, rawDamage * CombatMath.ResolveArmorDamageMultiplier(armor));
    }
}

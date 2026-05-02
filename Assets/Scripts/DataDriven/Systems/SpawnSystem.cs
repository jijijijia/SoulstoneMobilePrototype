using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterSystem player;
    [SerializeField] private DDRunManager runManager;
    [SerializeField] private SpawnTimelineData spawnTimeline;
    [SerializeField] private Transform groundTransform;
    [SerializeField] private EnemyData[] enemyPool;
    [SerializeField] private GameObject experienceGemPrefab;

    [Header("Fallback Timeline")]
    [SerializeField] private float fallbackSpawnRate = 0.5f;
    [SerializeField] private int fallbackPackSize = 1;
    [SerializeField] private int fallbackMaxAlive = 20;
    [SerializeField] private float fallbackSpawnRadius = 18f;

    [Header("Ring Spawn")]
    [SerializeField] private float mapPadding = 1.5f;
    [SerializeField] private float minimumSpawnSeparation = 2.5f;
    [SerializeField] private float activeEnemySpawnSeparation = 2f;
    [SerializeField, Min(8)] private int ringSampleCount = 24;
    [SerializeField, Range(0f, 1f)] private float ringRadiusJitter = 0.08f;

    [Header("Fallback Bounds")]
    [SerializeField] private Vector2 mapMin = new(-20f, -20f);
    [SerializeField] private Vector2 mapMax = new(20f, 20f);

    private float timer;
    private float elapsedTime;
    private int aliveEnemies;
    private int killCount;

    private readonly List<EnemyData> eligibleEnemiesBuffer = new();
    private readonly List<Vector3> reservedSpawnPositionsBuffer = new();
    private readonly EnemySpawnSelector enemySpawnSelector = new();
    private readonly HashSet<int> triggeredBurstEvents = new();

    private DataDrivenPlayerController playerController;
    private Bounds groundBounds;
    private bool hasGroundBounds;
    private bool hasMapBoundsOverride;
    private Vector2 mapBoundsOverrideMin;
    private Vector2 mapBoundsOverrideMax;
    private float mapEnemyHealthMultiplier = 1f;
    private float mapEnemyDamageMultiplier = 1f;
    private float mapSpawnRateMultiplier = 1f;
    private int mapPackSizeBonus;
    private int mapMaxAliveBonus;

    public event Action<int> KillCountChanged;
    public event Action<float> ElapsedTimeChanged;

    public int KillCount => killCount;
    public float ElapsedTime => elapsedTime;
    public float NormalizedTime => GetNormalizedTime();

    public void ApplyMapData(MapData mapData)
    {
        if (mapData == null)
        {
            return;
        }

        if (mapData.SpawnTimeline != null)
        {
            spawnTimeline = mapData.SpawnTimeline;
        }

        if (mapData.EnemyPool != null && mapData.EnemyPool.Length > 0)
        {
            enemyPool = mapData.EnemyPool;
        }

        mapEnemyHealthMultiplier = mapData.EnemyHealthMultiplier;
        mapEnemyDamageMultiplier = mapData.EnemyDamageMultiplier;
        mapSpawnRateMultiplier = mapData.SpawnRateMultiplier;
        mapPackSizeBonus = mapData.PackSizeBonus;
        mapMaxAliveBonus = mapData.MaxAliveBonus;
        hasMapBoundsOverride = mapData.OverrideMapBounds;
        mapBoundsOverrideMin = mapData.MapMin;
        mapBoundsOverrideMax = mapData.MapMax;

        CacheGroundBounds();
    }

    private void Awake()
    {
        CacheGroundBounds();
        CacheRunManager();
        CachePlayerController();
    }

    private void OnEnable()
    {
        timer = 0f;
        elapsedTime = 0f;
        aliveEnemies = 0;
        killCount = 0;
        triggeredBurstEvents.Clear();
        reservedSpawnPositionsBuffer.Clear();

        CacheGroundBounds();
        CacheRunManager();
        CachePlayerController();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        CacheRunManager();

        if (runManager != null && !runManager.IsGameplayRunning)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        timer += Time.deltaTime;
        ElapsedTimeChanged?.Invoke(elapsedTime);

        float normalizedTime = GetNormalizedTime();

        ProcessBurstEvents(normalizedTime);

        int maxAlive = GetCurrentMaxAlive(normalizedTime);
        float spawnRate = GetCurrentSpawnRate(normalizedTime);

        if (spawnRate <= 0f || aliveEnemies >= maxAlive)
        {
            return;
        }

        float spawnDelay = 1f / spawnRate;

        while (timer >= spawnDelay && aliveEnemies < maxAlive)
        {
            timer -= spawnDelay;
            SpawnPulse(normalizedTime, maxAlive);
        }
    }

    public void NotifyEnemyKilled(EnemyAgent enemy)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        killCount++;
        KillCountChanged?.Invoke(killCount);
    }

    public void SpawnSpecificEnemy(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        int maxAlive = GetCurrentMaxAlive(GetNormalizedTime());

        if (aliveEnemies >= maxAlive)
        {
            return;
        }

        reservedSpawnPositionsBuffer.Clear();
        Vector3 spawnPosition = FindSpawnPositionOnRing(enemyData, reservedSpawnPositionsBuffer, true);
        reservedSpawnPositionsBuffer.Add(spawnPosition);

        SpawnEnemy(
            enemyData,
            GetCurrentHealthMultiplier(GetNormalizedTime()),
            GetCurrentDamageMultiplier(GetNormalizedTime()),
            spawnPosition);
    }

    public Vector3 ClampToMap(Vector3 position)
    {
        Vector2 min = GetMapMin();
        Vector2 max = GetMapMax();
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.z = Mathf.Clamp(position.z, min.y, max.y);
        return position;
    }

    public void SpawnExperienceGem(Vector3 position, int experienceValue)
    {
        GameObject gemPrefab = experienceGemPrefab != null
            ? experienceGemPrefab
            : DefaultRuntimePrefabFactory.GetExperienceGemPrefab();

        GameObject gemObject = PoolManager.Spawn(gemPrefab, position + Vector3.up * 0.5f, Quaternion.identity);

        if (gemObject == null)
        {
            return;
        }

        ExperienceGem gem = gemObject.GetComponent<ExperienceGem>();

        if (gem == null)
        {
            gem = gemObject.AddComponent<ExperienceGem>();
        }

        float experienceMultiplier = runManager != null ? runManager.CurrentExperienceMultiplier : 1f;
        gem.SetExperienceValue(Mathf.Max(1, Mathf.RoundToInt(experienceValue * experienceMultiplier)));
        gem.SetCollector(player);
    }

    private void SpawnPulse(float normalizedTime, int maxAlive)
    {
        enemySpawnSelector.FillEligibleEnemies(
            eligibleEnemiesBuffer,
            enemyPool,
            spawnTimeline,
            normalizedTime,
            killCount);

        if (eligibleEnemiesBuffer.Count == 0)
        {
            return;
        }

        int spawnCount = Mathf.Min(GetCurrentPackSize(normalizedTime), Mathf.Max(0, maxAlive - aliveEnemies));
        float healthMultiplier = GetCurrentHealthMultiplier(normalizedTime);
        float damageMultiplier = GetCurrentDamageMultiplier(normalizedTime);

        reservedSpawnPositionsBuffer.Clear();

        for (int i = 0; i < spawnCount && aliveEnemies < maxAlive; i++)
        {
            EnemyData selected = enemySpawnSelector.PickEnemyByWeight(eligibleEnemiesBuffer, spawnTimeline, normalizedTime);

            if (selected == null)
            {
                break;
            }

            Vector3 spawnPosition = FindSpawnPositionOnRing(selected, reservedSpawnPositionsBuffer, false);
            reservedSpawnPositionsBuffer.Add(spawnPosition);

            SpawnEnemy(selected, healthMultiplier, damageMultiplier, spawnPosition);
        }
    }

    private void ProcessBurstEvents(float normalizedTime)
    {
        if (spawnTimeline == null || spawnTimeline.BurstEvents == null || spawnTimeline.BurstEvents.Length == 0)
        {
            return;
        }

        for (int i = 0; i < spawnTimeline.BurstEvents.Length; i++)
        {
            if (triggeredBurstEvents.Contains(i))
            {
                continue;
            }

            SpawnBurstEvent burstEvent = spawnTimeline.BurstEvents[i];

            if (burstEvent == null || burstEvent.enemy == null || normalizedTime < burstEvent.t)
            {
                continue;
            }

            triggeredBurstEvents.Add(i);

            int maxAlive = GetCurrentMaxAlive(normalizedTime);
            int count = Mathf.Max(1, burstEvent.count);
            reservedSpawnPositionsBuffer.Clear();

            for (int spawnIndex = 0; spawnIndex < count && aliveEnemies < maxAlive; spawnIndex++)
            {
                Vector3 spawnPosition = FindSpawnPositionOnRing(burstEvent.enemy, reservedSpawnPositionsBuffer, true);
                reservedSpawnPositionsBuffer.Add(spawnPosition);

                SpawnEnemy(
                    burstEvent.enemy,
                    Mathf.Max(0.1f, burstEvent.healthMultiplier),
                    Mathf.Max(0.1f, burstEvent.damageMultiplier),
                    spawnPosition);
            }
        }
    }

    private void SpawnEnemy(EnemyData enemyData, float healthMultiplier, float damageMultiplier, Vector3 spawnPosition)
    {
        if (enemyData == null || enemyData.Prefab == null)
        {
            return;
        }

        float normalizedTime = GetNormalizedTime();
        float resolvedHealthMultiplier = CombatMath.ResolveEnemyHealthMultiplier(healthMultiplier, normalizedTime, enemyData.Category);
        float resolvedDamageMultiplier = CombatMath.ResolveEnemyDamageMultiplier(damageMultiplier, normalizedTime, enemyData.Category);
        GameObject enemyObject = PoolManager.Spawn(enemyData.Prefab, spawnPosition, Quaternion.identity);

        if (enemyObject == null)
        {
            return;
        }

        EnemyAgent agent = enemyObject.GetComponent<EnemyAgent>();

        if (agent == null)
        {
            agent = enemyObject.AddComponent<EnemyAgent>();
        }

        agent.Initialize(enemyData, player, this, resolvedHealthMultiplier, resolvedDamageMultiplier);
        aliveEnemies++;
    }

    private Vector3 FindSpawnPositionOnRing(EnemyData enemyData, List<Vector3> reservedPositions, bool safePadding)
    {
        float radius = spawnTimeline != null ? spawnTimeline.SpawnDistance : fallbackSpawnRadius;
        float directionBias = spawnTimeline != null ? spawnTimeline.PlayerDirectionBias : 0f;
        Vector3 moveDirection = GetPlayerMoveDirection();
        float baseAngle = GetBaseAngle(moveDirection, directionBias);
        int samples = Mathf.Max(8, ringSampleCount);
        float padding = safePadding
            ? Mathf.Max(mapPadding, enemyData != null ? enemyData.VisualScaleMultiplier * 1.6f : mapPadding)
            : mapPadding;

        int startIndex = UnityEngine.Random.Range(0, samples);

        for (int i = 0; i < samples; i++)
        {
            int sampleIndex = (startIndex + i) % samples;
            float angle = baseAngle + (sampleIndex / (float)samples) * Mathf.PI * 2f;
            float jitterMultiplier = 1f + UnityEngine.Random.Range(-ringRadiusJitter, ringRadiusJitter);
            Vector3 direction = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 candidate = player.transform.position + direction * radius * jitterMultiplier;
            candidate.y = 0.5f;

            if (IsValidSpawnPoint(candidate, reservedPositions, padding))
            {
                return candidate;
            }
        }

        return GetFallbackSpawnPosition(reservedPositions, padding);
    }

    private bool IsValidSpawnPoint(Vector3 candidate, List<Vector3> reservedPositions, float padding)
    {
        if (!IsInsideMap(candidate, padding))
        {
            return false;
        }

        if (!HasEnoughSeparation(candidate, reservedPositions))
        {
            return false;
        }

        return true;
    }

    private Vector3 GetFallbackSpawnPosition(List<Vector3> reservedPositions, float padding)
    {
        Vector2 min = GetMapMin();
        Vector2 max = GetMapMax();

        for (int i = 0; i < 48; i++)
        {
            Vector3 candidate = new(
                UnityEngine.Random.Range(min.x + padding, max.x - padding),
                0.5f,
                UnityEngine.Random.Range(min.y + padding, max.y - padding));

            if (HasEnoughSeparation(candidate, reservedPositions))
            {
                return candidate;
            }
        }

        return new Vector3(
            Mathf.Clamp(player.transform.position.x, min.x + padding, max.x - padding),
            0.5f,
            Mathf.Clamp(player.transform.position.z, min.y + padding, max.y - padding));
    }

    private float GetBaseAngle(Vector3 moveDirection, float directionBias)
    {
        if (moveDirection.sqrMagnitude <= 0.001f || directionBias <= 0f)
        {
            return UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        }

        Vector3 biasedDirection = Vector3.Slerp(Vector3.forward, moveDirection.normalized, directionBias).normalized;
        return Mathf.Atan2(biasedDirection.z, biasedDirection.x);
    }

    private bool HasEnoughSeparation(Vector3 candidate, List<Vector3> reservedPositions)
    {
        if (reservedPositions != null && reservedPositions.Count > 0 && minimumSpawnSeparation > 0.01f)
        {
            float minimumDistanceSqr = minimumSpawnSeparation * minimumSpawnSeparation;

            for (int i = 0; i < reservedPositions.Count; i++)
            {
                Vector3 delta = reservedPositions[i] - candidate;
                delta.y = 0f;

                if (delta.sqrMagnitude < minimumDistanceSqr)
                {
                    return false;
                }
            }
        }

        if (activeEnemySpawnSeparation > 0.01f && EnemyRegistry.HasEnemyWithinDistance(candidate, activeEnemySpawnSeparation))
        {
            return false;
        }

        return true;
    }

    private bool IsInsideMap(Vector3 position, float padding)
    {
        Vector2 min = GetMapMin();
        Vector2 max = GetMapMax();

        return position.x >= min.x + padding &&
               position.x <= max.x - padding &&
               position.z >= min.y + padding &&
               position.z <= max.y - padding;
    }

    private void CacheGroundBounds()
    {
        if (groundTransform == null)
        {
            GameObject groundObject = GameObject.Find("Ground");
            groundTransform = groundObject != null ? groundObject.transform : null;
        }

        if (groundTransform == null)
        {
            hasGroundBounds = false;
            return;
        }

        if (groundTransform.TryGetComponent(out Collider groundCollider))
        {
            groundBounds = groundCollider.bounds;
            hasGroundBounds = true;
            return;
        }

        if (groundTransform.TryGetComponent(out Renderer groundRenderer))
        {
            groundBounds = groundRenderer.bounds;
            hasGroundBounds = true;
            return;
        }

        hasGroundBounds = false;
    }

    private void CachePlayerController()
    {
        if (player != null)
        {
            playerController = player.GetComponent<DataDrivenPlayerController>();
        }
    }

    private void CacheRunManager()
    {
        if (runManager == null)
        {
            runManager = FindFirstObjectByType<DDRunManager>();
        }
    }

    private float GetNormalizedTime()
    {
        return spawnTimeline != null ? spawnTimeline.GetNormalizedTime(elapsedTime) : 0f;
    }

    private float GetCurrentSpawnRate(float normalizedTime)
    {
        float baseValue = spawnTimeline != null
            ? spawnTimeline.GetSpawnRate(normalizedTime, fallbackSpawnRate)
            : fallbackSpawnRate;

        return baseValue * mapSpawnRateMultiplier * (runManager != null ? runManager.CurrentSpawnRateMultiplier : 1f);
    }

    private int GetCurrentPackSize(float normalizedTime)
    {
        int baseValue = spawnTimeline != null
            ? spawnTimeline.GetPackSize(normalizedTime, fallbackPackSize)
            : fallbackPackSize;

        return Mathf.Max(1, baseValue + mapPackSizeBonus + (runManager != null ? runManager.CurrentPackSizeBonus : 0));
    }

    private int GetCurrentMaxAlive(float normalizedTime)
    {
        int baseValue = spawnTimeline != null
            ? spawnTimeline.GetMaxAlive(normalizedTime, fallbackMaxAlive)
            : fallbackMaxAlive;

        return Mathf.Max(1, baseValue + mapMaxAliveBonus + (runManager != null ? runManager.CurrentMaxAliveBonus : 0));
    }

    private float GetCurrentHealthMultiplier(float normalizedTime)
    {
        float baseValue = spawnTimeline != null
            ? spawnTimeline.GetHealthMultiplier(normalizedTime, 1f)
            : 1f;

        return baseValue * mapEnemyHealthMultiplier * (runManager != null ? runManager.CurrentEnemyHealthMultiplier : 1f);
    }

    private float GetCurrentDamageMultiplier(float normalizedTime)
    {
        float baseValue = spawnTimeline != null
            ? spawnTimeline.GetDamageMultiplier(normalizedTime, 1f)
            : 1f;

        return baseValue * mapEnemyDamageMultiplier * (runManager != null ? runManager.CurrentEnemyDamageMultiplier : 1f);
    }

    private Vector3 GetPlayerMoveDirection()
    {
        CachePlayerController();

        if (playerController == null)
        {
            return Vector3.zero;
        }

        Vector3 moveDirection = playerController.LastMoveDirection;
        moveDirection.y = 0f;
        return moveDirection.sqrMagnitude > 0.001f ? moveDirection.normalized : Vector3.zero;
    }

    private Vector2 GetMapMin()
    {
        if (hasMapBoundsOverride)
        {
            return mapBoundsOverrideMin;
        }

        return hasGroundBounds
            ? new Vector2(groundBounds.min.x, groundBounds.min.z)
            : mapMin;
    }

    private Vector2 GetMapMax()
    {
        if (hasMapBoundsOverride)
        {
            return mapBoundsOverrideMax;
        }

        return hasGroundBounds
            ? new Vector2(groundBounds.max.x, groundBounds.max.z)
            : mapMax;
    }
}

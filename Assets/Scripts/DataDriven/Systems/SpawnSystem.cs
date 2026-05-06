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
    private int eliteKillCount;
    private int miniBossKillCount;
    private int bossKillCount;

    private readonly List<EnemyData> eligibleEnemiesBuffer = new();
    private readonly List<Vector3> reservedSpawnPositionsBuffer = new();
    private readonly EnemySpawnSelector enemySpawnSelector = new();
    private readonly MapBoundsResolver mapBoundsResolver = new();
    private readonly RingSpawnPositionProvider spawnPositionProvider = new();
    private readonly HashSet<int> triggeredBurstEvents = new();

    private DataDrivenPlayerController playerController;
    private bool attemptedRunManagerResolve;
    private float mapEnemyHealthMultiplier = 1f;
    private float mapEnemyDamageMultiplier = 1f;
    private float mapSpawnRateMultiplier = 1f;
    private int mapPackSizeBonus;
    private int mapMaxAliveBonus;

    public event Action<int> KillCountChanged;
    public event Action<float> ElapsedTimeChanged;
    public event Action<EnemyAgent> BossEnemySpawned;
    public event Action<EnemyAgent> BossEnemyKilled;

    private int activeBossCount;
    private bool hadBossesSpawned;

    public int KillCount => killCount;
    public int EliteKillCount => eliteKillCount;
    public int MiniBossKillCount => miniBossKillCount;
    public int BossKillCount => bossKillCount;
    public float ElapsedTime => elapsedTime;
    public float NormalizedTime => GetNormalizedTime();
    public int ActiveBossCount => activeBossCount;
    public bool HadBossesSpawned => hadBossesSpawned;

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
        mapBoundsResolver.ApplyMapData(mapData);

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
        eliteKillCount = 0;
        miniBossKillCount = 0;
        bossKillCount = 0;
        activeBossCount = 0;
        hadBossesSpawned = false;
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

        if (enemy != null && enemy.Data != null)
        {
            switch (enemy.Data.Category)
            {
                case EnemyCategory.Elite:
                    eliteKillCount++;
                    break;
                case EnemyCategory.MiniBoss:
                    miniBossKillCount++;
                    break;
                case EnemyCategory.Boss:
                    bossKillCount++;
                    break;
            }
        }

        if (enemy != null && enemy.Data != null && IsBossCategory(enemy.Data.Category))
        {
            activeBossCount = Mathf.Max(0, activeBossCount - 1);
            BossEnemyKilled?.Invoke(enemy);
        }
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

    public void SpawnSpecificEnemy(EnemyData enemyData, Vector3 spawnCenter, float spawnRadius)
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

        Vector3 spawnPosition = FindSpawnPositionNear(spawnCenter, Mathf.Max(0.5f, spawnRadius));

        SpawnEnemy(
            enemyData,
            GetCurrentHealthMultiplier(GetNormalizedTime()),
            GetCurrentDamageMultiplier(GetNormalizedTime()),
            spawnPosition);
    }

    public Vector3 ClampToMap(Vector3 position)
    {
        return mapBoundsResolver.Clamp(position);
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
            PoolManager.MarkPoolableCacheDirty(gemObject);
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
            PoolManager.MarkPoolableCacheDirty(enemyObject);
        }

        agent.Initialize(enemyData, player, this, resolvedHealthMultiplier, resolvedDamageMultiplier);
        aliveEnemies++;

        if (IsBossCategory(enemyData.Category))
        {
            activeBossCount++;
            hadBossesSpawned = true;
            BossEnemySpawned?.Invoke(agent);
        }
    }

    private Vector3 FindSpawnPositionOnRing(EnemyData enemyData, List<Vector3> reservedPositions, bool safePadding)
    {
        return spawnPositionProvider.FindSpawnPosition(
            player.transform,
            enemyData,
            spawnTimeline,
            fallbackSpawnRadius,
            GetPlayerMoveDirection(),
            reservedPositions,
            safePadding,
            mapBoundsResolver,
            mapPadding,
            minimumSpawnSeparation,
            activeEnemySpawnSeparation,
            ringSampleCount,
            ringRadiusJitter);
    }

    private Vector3 FindSpawnPositionNear(Vector3 center, float radius)
    {
        const int sampleCount = 12;

        Vector3 bestPosition = mapBoundsResolver.Clamp(center);
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < sampleCount; i++)
        {
            float angle = (360f / sampleCount) * i + UnityEngine.Random.Range(-12f, 12f);
            float distance = UnityEngine.Random.Range(radius * 0.35f, radius);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad)) * distance;
            Vector3 candidate = mapBoundsResolver.Clamp(center + offset);
            float score = ScoreLocalSpawnPosition(candidate);

            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = candidate;
            }
        }

        return bestPosition;
    }

    private float ScoreLocalSpawnPosition(Vector3 candidate)
    {
        return EnemyRegistry.HasEnemyWithinDistance(candidate, activeEnemySpawnSeparation) ? -100f : 0f;
    }

    private void CacheGroundBounds()
    {
        mapBoundsResolver.ConfigureFallback(mapMin, mapMax);

        if (groundTransform == null)
        {
            GameObject groundObject = GameObject.Find("Ground");
            groundTransform = groundObject != null ? groundObject.transform : null;
        }

        mapBoundsResolver.CacheGroundBounds(groundTransform);
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
        if (runManager != null || attemptedRunManagerResolve)
        {
            return;
        }

        attemptedRunManagerResolve = true;
        runManager = FindFirstObjectByType<DDRunManager>();
    }

    private void OnValidate()
    {
        if (runManager != null)
        {
            attemptedRunManagerResolve = true;
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

    private static bool IsBossCategory(EnemyCategory category)
    {
        return category == EnemyCategory.Boss || category == EnemyCategory.MiniBoss;
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

}

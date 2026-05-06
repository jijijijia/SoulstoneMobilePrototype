using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Map", fileName = "MapData")]
public class MapData : ScriptableObject
{
    [SerializeField] private string mapId;
    [SerializeField] private string displayName;
    [SerializeField] private string difficulty;
    [SerializeField] private string biome;
    [TextArea(2, 5)]
    [SerializeField] private string enemyDescription;
    [TextArea(2, 5)]
    [SerializeField] private string rewardsDescription;
    [SerializeField] private Sprite previewImage;
    [SerializeField] private int recommendedLevel = 1;
    [SerializeField] private float experienceMultiplier = 1f;
    [SerializeField] private EnemyData[] bossList;
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private SpawnTimelineData spawnTimeline;
    [SerializeField] private EnemyData[] enemyPool;
    [SerializeField] private float enemyHealthMultiplier = 1f;
    [SerializeField] private float enemyDamageMultiplier = 1f;
    [SerializeField] private float spawnRateMultiplier = 1f;
    [SerializeField] private int packSizeBonus;
    [SerializeField] private int maxAliveBonus;
    [Header("Rewards")]
    [SerializeField] private float rewardMultiplier = 1f;
    [Header("Player Spawn")]
    [SerializeField] private Vector3 playerSpawnPosition = new(0f, 1.08f, 0f);
    [SerializeField] private float playerSpawnClearRadius = 1.25f;
    [SerializeField] private bool overrideMapBounds;
    [SerializeField] private Vector2 mapMin = new(-20f, -20f);
    [SerializeField] private Vector2 mapMax = new(20f, 20f);
    [Header("Unlock")]
    [SerializeField] private string unlockId;
    [SerializeField] private bool unlockedByDefault;
    [SerializeField] private int unlockCost;
    [SerializeField] private CurrencyType unlockCurrency = CurrencyType.SoulShards;
    [TextArea(2, 4)]
    [SerializeField] private string unlockRequirementText;
    [SerializeField] private string requiredAchievementId;
    [SerializeField] private int requiredPlayerLevel = 1;

    public string MapId => mapId;
    public string DisplayName => displayName;
    public string Difficulty => difficulty;
    public string Biome => biome;
    public string EnemyDescription => enemyDescription;
    public string RewardsDescription => rewardsDescription;
    public Sprite PreviewImage => previewImage;
    public int RecommendedLevel => Mathf.Max(1, recommendedLevel);
    public float ExperienceMultiplier => Mathf.Max(0.1f, experienceMultiplier);
    public EnemyData[] BossList => bossList;
    public GameObject MapPrefab => mapPrefab;
    public SpawnTimelineData SpawnTimeline => spawnTimeline;
    public EnemyData[] EnemyPool => enemyPool;
    public float EnemyHealthMultiplier => Mathf.Max(0.1f, enemyHealthMultiplier);
    public float EnemyDamageMultiplier => Mathf.Max(0.1f, enemyDamageMultiplier);
    public float SpawnRateMultiplier => Mathf.Max(0.1f, spawnRateMultiplier);
    public int PackSizeBonus => packSizeBonus;
    public int MaxAliveBonus => maxAliveBonus;
    public float RewardMultiplier => Mathf.Max(0.1f, rewardMultiplier);
    public Vector3 PlayerSpawnPosition => playerSpawnPosition;
    public float PlayerSpawnClearRadius => Mathf.Max(0.25f, playerSpawnClearRadius);
    public bool OverrideMapBounds => overrideMapBounds;
    public Vector2 MapMin => mapMin;
    public Vector2 MapMax => mapMax;
    public string UnlockId => string.IsNullOrWhiteSpace(unlockId) ? mapId : unlockId;
    public bool UnlockedByDefault => unlockedByDefault;
    public int UnlockCost => unlockedByDefault ? 0 : Mathf.Max(120, unlockCost);
    public CurrencyType UnlockCurrency => unlockCurrency;
    public string UnlockRequirementText => string.IsNullOrWhiteSpace(unlockRequirementText)
        ? $"Разблокировать за {UnlockCost} {UnlockCurrency}"
        : unlockRequirementText;
    public string RequiredAchievementId => requiredAchievementId;
    public int RequiredPlayerLevel => Mathf.Max(1, requiredPlayerLevel);

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            Debug.LogWarning($"{name} has an empty map id.", this);
        }

        if (mapPrefab == null)
        {
            Debug.LogWarning($"{name} has no map prefab. Maps are prefab-owned; assign a prefab unless this is an intentional prototype scene test.", this);
        }

        if (overrideMapBounds && (mapMin.x >= mapMax.x || mapMin.y >= mapMax.y))
        {
            Debug.LogWarning($"{name} has invalid map bounds. MapMin must be lower than MapMax.", this);
        }
    }
}

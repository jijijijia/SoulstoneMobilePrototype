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
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private SpawnTimelineData spawnTimeline;
    [SerializeField] private EnemyData[] enemyPool;
    [SerializeField] private float enemyHealthMultiplier = 1f;
    [SerializeField] private float enemyDamageMultiplier = 1f;
    [SerializeField] private float spawnRateMultiplier = 1f;
    [SerializeField] private int packSizeBonus;
    [SerializeField] private int maxAliveBonus;
    [SerializeField] private bool overrideMapBounds;
    [SerializeField] private Vector2 mapMin = new(-20f, -20f);
    [SerializeField] private Vector2 mapMax = new(20f, 20f);

    public string MapId => mapId;
    public string DisplayName => displayName;
    public string Difficulty => difficulty;
    public string Biome => biome;
    public string EnemyDescription => enemyDescription;
    public string RewardsDescription => rewardsDescription;
    public GameObject MapPrefab => mapPrefab;
    public SpawnTimelineData SpawnTimeline => spawnTimeline;
    public EnemyData[] EnemyPool => enemyPool;
    public float EnemyHealthMultiplier => Mathf.Max(0.1f, enemyHealthMultiplier);
    public float EnemyDamageMultiplier => Mathf.Max(0.1f, enemyDamageMultiplier);
    public float SpawnRateMultiplier => Mathf.Max(0.1f, spawnRateMultiplier);
    public int PackSizeBonus => packSizeBonus;
    public int MaxAliveBonus => maxAliveBonus;
    public bool OverrideMapBounds => overrideMapBounds;
    public Vector2 MapMin => mapMin;
    public Vector2 MapMax => mapMax;
}

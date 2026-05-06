using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Enemy", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private string enemyId;
    [SerializeField] private string displayName;
    [SerializeField] private EnemyCategory category;
    [SerializeField] private GameObject prefab;
    [SerializeField] private SkillData attackSkill;
    [SerializeField] private EnemyAbilityConfig abilityConfig;
    [SerializeField] private EnemyAttackType attackType = EnemyAttackType.MeleeContact;
    [SerializeField] private StatValue[] baseStats;
    [SerializeField] private int spawnCost = 1;
    [SerializeField] private float spawnWeight = 1f;
    [SerializeField] private int unlockPlayerLevel = 1;
    [SerializeField] private int killRequirement;
    [SerializeField] private int experienceReward = 1;
    [SerializeField] private float visualScaleMultiplier = 1f;
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField] private float preferredDistance = 1.4f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private float projectileScale = 0.4f;
    [SerializeField] private Color projectileColor = Color.green;
    [SerializeField] private StatusEffectData[] attackStatuses;

    public string EnemyId => enemyId;
    public string DisplayName => displayName;
    public EnemyCategory Category => category;
    public GameObject Prefab => prefab;
    public SkillData AttackSkill => attackSkill;
    public EnemyAbilityConfig AbilityConfig => abilityConfig;
    public EnemyAttackType AttackType => abilityConfig != null ? abilityConfig.MovementAttackType : attackType;
    public EnemyAbilityDeliveryType AbilityDeliveryType => abilityConfig != null
        ? abilityConfig.DeliveryType
        : (attackType == EnemyAttackType.RangedProjectile ? EnemyAbilityDeliveryType.Projectile : EnemyAbilityDeliveryType.MeleeContact);
    public StatValue[] BaseStats => baseStats;
    public int SpawnCost => Mathf.Max(1, spawnCost);
    public float SpawnWeight => Mathf.Max(0.1f, spawnWeight);
    public int UnlockPlayerLevel => Mathf.Max(1, unlockPlayerLevel);
    public int KillRequirement => Mathf.Max(0, killRequirement);
    public int ExperienceReward => Mathf.Max(1, experienceReward);
    public float VisualScaleMultiplier => Mathf.Max(0.1f, visualScaleMultiplier);
    public Color TintColor => tintColor;
    public float PreferredDistance => abilityConfig != null ? abilityConfig.PreferredDistance : Mathf.Max(0.5f, preferredDistance);
    public float AttackInterval => AbilityCooldown;
    public float AbilityCooldown => abilityConfig != null ? abilityConfig.Cooldown : Mathf.Max(0.1f, attackInterval);
    public float AbilityDamageMultiplier => abilityConfig != null ? abilityConfig.DamageMultiplier : 1f;
    public float AbilityRangeTolerance => abilityConfig != null ? abilityConfig.RangeTolerance : 0.35f;
    public float ProjectileSpeed => abilityConfig != null ? abilityConfig.ProjectileSpeed : Mathf.Max(0.1f, projectileSpeed);
    public float ProjectileLifetime => abilityConfig != null ? abilityConfig.ProjectileLifetime : Mathf.Max(0.1f, projectileLifetime);
    public float ProjectileScale => abilityConfig != null ? abilityConfig.ProjectileScale : Mathf.Max(0.1f, projectileScale);
    public Color ProjectileColor => abilityConfig != null ? abilityConfig.ProjectileColor : projectileColor;
    public GameObject ProjectilePrefab => abilityConfig != null ? abilityConfig.ProjectilePrefab : null;
    public float AbilityAreaRadius => abilityConfig != null ? abilityConfig.AreaRadius : Mathf.Max(0.1f, preferredDistance);
    public GameObject AbilityAreaVisualPrefab => abilityConfig != null ? abilityConfig.AreaVisualPrefab : null;
    public float AbilityAreaVisualLifetime => abilityConfig != null ? abilityConfig.AreaVisualLifetime : 0.25f;
    public StatusEffectData[] AttackStatuses => AbilityStatuses;
    public StatusEffectData[] AbilityStatuses => abilityConfig != null ? abilityConfig.Statuses : attackStatuses;
}

using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Enemy Ability", fileName = "EnemyAbility")]
public class EnemyAbilityConfig : ScriptableObject
{
    [SerializeField] private string abilityId;
    [SerializeField] private string displayName;
    [SerializeField] private EnemyAbilityDeliveryType deliveryType = EnemyAbilityDeliveryType.MeleeContact;
    [Tooltip("Optional bridge to the shared player attack module vocabulary. Enemy execution uses the fields below.")]
    [SerializeField] private AttackDefinition attackDefinition;
    [SerializeField] private float preferredDistance = 1.4f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private float rangeTolerance = 0.35f;
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 4f;
    [SerializeField] private float projectileScale = 0.4f;
    [SerializeField] private Color projectileColor = Color.green;
    [Header("Area")]
    [SerializeField] private float areaRadius = 2.4f;
    [SerializeField] private GameObject areaVisualPrefab;
    [SerializeField] private float areaVisualLifetime = 0.25f;
    [Header("Status")]
    [SerializeField] private StatusEffectData[] statuses;
    [Header("Dash / Telegraph")]
    [SerializeField] private float telegraphDuration = 0.5f;
    [Header("Delayed Area")]
    [SerializeField] private float delayDuration = 1.2f;
    [Header("Lasting Area")]
    [SerializeField] private float poolDuration = 4f;
    [SerializeField] private float poolTickInterval = 0.5f;
    [Header("Summon")]
    [SerializeField] private int summonCount = 2;
    [SerializeField] private EnemyData summonEnemyData;
    [Header("Ally Buff")]
    [SerializeField] private float buffRadius = 5f;
    [SerializeField] private StatType buffStatType = StatType.ContactDamage;
    [SerializeField] private float buffMultiplier = 0.25f;
    [SerializeField] private float buffDuration = 8f;

    public string AbilityId => abilityId;
    public string DisplayName => displayName;
    public EnemyAbilityDeliveryType DeliveryType => deliveryType;
    public AttackDefinition AttackDefinition => attackDefinition;
    public float PreferredDistance => Mathf.Max(0.5f, preferredDistance);
    public float Cooldown => Mathf.Max(0.1f, cooldown);
    public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);
    public float RangeTolerance => Mathf.Max(0f, rangeTolerance);
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => Mathf.Max(0.1f, projectileSpeed);
    public float ProjectileLifetime => Mathf.Max(0.1f, projectileLifetime);
    public float ProjectileScale => Mathf.Max(0.1f, projectileScale);
    public Color ProjectileColor => projectileColor;
    public float AreaRadius => Mathf.Max(0.1f, areaRadius);
    public GameObject AreaVisualPrefab => areaVisualPrefab;
    public float AreaVisualLifetime => Mathf.Max(0.01f, areaVisualLifetime);
    public StatusEffectData[] Statuses => statuses;
    public float TelegraphDuration => Mathf.Max(0.1f, telegraphDuration);
    public float DelayDuration => Mathf.Max(0.1f, delayDuration);
    public float PoolDuration => Mathf.Max(0.5f, poolDuration);
    public float PoolTickInterval => Mathf.Max(0.05f, poolTickInterval);
    public int SummonCount => Mathf.Max(1, summonCount);
    public EnemyData SummonEnemyData => summonEnemyData;
    public float BuffRadius => Mathf.Max(0.5f, buffRadius);
    public StatType BuffStatType => buffStatType;
    public float BuffMultiplier => buffMultiplier;
    public float BuffDuration => Mathf.Max(0.5f, buffDuration);

    public EnemyAttackType MovementAttackType => deliveryType switch
    {
        EnemyAbilityDeliveryType.Projectile => EnemyAttackType.RangedProjectile,
        EnemyAbilityDeliveryType.AreaPulse => EnemyAttackType.RangedProjectile,
        EnemyAbilityDeliveryType.SlowProjectile => EnemyAttackType.RangedProjectile,
        EnemyAbilityDeliveryType.LastingPoisonPool => EnemyAttackType.RangedProjectile,
        EnemyAbilityDeliveryType.DelayedAreaMarker => EnemyAttackType.RangedProjectile,
        _ => EnemyAttackType.MeleeContact
    };
}

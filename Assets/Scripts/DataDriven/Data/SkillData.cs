using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill", fileName = "SkillData")]
public class SkillData : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private SkillHitType hitType;
    [SerializeField] private SkillElement element;
    [SerializeField] private SkillRuntimeDefinition runtimeDefinition;
    [SerializeField] private GameObject visualPrefab;
    [Header("Skill Combat Stats")]
    [SerializeField] private float baseDamage;
    [SerializeField] private float flatDamageBonus;
    [SerializeField] private float critChance = 0.05f;
    [SerializeField] private float critMultiplier = 2f;
    [SerializeField] private float areaBonus;
    [SerializeField] private float cooldown;
    [SerializeField] private float cooldownCoefficient = 1f;
    [SerializeField] private float cooldownDivider = 1f;
    [SerializeField] private float areaMultiplier = 1f;
    [SerializeField] private float repeatChance;
    [Header("Advanced")]
    [SerializeField] private bool globalSkill;
    [SerializeField] private int maxRank = 1;
    [SerializeField] private string[] tags;
    [SerializeField] private StatusEffectData[] appliedStatuses;
    [SerializeField] private ConfigurableSkillAttackData[] configurableAttacks;
    [SerializeField] private AttackDefinition[] attackDefinitions;
    [SerializeField] private SkillParameterData[] parameters;

    public string SkillId => skillId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public SkillHitType HitType => hitType;
    public SkillElement Element => element;
    public SkillRuntimeDefinition RuntimeDefinition => runtimeDefinition;
    public GameObject VisualPrefab => visualPrefab;
    public float BaseDamage => baseDamage;
    public float FlatDamageBonus => flatDamageBonus;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public float AreaBonus => areaBonus;
    public float Cooldown => cooldown;
    public float CooldownCoefficient => cooldownCoefficient;
    public float CooldownDivider => cooldownDivider;
    public float AreaMultiplier => areaMultiplier;
    public float RepeatChance => repeatChance;
    public bool GlobalSkill => globalSkill;
    public int MaxRank => maxRank;
    public string[] Tags => tags;
    public StatusEffectData[] AppliedStatuses => appliedStatuses;
    public ConfigurableSkillAttackData[] ConfigurableAttacks => configurableAttacks;
    public AttackDefinition[] AttackDefinitions => attackDefinitions;
    public SkillParameterData[] Parameters => parameters;

    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || tags == null)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (string.Equals(tags[i], tag, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public float GetParameter(string key, float fallback = 0f)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return fallback;
        }

        string requestedKey = key.Trim();

        foreach (SkillParameterData parameter in parameters)
        {
            if (string.Equals(parameter.key?.Trim(), requestedKey, System.StringComparison.OrdinalIgnoreCase))
            {
                return parameter.value;
            }
        }

        return fallback;
    }

    public float GetCombatStat(StatType statType)
    {
        return statType switch
        {
            StatType.Damage => flatDamageBonus,
            StatType.CritChance => critChance,
            StatType.CritMultiplier => critMultiplier,
            StatType.Area => areaBonus,
            StatType.Cooldown => cooldownCoefficient,
            StatType.SkillFrequency => cooldownDivider,
            StatType.AreaMultiplier => areaMultiplier,
            StatType.DoubleAttackChance => repeatChance,
            StatType.MulticastChance => repeatChance,
            _ => 0f
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        bool hasAttackDefinitions = attackDefinitions != null && attackDefinitions.Length > 0;

        if (runtimeDefinition == null && !hasAttackDefinitions)
        {
            Debug.LogWarning($"Skill '{name}' does not have Runtime Definition or Attack Definitions assigned.", this);
        }
    }
#endif
}

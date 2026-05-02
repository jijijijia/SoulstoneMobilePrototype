using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Upgrade", fileName = "UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [SerializeField] private bool useBuilderDefinition = true;
    [SerializeField] private string upgradeId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private UpgradeRarity rarity = UpgradeRarity.Common;
    [SerializeField] private UpgradeTargetType targetType = UpgradeTargetType.Character;
    [SerializeField] private UpgradeSkillTargetMode skillTargetMode = UpgradeSkillTargetMode.AllActiveSkills;
    [SerializeField] private StatType targetStat = StatType.AttackPower;
    [SerializeField] private float targetStatValue = 0.1f;
    [SerializeField] private UpgradeScope scope;
    [SerializeField] private bool repeatable = true;
    [SerializeField] private SkillData targetSkill;
    [SerializeField] private string[] targetTags;
    [SerializeField] private StatModifierData[] statModifiers;
    [SerializeField] private StatModifierData[] tagStatModifiers;

    public string UpgradeId => upgradeId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public bool UseBuilderDefinition => useBuilderDefinition;
    public UpgradeRarity Rarity => rarity;
    public UpgradeTargetType TargetType => targetType;
    public UpgradeSkillTargetMode SkillTargetMode => skillTargetMode;
    public StatType TargetStat => targetStat;
    public float TargetStatValue => targetStatValue;
    public UpgradeScope Scope => scope;
    public bool Repeatable => repeatable;
    public SkillData TargetSkill => targetSkill;
    public string[] TargetTags => targetTags;
    public StatModifierData[] StatModifiers => statModifiers;
    public StatModifierData[] TagStatModifiers => tagStatModifiers;

    public bool AffectsSpecificSkill => useBuilderDefinition
        ? targetType == UpgradeTargetType.Skill && skillTargetMode == UpgradeSkillTargetMode.SpecificSkill && targetSkill != null
        : scope == UpgradeScope.SpecificSkill && targetSkill != null;

    public bool AffectsTaggedSkills => !useBuilderDefinition && scope == UpgradeScope.TaggedSkills && targetTags != null && targetTags.Length > 0;

    public bool TargetsAllActiveSkills => useBuilderDefinition &&
                                          targetType == UpgradeTargetType.Skill &&
                                          skillTargetMode == UpgradeSkillTargetMode.AllActiveSkills;

    public StatModifierData[] GetCharacterModifiers()
    {
        if (!useBuilderDefinition)
        {
            return scope == UpgradeScope.GlobalStats || scope == UpgradeScope.CharacterOnly
                ? statModifiers
                : System.Array.Empty<StatModifierData>();
        }

        if (targetType != UpgradeTargetType.Character)
        {
            return System.Array.Empty<StatModifierData>();
        }

        return new[]
        {
            new StatModifierData
            {
                statType = targetStat,
                additive = targetStatValue,
                multiplier = 0f
            }
        };
    }

    public bool AffectsOnlySkillStats()
    {
        if (useBuilderDefinition)
        {
            return targetType == UpgradeTargetType.Skill && IsSkillStat(targetStat);
        }

        bool hasSkillModifiers = statModifiers != null && statModifiers.Length > 0;
        bool hasTagSkillModifiers = tagStatModifiers != null && tagStatModifiers.Length > 0;

        if (!hasSkillModifiers && !hasTagSkillModifiers)
        {
            return false;
        }

        return (!hasSkillModifiers || ContainsOnlySkillStats(statModifiers)) &&
               (!hasTagSkillModifiers || ContainsOnlySkillStats(tagStatModifiers));
    }

    public bool AffectsOnlyCharacterStats()
    {
        if (useBuilderDefinition)
        {
            return targetType == UpgradeTargetType.Character && !IsSkillStat(targetStat);
        }

        return ContainsOnlyCharacterStats(statModifiers);
    }

    public static bool IsSkillStat(StatType statType)
    {
        return statType is StatType.Damage
            or StatType.CritChance
            or StatType.CritMultiplier
            or StatType.Area
            or StatType.Cooldown
            or StatType.AttackRange
            or StatType.ProjectileSpeed
            or StatType.ProjectileLifetime
            or StatType.AttackPower
            or StatType.SkillFrequency
            or StatType.AreaMultiplier
            or StatType.DoubleAttackChance;
    }

    private static bool ContainsOnlySkillStats(StatModifierData[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < modifiers.Length; i++)
        {
            if (!IsSkillStat(modifiers[i].statType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsOnlyCharacterStats(StatModifierData[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < modifiers.Length; i++)
        {
            if (IsSkillStat(modifiers[i].statType))
            {
                return false;
            }
        }

        return true;
    }
}

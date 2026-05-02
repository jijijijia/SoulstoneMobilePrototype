using System.Text;

public static class UpgradePresentationUtility
{
    public static string BuildDescription(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return string.Empty;
        }

        if (upgrade.UseBuilderDefinition)
        {
            return BuildBuilderDescription(upgrade);
        }

        string baseDescription = upgrade.Description?.Trim() ?? string.Empty;
        string modifierSummary = BuildModifierSummary(upgrade);

        if (string.IsNullOrEmpty(modifierSummary))
        {
            return baseDescription;
        }

        if (string.IsNullOrEmpty(baseDescription))
        {
            return modifierSummary;
        }

        return $"{baseDescription}\n{modifierSummary}";
    }

    private static string BuildBuilderDescription(UpgradeData upgrade)
    {
        string targetLabel = upgrade.TargetType == UpgradeTargetType.Character
            ? "Hero"
            : upgrade.SkillTargetMode == UpgradeSkillTargetMode.AllActiveSkills
                ? "All Active Skills"
                : upgrade.TargetSkill != null
                    ? upgrade.TargetSkill.DisplayName
                    : "Skill";

        string prefix = $"{GetRarityLabel(upgrade.Rarity)} {targetLabel}";
        string modifier = $"{FormatAdditiveValue(upgrade.TargetStat, upgrade.TargetStatValue)} {GetStatLabel(upgrade.TargetStat)}";
        string baseDescription = upgrade.Description?.Trim();

        return string.IsNullOrEmpty(baseDescription)
            ? $"{prefix}: {modifier}"
            : $"{baseDescription}\n{prefix}: {modifier}";
    }

    private static string BuildModifierSummary(UpgradeData upgrade)
    {
        StringBuilder builder = new();
        AppendTargetPrefix(builder, upgrade);
        AppendModifierList(builder, upgrade.StatModifiers);

        if (upgrade.TagStatModifiers != null && upgrade.TagStatModifiers.Length > 0)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            AppendTagModifierList(builder, upgrade);
        }

        return builder.ToString().Trim();
    }

    private static void AppendTargetPrefix(StringBuilder builder, UpgradeData upgrade)
    {
        switch (upgrade.Scope)
        {
            case UpgradeScope.SpecificSkill when upgrade.TargetSkill != null:
                builder.Append(upgrade.TargetSkill.DisplayName);
                builder.Append(" only: ");
                break;
            case UpgradeScope.TaggedSkills when upgrade.TargetTags != null && upgrade.TargetTags.Length > 0:
                builder.Append(string.Join(", ", upgrade.TargetTags));
                builder.Append(" skills: ");
                break;
            case UpgradeScope.CharacterOnly:
                builder.Append("Character: ");
                break;
            case UpgradeScope.GlobalStats when upgrade.AffectsOnlySkillStats():
                builder.Append("All Skills: ");
                break;
            case UpgradeScope.GlobalStats when upgrade.AffectsOnlyCharacterStats():
                builder.Append("Character: ");
                break;
        }
    }

    private static void AppendTagModifierList(StringBuilder builder, UpgradeData upgrade)
    {
        if (upgrade.TagStatModifiers == null || upgrade.TagStatModifiers.Length == 0)
        {
            return;
        }

        if (upgrade.Scope != UpgradeScope.TaggedSkills && upgrade.TargetTags != null && upgrade.TargetTags.Length > 0)
        {
            builder.Append(string.Join(", ", upgrade.TargetTags));
            builder.Append(" skills: ");
        }

        AppendModifierList(builder, upgrade.TagStatModifiers);
    }

    private static void AppendModifierList(StringBuilder builder, StatModifierData[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
        {
            return;
        }

        bool wroteModifier = false;

        for (int i = 0; i < modifiers.Length; i++)
        {
            string formatted = FormatModifier(modifiers[i]);
            if (string.IsNullOrEmpty(formatted))
            {
                continue;
            }

            if (wroteModifier)
            {
                builder.Append(", ");
            }

            builder.Append(formatted);
            wroteModifier = true;
        }
    }

    private static string FormatModifier(StatModifierData modifier)
    {
        if (modifier.additive != 0f)
        {
            return $"{FormatAdditiveValue(modifier.statType, modifier.additive)} {GetStatLabel(modifier.statType)}";
        }

        if (modifier.multiplier != 0f)
        {
            return $"{FormatSignedPercent(modifier.multiplier)} {GetStatLabel(modifier.statType)}";
        }

        return string.Empty;
    }

    private static string FormatAdditiveValue(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.CritChance:
            case StatType.ParryChance:
            case StatType.DoubleAttackChance:
            case StatType.HealthRegenPercent:
                return FormatSignedPercent(value);
            case StatType.CritMultiplier:
            case StatType.AttackPower:
            case StatType.SkillFrequency:
            case StatType.AreaMultiplier:
            case StatType.ExperienceMultiplier:
                return FormatSignedPercent(value);
            case StatType.LifeTotemCount:
            case StatType.DashCharges:
                return FormatSignedFlat(value, true);
            default:
                return FormatSignedFlat(value, false);
        }
    }

    private static string GetStatLabel(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHealth => "Max Health",
            StatType.MoveSpeed => "Move Speed",
            StatType.Damage => "Flat Damage",
            StatType.CritChance => "Critical Chance",
            StatType.CritMultiplier => "Critical Damage",
            StatType.Area => "Area Radius",
            StatType.Cooldown => "Cooldown Factor",
            StatType.PickupRadius => "Pickup Radius",
            StatType.AttackRange => "Attack Range",
            StatType.ContactDamage => "Contact Damage",
            StatType.ProjectileSpeed => "Projectile Speed",
            StatType.ProjectileLifetime => "Projectile Lifetime",
            StatType.SpawnRate => "Spawn Rate",
            StatType.EnemyHealth => "Enemy Health",
            StatType.AttackPower => "Attack Power",
            StatType.SkillFrequency => "Skill Frequency",
            StatType.AreaMultiplier => "Area Multiplier",
            StatType.DoubleAttackChance => "Double Cast Chance",
            StatType.Defense => "Defense",
            StatType.ParryChance => "Parry Chance",
            StatType.ExperienceMultiplier => "XP Gain",
            StatType.DashCharges => "Dash Charges",
            StatType.HealthRegenPercent => "Health Regen",
            StatType.LifeTotemCount => "Life Totem",
            _ => statType.ToString()
        };
    }

    private static string FormatSignedPercent(float value)
    {
        float percent = value * 100f;
        return percent >= 0f ? $"+{percent:0.#}%" : $"{percent:0.#}%";
    }

    private static string FormatSignedFlat(float value, bool wholeNumberOnly)
    {
        if (wholeNumberOnly)
        {
            int rounded = UnityEngine.Mathf.RoundToInt(value);
            return rounded >= 0 ? $"+{rounded}" : rounded.ToString();
        }

        return value >= 0f ? $"+{value:0.##}" : $"{value:0.##}";
    }

    private static string GetRarityLabel(UpgradeRarity rarity)
    {
        return rarity switch
        {
            UpgradeRarity.Common => "Common",
            UpgradeRarity.Uncommon => "Uncommon",
            UpgradeRarity.Rare => "Rare",
            UpgradeRarity.Epic => "Epic",
            UpgradeRarity.Legendary => "Legendary",
            _ => "Upgrade"
        };
    }
}

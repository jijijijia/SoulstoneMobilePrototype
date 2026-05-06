using UnityEngine;

public class SkillRuntimeContext
{
    public CharacterSystem Owner { get; set; }
    public SkillData SkillData { get; set; }
    public RuntimeStats OwnerStats { get; set; }
    public UpgradeSystem UpgradeSystem { get; set; }

    public float ApplyTaggedModifiers(float value, StatType statType)
    {
        return ApplySkillModifiers(value, statType);
    }

    public float ApplySkillModifiers(float value, StatType statType)
    {
        if (SkillData == null)
        {
            return value;
        }

        float additive = 0f;
        float multiplier = 0f;

        if (UpgradeSystem != null)
        {
            UpgradeSystem.GetSkillModifierTotals(SkillData, statType, out additive, out multiplier);
        }

        WeaponSystem weaponSystem = Owner != null ? Owner.WeaponSystem : null;

        if (weaponSystem != null)
        {
            weaponSystem.GetSkillModifierTotals(SkillData, statType, out float weaponAdditive, out float weaponMultiplier);
            additive += weaponAdditive;
            multiplier += weaponMultiplier;
        }

        return (value + additive) * (1f + multiplier);
    }

    public float GetSkillStat(StatType statType)
    {
        float baseValue = SkillData != null ? SkillData.GetCombatStat(statType) : 0f;

        if (baseValue <= 0f && IsMultiplicativeDefaultOneStat(statType))
        {
            baseValue = 1f;
        }

        return ApplySkillModifiers(baseValue, statType);
    }

    public float ResolveCooldown(float baseCooldown, float cooldownReductionPerRank, int rank)
    {
        float skillCooldown = SkillData != null && SkillData.Cooldown > 0f ? SkillData.Cooldown : baseCooldown;

        return CombatMath.ResolveCooldown(
            skillCooldown,
            cooldownReductionPerRank,
            rank,
            GetSkillStat(StatType.Cooldown),
            GetSkillStat(StatType.SkillFrequency));
    }

    public int ResolveDamage(float baseDamage, float damagePerRank, int rank, float attackPowerMultiplier = 1f)
    {
        float resolvedBaseDamage = SkillData != null && SkillData.BaseDamage > 0f ? SkillData.BaseDamage : baseDamage;
        float attackPower = 1f;
        float flatDamageBonus = GetSkillStat(StatType.Damage);
        float totalDamage = CombatMath.ResolveSkillDamage(
            resolvedBaseDamage,
            damagePerRank,
            rank,
            attackPower,
            flatDamageBonus,
            attackPowerMultiplier);

        return CombatMath.RollCriticalDamage(totalDamage, GetSkillStat(StatType.CritChance), GetSkillStat(StatType.CritMultiplier));
    }

    public float ResolveAreaRadius(float baseRadius)
    {
        return CombatMath.ResolveAreaRadius(baseRadius, GetSkillStat(StatType.Area), GetSkillStat(StatType.AreaMultiplier));
    }

    public int GetExecutionCount()
    {
        float doubleAttackChance = Mathf.Max(GetSkillStat(StatType.DoubleAttackChance), GetSkillStat(StatType.MulticastChance));
        return CombatMath.RollExecutionCount(doubleAttackChance);
    }

    private static bool IsMultiplicativeDefaultOneStat(StatType statType)
    {
        return statType is StatType.CritMultiplier
            or StatType.Cooldown
            or StatType.SkillFrequency
            or StatType.AreaMultiplier;
    }
}

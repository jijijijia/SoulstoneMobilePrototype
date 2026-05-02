using System.Collections.Generic;
using System;

public class RuntimeStats
{
    private readonly Dictionary<StatType, float> baseValues = new();
    private readonly Dictionary<string, List<StatModifierData>> modifiersBySource = new();

    public event Action StatsChanged;

    public void Initialize(IEnumerable<StatValue> stats)
    {
        baseValues.Clear();
        modifiersBySource.Clear();

        if (stats == null)
        {
            StatsChanged?.Invoke();
            return;
        }

        foreach (StatValue stat in stats)
        {
            baseValues[stat.statType] = stat.value;
        }

        StatsChanged?.Invoke();
    }

    public float GetValue(StatType statType)
    {
        float baseValue = baseValues.TryGetValue(statType, out float value) ? value : GetDefaultValue(statType);
        float additive = 0f;
        float multiplier = 1f;

        foreach (KeyValuePair<string, List<StatModifierData>> entry in modifiersBySource)
        {
            foreach (StatModifierData modifier in entry.Value)
            {
                if (modifier.statType != statType)
                {
                    continue;
                }

                additive += modifier.additive;
                multiplier += modifier.multiplier;
            }
        }

        return (baseValue + additive) * multiplier;
    }

    public void SetBaseValue(StatType statType, float value)
    {
        baseValues[statType] = value;
        StatsChanged?.Invoke();
    }

    public void AddModifiers(string sourceId, IEnumerable<StatModifierData> modifiers)
    {
        if (string.IsNullOrWhiteSpace(sourceId) || modifiers == null)
        {
            return;
        }

        modifiersBySource[sourceId] = new List<StatModifierData>(modifiers);
        StatsChanged?.Invoke();
    }

    public void RemoveModifiers(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            return;
        }

        modifiersBySource.Remove(sourceId);
        StatsChanged?.Invoke();
    }

    private static float GetDefaultValue(StatType statType)
    {
        return statType switch
        {
            StatType.CritMultiplier => 2f,
            StatType.AttackPower => 1f,
            StatType.SkillFrequency => 1f,
            StatType.AreaMultiplier => 1f,
            StatType.ExperienceMultiplier => 1f,
            StatType.Armor => CombatMath.BaseEnemyArmor,
            _ => 0f
        };
    }
}

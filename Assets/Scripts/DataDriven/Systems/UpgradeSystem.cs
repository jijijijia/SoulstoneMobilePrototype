using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    private readonly Dictionary<string, int> appliedUpgradeStacks = new();
    private readonly List<string> appliedModifierSources = new();

    private CharacterSystem owner;
    public event System.Action SkillModifiersChanged;

    public void Initialize(CharacterSystem characterSystem)
    {
        owner = characterSystem;
        ResetState();
    }

    public IEnumerable<UpgradeData> GetOfferableUpgrades()
    {
        if (owner.CharacterData == null)
        {
            return Enumerable.Empty<UpgradeData>();
        }

        return owner.CharacterData.GlobalUpgradePool.Where(CanOfferUpgrade);
    }

    public bool CanOfferUpgrade(UpgradeData upgradeData)
    {
        if (upgradeData == null)
        {
            return false;
        }

        if (!upgradeData.Repeatable && appliedUpgradeStacks.ContainsKey(upgradeData.UpgradeId))
        {
            return false;
        }

        if (upgradeData.UseBuilderDefinition)
        {
            if (upgradeData.TargetType == UpgradeTargetType.Skill)
            {
                if (upgradeData.SkillTargetMode == UpgradeSkillTargetMode.SpecificSkill &&
                    (upgradeData.TargetSkill == null || !owner.SkillSystem.HasSkill(upgradeData.TargetSkill)))
                {
                    return false;
                }

                if (upgradeData.SkillTargetMode == UpgradeSkillTargetMode.AllActiveSkills &&
                    owner.SkillSystem.ActiveSkills.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        if (upgradeData.Scope == UpgradeScope.SpecificSkill &&
            (upgradeData.TargetSkill == null || !owner.SkillSystem.HasSkill(upgradeData.TargetSkill)))
        {
            return false;
        }

        if (upgradeData.Scope == UpgradeScope.TaggedSkills &&
            (upgradeData.TargetTags == null || upgradeData.TargetTags.Length == 0))
        {
            return false;
        }

        return true;
    }

    public void ApplyUpgrade(UpgradeData upgradeData)
    {
        if (!CanOfferUpgrade(upgradeData))
        {
            return;
        }

        int currentStack = appliedUpgradeStacks.TryGetValue(upgradeData.UpgradeId, out int value) ? value : 0;
        int nextStack = currentStack + 1;
        appliedUpgradeStacks[upgradeData.UpgradeId] = nextStack;

        if (upgradeData.UseBuilderDefinition)
        {
            if (upgradeData.TargetType == UpgradeTargetType.Character)
            {
                string sourceId = $"upgrade:{upgradeData.UpgradeId}:{nextStack}";
                owner.RuntimeStats.AddModifiers(sourceId, upgradeData.GetCharacterModifiers());
                appliedModifierSources.Add(sourceId);
            }

            SkillModifiersChanged?.Invoke();
            return;
        }

        if (upgradeData.Scope == UpgradeScope.GlobalStats || upgradeData.Scope == UpgradeScope.CharacterOnly)
        {
            string sourceId = $"upgrade:{upgradeData.UpgradeId}:{nextStack}";
            owner.RuntimeStats.AddModifiers(sourceId, upgradeData.StatModifiers);
            appliedModifierSources.Add(sourceId);
        }

        SkillModifiersChanged?.Invoke();
    }

    public void GetSkillModifierTotals(SkillData skillData, StatType statType, out float additive, out float multiplier)
    {
        additive = 0f;
        multiplier = 0f;

        if (skillData == null || owner?.CharacterData == null)
        {
            return;
        }

        foreach (UpgradeData upgrade in owner.CharacterData.GlobalUpgradePool)
        {
            if (upgrade == null)
            {
                continue;
            }

            if (!appliedUpgradeStacks.TryGetValue(upgrade.UpgradeId, out int stacks) || stacks <= 0)
            {
                continue;
            }

            if (upgrade.UseBuilderDefinition)
            {
                if (upgrade.TargetType != UpgradeTargetType.Skill || upgrade.TargetStat != statType)
                {
                    continue;
                }

                if (upgrade.SkillTargetMode == UpgradeSkillTargetMode.AllActiveSkills ||
                    upgrade.TargetSkill == skillData)
                {
                    additive += upgrade.TargetStatValue * stacks;
                }

                continue;
            }

            if (upgrade.Scope == UpgradeScope.SpecificSkill)
            {
                if (upgrade.TargetSkill != skillData)
                {
                    continue;
                }

                AccumulateModifiers(upgrade.StatModifiers, statType, stacks, ref additive, ref multiplier);
                continue;
            }

            if (upgrade.Scope == UpgradeScope.TaggedSkills)
            {
                if (!MatchesAnyTag(skillData, upgrade.TargetTags))
                {
                    continue;
                }

                AccumulateModifiers(upgrade.TagStatModifiers, statType, stacks, ref additive, ref multiplier);
            }
        }
    }

    public void ResetState()
    {
        if (owner != null && owner.RuntimeStats != null)
        {
            foreach (string sourceId in appliedModifierSources)
            {
                owner.RuntimeStats.RemoveModifiers(sourceId);
            }
        }

        appliedModifierSources.Clear();
        appliedUpgradeStacks.Clear();
        SkillModifiersChanged?.Invoke();
    }

    private static bool MatchesAnyTag(SkillData skillData, string[] tags)
    {
        if (skillData == null || tags == null || tags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (skillData.HasTag(tags[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static void AccumulateModifiers(StatModifierData[] modifiers, StatType statType, int stacks, ref float additive, ref float multiplier)
    {
        if (modifiers == null || stacks <= 0)
        {
            return;
        }

        foreach (StatModifierData modifier in modifiers)
        {
            if (modifier.statType != statType)
            {
                continue;
            }

            additive += modifier.additive * stacks;
            multiplier += modifier.multiplier * stacks;
        }
    }
}

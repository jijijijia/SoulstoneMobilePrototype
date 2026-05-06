using System.Collections.Generic;
using UnityEngine;

public class ProgressionChoiceGenerator
{
    public bool HasSkillChoices(SkillSystem skillSystem)
    {
        return skillSystem != null &&
               skillSystem.ActiveSkills.Count < skillSystem.MaxSkills &&
               CountAvailableSkills(skillSystem) > 0;
    }

    public bool HasUpgradeChoices(UpgradeSystem upgradeSystem)
    {
        return upgradeSystem != null && GetOfferableUpgrades(upgradeSystem).Count > 0;
    }

    public List<ProgressionChoice> GenerateSkillChoices(SkillSystem skillSystem)
    {
        List<ProgressionChoice> choices = new();

        if (!HasSkillChoices(skillSystem))
        {
            return choices;
        }

        List<SkillData> skillOffers = GetRandomSkills(skillSystem, 3);

        for (int i = 0; i < skillOffers.Count; i++)
        {
            choices.Add(BuildSkillChoice(skillSystem, skillOffers[i]));
        }

        return choices;
    }

    public List<ProgressionChoice> GenerateUpgradeChoices(UpgradeSystem upgradeSystem)
    {
        List<ProgressionChoice> choices = new();
        List<UpgradeData> upgradeOffers = GetWeightedRandomUpgrades(upgradeSystem, 3);

        for (int i = 0; i < upgradeOffers.Count; i++)
        {
            choices.Add(BuildUpgradeChoice(upgradeSystem, upgradeOffers[i]));
        }

        return choices;
    }

    private static ProgressionChoice BuildSkillChoice(SkillSystem skillSystem, SkillData skill)
    {
        return new ProgressionChoice
        {
            TypeLabel = "Active Skill",
            Title = skill.DisplayName,
            Description = skill.Description,
            RankText = $"{skill.HitType} / {skill.Element}",
            Icon = skill.Icon,
            ApplyAction = () => skillSystem.AcquireSkill(skill)
        };
    }

    private static ProgressionChoice BuildUpgradeChoice(UpgradeSystem upgradeSystem, UpgradeData upgrade)
    {
        return new ProgressionChoice
        {
            TypeLabel = GetUpgradeTypeLabel(upgrade),
            Title = upgrade.DisplayName,
            Description = UpgradePresentationUtility.BuildDescription(upgrade),
            RankText = upgrade.Repeatable ? "Stacking" : "Unique",
            Icon = upgrade.Icon,
            ApplyAction = () => upgradeSystem.ApplyUpgrade(upgrade)
        };
    }

    private static string GetUpgradeTypeLabel(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return "Passive";
        }

        return upgrade.Scope switch
        {
            UpgradeScope.SpecificSkill => "Skill Stat",
            UpgradeScope.TaggedSkills => "Tag Synergy",
            UpgradeScope.CharacterOnly => "Character",
            UpgradeScope.GlobalStats when upgrade.AffectsOnlySkillStats() => "All Skills",
            UpgradeScope.GlobalStats when upgrade.AffectsOnlyCharacterStats() => "Character",
            _ => "Passive"
        };
    }

    private static int CountAvailableSkills(SkillSystem skillSystem)
    {
        int count = 0;

        foreach (SkillData _ in skillSystem.GetNewSkillOffers())
        {
            count++;
        }

        return count;
    }

    private static List<SkillData> GetRandomSkills(SkillSystem skillSystem, int count)
    {
        List<SkillData> pool = GetSkillPool(skillSystem);
        return PickRandomWithoutReplacement(pool, count);
    }

    private static List<SkillData> GetSkillPool(SkillSystem skillSystem)
    {
        List<SkillData> pool = new();

        if (skillSystem == null)
        {
            return pool;
        }

        foreach (SkillData skill in skillSystem.GetNewSkillOffers())
        {
            if (skill != null)
            {
                pool.Add(skill);
            }
        }

        return pool;
    }

    private static List<UpgradeData> GetOfferableUpgrades(UpgradeSystem upgradeSystem)
    {
        List<UpgradeData> pool = new();

        foreach (UpgradeData upgrade in upgradeSystem.GetOfferableUpgrades())
        {
            if (upgrade != null)
            {
                pool.Add(upgrade);
            }
        }

        return pool;
    }

    private static List<UpgradeData> GetWeightedRandomUpgrades(UpgradeSystem upgradeSystem, int count)
    {
        List<UpgradeData> pool = GetOfferableUpgrades(upgradeSystem);
        List<UpgradeData> result = new();

        while (result.Count < count && pool.Count > 0)
        {
            UpgradeData picked = PickWeightedUpgrade(pool);

            if (picked == null)
            {
                break;
            }

            result.Add(picked);
        }

        return result;
    }

    private static UpgradeData PickWeightedUpgrade(List<UpgradeData> pool)
    {
        if (pool == null || pool.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            totalWeight += GetRarityWeight(pool[i].Rarity);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            cumulativeWeight += GetRarityWeight(pool[i].Rarity);

            if (randomValue > cumulativeWeight && i < pool.Count - 1)
            {
                continue;
            }

            UpgradeData picked = pool[i];
            pool.RemoveAt(i);
            return picked;
        }

        return null;
    }

    private static List<T> PickRandomWithoutReplacement<T>(List<T> pool, int count)
    {
        List<T> result = new();
        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    private static float GetRarityWeight(UpgradeRarity rarity)
    {
        return rarity switch
        {
            UpgradeRarity.Common => 60f * SkillTreeSystem.GetRarityWeightMultiplier(rarity),
            UpgradeRarity.Uncommon => 25f * SkillTreeSystem.GetRarityWeightMultiplier(rarity),
            UpgradeRarity.Rare => 10f * SkillTreeSystem.GetRarityWeightMultiplier(rarity),
            UpgradeRarity.Epic => 4f * SkillTreeSystem.GetRarityWeightMultiplier(rarity),
            UpgradeRarity.Legendary => 1f * SkillTreeSystem.GetRarityWeightMultiplier(rarity),
            _ => 1f
        };
    }
}

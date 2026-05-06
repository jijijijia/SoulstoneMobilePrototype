using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ForgeSystem
{
    public static WeaponProgressData GetProgress(WeaponData weapon)
    {
        if (weapon == null)
        {
            return null;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;
        WeaponProgressData progress = profile.EnsureWeaponProgress(weapon.WeaponId, weapon.MaxLevel);
        bool unlocked = UnlockSystem.IsUnlocked(weapon);

        if (unlocked && !progress.unlocked)
        {
            progress.unlocked = true;
            progress.level = Mathf.Max(1, progress.level);
            SaveSystem.Save();
        }

        progress.maxLevel = Mathf.Max(progress.maxLevel, weapon.MaxLevel);
        progress.level = Mathf.Clamp(progress.level, progress.unlocked ? 1 : 0, progress.maxLevel);
        return progress;
    }

    public static bool CanImprove(WeaponData weapon)
    {
        WeaponProgressData progress = GetProgress(weapon);

        if (weapon == null || progress == null || !progress.unlocked || progress.level >= weapon.MaxLevel)
        {
            return false;
        }

        return CanAfford(GetTotalUpgradeCost(weapon, progress.level));
    }

    public static bool TryImprove(WeaponData weapon)
    {
        WeaponProgressData progress = GetProgress(weapon);

        if (weapon == null || progress == null || !progress.unlocked || progress.level >= weapon.MaxLevel)
        {
            return false;
        }

        CurrencyAmount[] cost = GetTotalUpgradeCost(weapon, progress.level);

        if (!CanAfford(cost))
        {
            return false;
        }

        for (int i = 0; i < cost.Length; i++)
        {
            CurrencyAmount amount = cost[i];

            if (amount == null || amount.amount <= 0)
            {
                continue;
            }

            SaveSystem.Wallet.Spend(amount.currencyType, amount.amount, save: false);
        }

        progress.level = Mathf.Min(weapon.MaxLevel, progress.level + 1);
        progress.maxLevel = Mathf.Max(progress.maxLevel, weapon.MaxLevel);
        progress.unlocked = true;
        SaveSystem.Save();
        return true;
    }

    public static StatModifierData[] GetCharacterStatModifiers(WeaponData weapon)
    {
        WeaponProgressData progress = GetProgress(weapon);
        return BuildScaledModifiers(weapon != null ? weapon.StatGrowthPerLevel : null, progress != null ? progress.level - 1 : 0);
    }

    public static void GetSkillModifierTotals(WeaponData weapon, SkillData skillData, StatType statType, out float additive, out float multiplier)
    {
        additive = 0f;
        multiplier = 0f;

        if (weapon == null || skillData == null || weapon.StartingSkill != skillData)
        {
            return;
        }

        WeaponProgressData progress = GetProgress(weapon);
        int bonusLevels = progress != null ? Mathf.Max(0, progress.level - 1) : 0;
        StatModifierData[] modifiers = weapon.SkillGrowthPerLevel;

        if (modifiers == null || bonusLevels <= 0)
        {
            return;
        }

        for (int i = 0; i < modifiers.Length; i++)
        {
            if (modifiers[i].statType != statType)
            {
                continue;
            }

            additive += modifiers[i].additive * bonusLevels;
            multiplier += modifiers[i].multiplier * bonusLevels;
        }
    }

    public static CurrencyAmount[] GetTotalUpgradeCost(WeaponData weapon, int currentLevel)
    {
        if (weapon == null)
        {
            return new CurrencyAmount[0];
        }

        Dictionary<CurrencyType, int> totals = new();
        AddCosts(totals, weapon.GetForgeUpgradeCost(currentLevel));
        AddCosts(totals, weapon.RequiredMaterials);

        CurrencyAmount[] result = new CurrencyAmount[totals.Count];
        int index = 0;

        foreach (KeyValuePair<CurrencyType, int> entry in totals)
        {
            result[index++] = new CurrencyAmount
            {
                currencyType = entry.Key,
                amount = Mathf.Max(0, entry.Value)
            };
        }

        return result;
    }

    public static string BuildCostLine(WeaponData weapon)
    {
        WeaponProgressData progress = GetProgress(weapon);

        if (weapon == null || progress == null)
        {
            return "Стоимость: нет данных";
        }

        if (!progress.unlocked)
        {
            return "Оружие не разблокировано";
        }

        if (progress.level >= weapon.MaxLevel)
        {
            return "Максимальный уровень";
        }

        CurrencyAmount[] cost = GetTotalUpgradeCost(weapon, progress.level);

        if (cost.Length == 0)
        {
            return "Стоимость: бесплатно";
        }

        StringBuilder sb = new("Стоимость улучшения: ");

        for (int i = 0; i < cost.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(GetCurrencyDisplayName(cost[i].currencyType));
            sb.Append(" ");
            sb.Append(cost[i].amount);
        }

        return sb.ToString();
    }

    private static bool CanAfford(CurrencyAmount[] cost)
    {
        if (cost == null)
        {
            return true;
        }

        for (int i = 0; i < cost.Length; i++)
        {
            CurrencyAmount amount = cost[i];

            if (amount != null && amount.amount > 0 && !SaveSystem.Wallet.CanAfford(amount.currencyType, amount.amount))
            {
                return false;
            }
        }

        return true;
    }

    private static StatModifierData[] BuildScaledModifiers(StatModifierData[] perLevel, int bonusLevels)
    {
        if (perLevel == null || perLevel.Length == 0 || bonusLevels <= 0)
        {
            return new StatModifierData[0];
        }

        StatModifierData[] scaled = new StatModifierData[perLevel.Length];

        for (int i = 0; i < perLevel.Length; i++)
        {
            scaled[i] = new StatModifierData
            {
                statType = perLevel[i].statType,
                additive = perLevel[i].additive * bonusLevels,
                multiplier = perLevel[i].multiplier * bonusLevels
            };
        }

        return scaled;
    }

    private static void AddCosts(Dictionary<CurrencyType, int> totals, CurrencyAmount[] costs)
    {
        if (totals == null || costs == null)
        {
            return;
        }

        for (int i = 0; i < costs.Length; i++)
        {
            CurrencyAmount cost = costs[i];

            if (cost == null || cost.amount <= 0)
            {
                continue;
            }

            totals.TryGetValue(cost.currencyType, out int current);
            totals[cost.currencyType] = current + cost.amount;
        }
    }

    private static string GetCurrencyDisplayName(CurrencyType currencyType)
    {
        return currencyType switch
        {
            CurrencyType.SoulShards => "осколки душ",
            CurrencyType.RedCrystal => "редкие кристаллы",
            CurrencyType.WeaponOre => "материалы оружия",
            CurrencyType.RuneDust => "руническая пыль",
            CurrencyType.BlueCrystal => "синие кристаллы",
            CurrencyType.AmberCrystal => "янтарные кристаллы",
            CurrencyType.GreenCrystal => "зелёные кристаллы",
            CurrencyType.VoidCrystal => "кристаллы бездны",
            _ => currencyType.ToString()
        };
    }
}

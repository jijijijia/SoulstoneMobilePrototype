using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfileData
{
    public int schemaVersion = 1;
    public string profileId = "profile_1";
    public string selectedCharacterId;
    public string selectedWeaponId;
    public string selectedMapId;
    public List<string> unlockedCharacterIds = new();
    public List<string> unlockedWeaponIds = new();
    public List<string> unlockedMapIds = new();
    public List<WeaponProgressData> weaponProgress = new();
    public List<CurrencyAmount> currencies = new();
    public List<SkillTreeNodeProgress> skillTreeProgress = new();
    public List<RuneProgress> runeProgress = new();
    public List<string> equippedRuneIds = new();
    public List<AchievementProgress> achievements = new();

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            profileId = GameProfileStore.ActiveProfileId;
        }

        unlockedCharacterIds ??= new List<string>();
        unlockedWeaponIds ??= new List<string>();
        unlockedMapIds ??= new List<string>();
        weaponProgress ??= new List<WeaponProgressData>();
        currencies ??= new List<CurrencyAmount>();
        skillTreeProgress ??= new List<SkillTreeNodeProgress>();
        runeProgress ??= new List<RuneProgress>();
        equippedRuneIds ??= new List<string>();
        achievements ??= new List<AchievementProgress>();

        EnsureCurrency(CurrencyType.SoulShards, 0);
        EnsureCurrency(CurrencyType.RedCrystal, 0);
        EnsureCurrency(CurrencyType.BlueCrystal, 0);
        EnsureCurrency(CurrencyType.AmberCrystal, 0);
        EnsureCurrency(CurrencyType.GreenCrystal, 0);
        EnsureCurrency(CurrencyType.VoidCrystal, 0);
        EnsureCurrency(CurrencyType.RuneDust, 0);
        EnsureCurrency(CurrencyType.WeaponOre, 0);
    }

    public bool IsCharacterUnlocked(string characterId) => ContainsId(unlockedCharacterIds, characterId);
    public bool IsWeaponUnlocked(string weaponId) => ContainsId(unlockedWeaponIds, weaponId);
    public bool IsMapUnlocked(string mapId) => ContainsId(unlockedMapIds, mapId);
    public WeaponProgressData GetWeaponProgress(string weaponId) => FindWeaponProgress(weaponId);

    public void UnlockCharacter(string characterId) => AddUniqueId(unlockedCharacterIds, characterId);
    public void UnlockWeapon(string weaponId)
    {
        AddUniqueId(unlockedWeaponIds, weaponId);

        if (!string.IsNullOrWhiteSpace(weaponId))
        {
            WeaponProgressData progress = EnsureWeaponProgress(weaponId, 1);
            progress.unlocked = true;
            progress.level = Mathf.Max(1, progress.level);
        }
    }
    public void UnlockMap(string mapId) => AddUniqueId(unlockedMapIds, mapId);

    public WeaponProgressData EnsureWeaponProgress(string weaponId, int maxLevel)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return null;
        }

        WeaponProgressData existing = FindWeaponProgress(weaponId);

        if (existing != null)
        {
            existing.maxLevel = Mathf.Max(existing.maxLevel, maxLevel, 1);
            existing.level = Mathf.Clamp(existing.level, existing.unlocked ? 1 : 0, existing.maxLevel);
            return existing;
        }

        WeaponProgressData created = new()
        {
            weaponId = weaponId,
            level = 0,
            maxLevel = Mathf.Max(1, maxLevel),
            unlocked = IsWeaponUnlocked(weaponId),
            equipped = false,
            selected = false
        };

        if (created.unlocked)
        {
            created.level = 1;
        }

        weaponProgress.Add(created);
        return created;
    }

    public void SetSelectedWeaponProgress(string weaponId)
    {
        if (weaponProgress == null)
        {
            weaponProgress = new List<WeaponProgressData>();
        }

        for (int i = 0; i < weaponProgress.Count; i++)
        {
            if (weaponProgress[i] == null)
            {
                continue;
            }

            bool selectedWeapon = string.Equals(weaponProgress[i].weaponId, weaponId, StringComparison.OrdinalIgnoreCase);
            weaponProgress[i].selected = selectedWeapon;
            weaponProgress[i].equipped = selectedWeapon;
        }
    }

    public int GetCurrency(CurrencyType currencyType)
    {
        CurrencyAmount amount = FindCurrency(currencyType);
        return amount != null ? Mathf.Max(0, amount.amount) : 0;
    }

    public void SetCurrency(CurrencyType currencyType, int amount)
    {
        CurrencyAmount currency = EnsureCurrency(currencyType, 0);
        currency.amount = Mathf.Max(0, amount);
    }

    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetCurrency(currencyType, GetCurrency(currencyType) + amount);
    }

    public bool SpendCurrency(CurrencyType currencyType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = GetCurrency(currencyType);

        if (current < amount)
        {
            return false;
        }

        SetCurrency(currencyType, current - amount);
        return true;
    }

    private CurrencyAmount EnsureCurrency(CurrencyType currencyType, int defaultAmount)
    {
        CurrencyAmount existing = FindCurrency(currencyType);

        if (existing != null)
        {
            existing.amount = Mathf.Max(0, existing.amount);
            return existing;
        }

        CurrencyAmount created = new()
        {
            currencyType = currencyType,
            amount = Mathf.Max(0, defaultAmount)
        };
        currencies.Add(created);
        return created;
    }

    private CurrencyAmount FindCurrency(CurrencyType currencyType)
    {
        for (int i = 0; i < currencies.Count; i++)
        {
            if (currencies[i] != null && currencies[i].currencyType == currencyType)
            {
                return currencies[i];
            }
        }

        return null;
    }

    private WeaponProgressData FindWeaponProgress(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId) || weaponProgress == null)
        {
            return null;
        }

        for (int i = 0; i < weaponProgress.Count; i++)
        {
            if (weaponProgress[i] != null && string.Equals(weaponProgress[i].weaponId, weaponId, StringComparison.OrdinalIgnoreCase))
            {
                return weaponProgress[i];
            }
        }

        return null;
    }

    private static bool ContainsId(List<string> ids, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || ids == null)
        {
            return false;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddUniqueId(List<string> ids, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || ContainsId(ids, id))
        {
            return;
        }

        ids.Add(id);
    }
}

[Serializable]
public class CurrencyAmount
{
    public CurrencyType currencyType;
    public int amount;
}

[Serializable]
public class WeaponProgressData
{
    public string weaponId;
    public int level = 1;
    public int maxLevel = 10;
    public bool unlocked;
    public bool equipped;
    public bool selected;
}

[Serializable]
public class SkillTreeNodeProgress
{
    public string nodeId;
    public int level;
}

[Serializable]
public class RuneProgress
{
    public string runeId;
    public bool unlocked;
    public int level;
}

[Serializable]
public class AchievementProgress
{
    public string achievementId;
    public int currentValue;
    public bool completed;
    public bool rewardClaimed;
}

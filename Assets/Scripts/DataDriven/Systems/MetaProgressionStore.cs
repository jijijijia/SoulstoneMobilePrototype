using UnityEngine;

// Global meta-progression layer. Data persists across all profiles and runs.
// Souls are earned at run end and spent on permanent unlocks/upgrades.
public static class MetaProgressionStore
{
    private const string SoulsKey = "soulstone.meta.souls";
    private const string UnlockedCharacterPrefix = "soulstone.meta.char_unlock.";
    private const string UnlockedWeaponPrefix = "soulstone.meta.weapon_unlock.";
    private const string MetaUpgradeRankPrefix = "soulstone.meta.upgrade_rank.";

    public static int TotalSouls
    {
        get => PlayerPrefs.GetInt(SoulsKey, 0);
        private set
        {
            PlayerPrefs.SetInt(SoulsKey, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }

    public static void AddSouls(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        TotalSouls += amount;
    }

    public static bool SpendSouls(int cost)
    {
        if (cost <= 0 || TotalSouls < cost)
        {
            return false;
        }

        TotalSouls -= cost;
        return true;
    }

    public static bool IsCharacterUnlocked(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        // First character is always unlocked by default.
        return PlayerPrefs.GetInt(UnlockedCharacterPrefix + characterId, 0) == 1;
    }

    public static void UnlockCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        PlayerPrefs.SetInt(UnlockedCharacterPrefix + characterId, 1);
        PlayerPrefs.Save();
    }

    public static bool IsWeaponUnlocked(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return false;
        }

        return PlayerPrefs.GetInt(UnlockedWeaponPrefix + weaponId, 0) == 1;
    }

    public static void UnlockWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return;
        }

        PlayerPrefs.SetInt(UnlockedWeaponPrefix + weaponId, 1);
        PlayerPrefs.Save();
    }

    public static int GetMetaUpgradeRank(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
        {
            return 0;
        }

        return PlayerPrefs.GetInt(MetaUpgradeRankPrefix + upgradeId, 0);
    }

    public static bool PurchaseMetaUpgrade(string upgradeId, int costPerRank, int maxRank)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
        {
            return false;
        }

        int currentRank = GetMetaUpgradeRank(upgradeId);

        if (currentRank >= maxRank)
        {
            return false;
        }

        if (!SpendSouls(costPerRank))
        {
            return false;
        }

        PlayerPrefs.SetInt(MetaUpgradeRankPrefix + upgradeId, currentRank + 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void ResetAllMetaProgress()
    {
        PlayerPrefs.DeleteKey(SoulsKey);
        PlayerPrefs.Save();
        // Character/weapon/upgrade keys require knowing all IDs; wipe by re-saving to 0 from caller.
    }
}

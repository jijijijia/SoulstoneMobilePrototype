public static class UnlockSystem
{
    public static bool IsUnlocked(CharacterData character)
    {
        if (character == null)
        {
            return false;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;
        return character.UnlockedByDefault || profile.IsCharacterUnlocked(character.UnlockId) || profile.IsCharacterUnlocked(character.CharacterId);
    }

    public static bool IsUnlocked(WeaponData weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;
        return weapon.UnlockedByDefault || profile.IsWeaponUnlocked(weapon.UnlockId) || profile.IsWeaponUnlocked(weapon.WeaponId);
    }

    public static bool IsUnlocked(MapData map)
    {
        if (map == null)
        {
            return false;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;
        return map.UnlockedByDefault || profile.IsMapUnlocked(map.UnlockId) || profile.IsMapUnlocked(map.MapId);
    }

    public static bool IsMapUnlocked(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return false;
        }

        return SaveSystem.CurrentProfile.IsMapUnlocked(mapId);
    }

    public static bool CanUnlock(CharacterData character) => character != null && !IsUnlocked(character) && SaveSystem.Wallet.CanAfford(character.UnlockCurrency, character.UnlockCost);
    public static bool CanUnlock(WeaponData weapon) => weapon != null && !IsUnlocked(weapon) && SaveSystem.Wallet.CanAfford(weapon.UnlockCurrency, weapon.UnlockCost);
    public static bool CanUnlock(MapData map) => map != null && !IsUnlocked(map) && SaveSystem.Wallet.CanAfford(map.UnlockCurrency, map.UnlockCost);

    public static bool TryUnlock(CharacterData character)
    {
        if (character == null)
        {
            return false;
        }

        if (IsUnlocked(character))
        {
            return true;
        }

        if (!SaveSystem.Wallet.Spend(character.UnlockCurrency, character.UnlockCost, save: false))
        {
            return false;
        }

        SaveSystem.UnlockCharacter(character.UnlockId, save: false);
        SaveSystem.UnlockCharacter(character.CharacterId, save: false);
        SaveSystem.Save();
        return true;
    }

    public static bool TryUnlock(WeaponData weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        if (IsUnlocked(weapon))
        {
            return true;
        }

        if (!SaveSystem.Wallet.Spend(weapon.UnlockCurrency, weapon.UnlockCost, save: false))
        {
            return false;
        }

        SaveSystem.UnlockWeapon(weapon.UnlockId, save: false);
        SaveSystem.UnlockWeapon(weapon.WeaponId, save: false);
        SaveSystem.Save();
        return true;
    }

    public static bool TryUnlock(MapData map)
    {
        if (map == null)
        {
            return false;
        }

        if (IsUnlocked(map))
        {
            return true;
        }

        if (!SaveSystem.Wallet.Spend(map.UnlockCurrency, map.UnlockCost, save: false))
        {
            return false;
        }

        SaveSystem.UnlockMap(map.UnlockId, save: false);
        SaveSystem.UnlockMap(map.MapId, save: false);
        SaveSystem.Save();
        return true;
    }

    public static string GetRequirementText(CharacterData character)
    {
        if (character == null)
        {
            return "Герой недоступен.";
        }

        return $"{character.UnlockRequirementText}\nСтоимость: {character.UnlockCost} {character.UnlockCurrency}";
    }

    public static string GetRequirementText(WeaponData weapon)
    {
        if (weapon == null)
        {
            return "Оружие недоступно.";
        }

        return $"{weapon.UnlockRequirementText}\nСтоимость: {weapon.UnlockCost} {weapon.UnlockCurrency}";
    }

    public static string GetRequirementText(MapData map)
    {
        if (map == null)
        {
            return "Карта недоступна.";
        }

        return $"{map.UnlockRequirementText}\nСтоимость: {map.UnlockCost} {map.UnlockCurrency}";
    }
}

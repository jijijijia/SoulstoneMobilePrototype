using UnityEngine;

public static class SelectedLoadoutStore
{
    private const string SelectedMapIdKey = "soulstone.selected_map_id";
    private const string SelectedWeaponPrefix = "soulstone.selected_weapon_id.";
    private const string ProfileSelectedMapIdKey = "selected_map_id";
    private const string ProfileSelectedWeaponPrefix = "selected_weapon_id.";
    private const string DefaultMapDatabaseResourcePath = "MapDatabase";

    private static MapDatabase cachedMapDatabase;

    public static MapDatabase LoadMapDatabase()
    {
        if (cachedMapDatabase == null)
        {
            cachedMapDatabase = Resources.Load<MapDatabase>(DefaultMapDatabaseResourcePath);
        }

        return cachedMapDatabase;
    }

    public static string GetSelectedMapId()
    {
        PlayerProfileData profile = SaveSystem.CurrentProfile;

        if (profile != null && !string.IsNullOrWhiteSpace(profile.selectedMapId))
        {
            return profile.selectedMapId;
        }

        string profileKey = GameProfileStore.GetProfileKey(ProfileSelectedMapIdKey);
        return PlayerPrefs.GetString(profileKey, PlayerPrefs.GetString(SelectedMapIdKey, string.Empty));
    }

    public static void SetSelectedMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        SaveSystem.SetSelectedMap(mapId);
        PlayerPrefs.SetString(GameProfileStore.GetProfileKey(ProfileSelectedMapIdKey), mapId);
        PlayerPrefs.Save();
    }

    public static MapData ResolveSelectedMap()
    {
        MapDatabase database = LoadMapDatabase();
        MapData selectedMap = database != null ? database.FindById(GetSelectedMapId()) : null;

        if (selectedMap != null)
        {
            return selectedMap;
        }

        MapData firstMap = database != null ? database.GetFirstMap() : null;

        if (firstMap != null)
        {
            SetSelectedMap(firstMap.MapId);
        }

        return firstMap;
    }

    public static string GetSelectedWeaponId(CharacterData characterData)
    {
        if (characterData == null || string.IsNullOrWhiteSpace(characterData.CharacterId))
        {
            return string.Empty;
        }

        PlayerProfileData profile = SaveSystem.CurrentProfile;

        if (profile != null && !string.IsNullOrWhiteSpace(profile.selectedWeaponId))
        {
            return profile.selectedWeaponId;
        }

        string profileKey = GetProfileWeaponKey(characterData);
        return PlayerPrefs.GetString(profileKey, PlayerPrefs.GetString(GetWeaponKey(characterData), string.Empty));
    }

    public static void SetSelectedWeapon(CharacterData characterData, WeaponData weaponData)
    {
        if (characterData == null || weaponData == null ||
            string.IsNullOrWhiteSpace(characterData.CharacterId) ||
            string.IsNullOrWhiteSpace(weaponData.WeaponId))
        {
            return;
        }

        SaveSystem.SetSelectedWeapon(weaponData.WeaponId);
        PlayerPrefs.SetString(GetProfileWeaponKey(characterData), weaponData.WeaponId);
        PlayerPrefs.Save();
    }

    public static WeaponData ResolveSelectedWeapon(CharacterData characterData)
    {
        if (characterData == null)
        {
            return null;
        }

        string selectedWeaponId = GetSelectedWeaponId(characterData);

        if (!string.IsNullOrWhiteSpace(selectedWeaponId) && characterData.AvailableWeapons != null)
        {
            for (int i = 0; i < characterData.AvailableWeapons.Length; i++)
            {
                WeaponData weapon = characterData.AvailableWeapons[i];

                if (weapon != null && string.Equals(weapon.WeaponId, selectedWeaponId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return weapon;
                }
            }
        }

        if (characterData.StartingWeapon != null)
        {
            SetSelectedWeapon(characterData, characterData.StartingWeapon);
            return characterData.StartingWeapon;
        }

        if (characterData.AvailableWeapons != null)
        {
            for (int i = 0; i < characterData.AvailableWeapons.Length; i++)
            {
                if (characterData.AvailableWeapons[i] != null)
                {
                    SetSelectedWeapon(characterData, characterData.AvailableWeapons[i]);
                    return characterData.AvailableWeapons[i];
                }
            }
        }

        return null;
    }

    private static string GetWeaponKey(CharacterData characterData)
    {
        return SelectedWeaponPrefix + characterData.CharacterId;
    }

    private static string GetProfileWeaponKey(CharacterData characterData)
    {
        return GameProfileStore.GetProfileKey(ProfileSelectedWeaponPrefix + characterData.CharacterId);
    }
}

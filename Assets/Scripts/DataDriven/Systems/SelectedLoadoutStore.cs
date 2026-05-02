using UnityEngine;

public static class SelectedLoadoutStore
{
    private const string SelectedMapIdKey = "soulstone.selected_map_id";
    private const string SelectedWeaponPrefix = "soulstone.selected_weapon_id.";
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
        return PlayerPrefs.GetString(SelectedMapIdKey, string.Empty);
    }

    public static void SetSelectedMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        PlayerPrefs.SetString(SelectedMapIdKey, mapId);
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

        return PlayerPrefs.GetString(GetWeaponKey(characterData), string.Empty);
    }

    public static void SetSelectedWeapon(CharacterData characterData, WeaponData weaponData)
    {
        if (characterData == null || weaponData == null ||
            string.IsNullOrWhiteSpace(characterData.CharacterId) ||
            string.IsNullOrWhiteSpace(weaponData.WeaponId))
        {
            return;
        }

        PlayerPrefs.SetString(GetWeaponKey(characterData), weaponData.WeaponId);
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
}

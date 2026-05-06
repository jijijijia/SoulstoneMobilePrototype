using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "player_profile.json";

    private static PlayerProfileData currentProfile;
    private static CurrencyWallet wallet;

    public static event Action<PlayerProfileData> ProfileLoaded;
    public static event Action<PlayerProfileData> ProfileSaved;

    public static PlayerProfileData CurrentProfile
    {
        get
        {
            EnsureLoaded();
            return currentProfile;
        }
    }

    public static CurrencyWallet Wallet
    {
        get
        {
            EnsureLoaded();
            return wallet;
        }
    }

    public static string SavePath => Path.Combine(Application.persistentDataPath, GameProfileStore.ActiveProfileId + "_" + SaveFileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        currentProfile = null;
        wallet = null;
        ProfileLoaded = null;
        ProfileSaved = null;
    }

    public static void EnsureLoaded()
    {
        if (currentProfile != null)
        {
            return;
        }

        LoadOrCreate();
    }

    public static PlayerProfileData LoadOrCreate()
    {
        currentProfile = TryLoadFromDisk();

        if (currentProfile == null)
        {
            currentProfile = CreateDefaultProfile();
            Save();
        }

        currentProfile.Normalize();
        wallet = new CurrencyWallet(currentProfile);
        ProfileLoaded?.Invoke(currentProfile);
        return currentProfile;
    }

    public static void Save()
    {
        EnsureLoaded();

        try
        {
            currentProfile.Normalize();
            string directory = Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(currentProfile, true);
            File.WriteAllText(SavePath, json);
            ProfileSaved?.Invoke(currentProfile);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save player profile to '{SavePath}': {exception}");
        }
    }

    public static void EnsureContentDefaults(CharacterRoster roster, MapDatabase mapDatabase)
    {
        EnsureLoaded();
        bool changed = false;

        CharacterData firstCharacter = roster != null ? roster.GetFirstCharacter() : null;

        if (roster != null && roster.Characters != null)
        {
            for (int i = 0; i < roster.Characters.Length; i++)
            {
                CharacterData character = roster.Characters[i];

                if (character == null || !character.UnlockedByDefault)
                {
                    continue;
                }

                changed |= UnlockCharacter(character.UnlockId, save: false);
                changed |= UnlockCharacter(character.CharacterId, save: false);
            }
        }

        if (firstCharacter != null)
        {
            changed |= UnlockCharacter(firstCharacter.UnlockId, save: false);
            changed |= UnlockCharacter(firstCharacter.CharacterId, save: false);

            if (string.IsNullOrWhiteSpace(currentProfile.selectedCharacterId))
            {
                currentProfile.selectedCharacterId = firstCharacter.CharacterId;
                changed = true;
            }

            WeaponData firstWeapon = firstCharacter.StartingWeapon ?? GetFirstWeapon(firstCharacter);

            if (firstWeapon != null)
            {
                changed |= UnlockWeapon(firstWeapon.UnlockId, save: false);
                changed |= UnlockWeapon(firstWeapon.WeaponId, save: false);
                currentProfile.EnsureWeaponProgress(firstWeapon.WeaponId, firstWeapon.MaxLevel);

                if (string.IsNullOrWhiteSpace(currentProfile.selectedWeaponId))
                {
                    currentProfile.selectedWeaponId = firstWeapon.WeaponId;
                    currentProfile.SetSelectedWeaponProgress(firstWeapon.WeaponId);
                    changed = true;
                }
            }
        }

        MapData firstMap = mapDatabase != null ? mapDatabase.GetFirstMap() : null;

        if (mapDatabase != null && mapDatabase.Maps != null)
        {
            for (int i = 0; i < mapDatabase.Maps.Length; i++)
            {
                MapData map = mapDatabase.Maps[i];

                if (map == null || !map.UnlockedByDefault)
                {
                    continue;
                }

                changed |= UnlockMap(map.UnlockId, save: false);
                changed |= UnlockMap(map.MapId, save: false);
            }
        }

        if (firstMap != null)
        {
            changed |= UnlockMap(firstMap.UnlockId, save: false);
            changed |= UnlockMap(firstMap.MapId, save: false);

            if (string.IsNullOrWhiteSpace(currentProfile.selectedMapId))
            {
                currentProfile.selectedMapId = firstMap.MapId;
                changed = true;
            }
        }

        if (changed)
        {
            Save();
        }
    }

    public static void SetSelectedCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        EnsureLoaded();
        currentProfile.selectedCharacterId = characterId;
        currentProfile.UnlockCharacter(characterId);
        Save();
    }

    public static void SetSelectedWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return;
        }

        EnsureLoaded();
        currentProfile.selectedWeaponId = weaponId;
        currentProfile.UnlockWeapon(weaponId);
        currentProfile.SetSelectedWeaponProgress(weaponId);
        Save();
    }

    public static void SetSelectedMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        EnsureLoaded();
        currentProfile.selectedMapId = mapId;
        currentProfile.UnlockMap(mapId);
        Save();
    }

    public static bool UnlockCharacter(string characterId, bool save = true)
    {
        EnsureLoaded();
        int before = currentProfile.unlockedCharacterIds.Count;
        currentProfile.UnlockCharacter(characterId);
        bool changed = currentProfile.unlockedCharacterIds.Count != before;

        if (changed && save)
        {
            Save();
        }

        return changed;
    }

    public static bool UnlockWeapon(string weaponId, bool save = true)
    {
        EnsureLoaded();
        int before = currentProfile.unlockedWeaponIds.Count;
        currentProfile.UnlockWeapon(weaponId);
        currentProfile.EnsureWeaponProgress(weaponId, 1);
        bool changed = currentProfile.unlockedWeaponIds.Count != before;

        if (changed && save)
        {
            Save();
        }

        return changed;
    }

    public static bool UnlockMap(string mapId, bool save = true)
    {
        EnsureLoaded();
        int before = currentProfile.unlockedMapIds.Count;
        currentProfile.UnlockMap(mapId);
        bool changed = currentProfile.unlockedMapIds.Count != before;

        if (changed && save)
        {
            Save();
        }

        return changed;
    }

    public static void DeleteActiveProfile()
    {
        currentProfile = null;
        wallet = null;

        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }

    private static PlayerProfileData TryLoadFromDisk()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                return null;
            }

            string json = File.ReadAllText(SavePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            PlayerProfileData loaded = JsonUtility.FromJson<PlayerProfileData>(json);
            loaded?.Normalize();
            return loaded;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to load player profile from '{SavePath}'. A new profile will be created. Error: {exception.Message}");
            TryBackupCorruptedSave();
            return null;
        }
    }

    private static PlayerProfileData CreateDefaultProfile()
    {
        PlayerProfileData profile = new()
        {
            profileId = GameProfileStore.ActiveProfileId
        };
        profile.Normalize();
        profile.SetCurrency(CurrencyType.SoulShards, 0);
        return profile;
    }

    private static void TryBackupCorruptedSave()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            string backupPath = SavePath + ".corrupted_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            File.Copy(SavePath, backupPath, overwrite: true);
            File.Delete(SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to backup corrupted profile save: {exception.Message}");
        }
    }

    private static WeaponData GetFirstWeapon(CharacterData character)
    {
        if (character == null || character.AvailableWeapons == null)
        {
            return null;
        }

        for (int i = 0; i < character.AvailableWeapons.Length; i++)
        {
            if (character.AvailableWeapons[i] != null)
            {
                return character.AvailableWeapons[i];
            }
        }

        return null;
    }
}

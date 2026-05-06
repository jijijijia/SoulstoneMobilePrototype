using UnityEngine;

public static class GameProfileStore
{
    private const string ActiveProfileKey = "soulstone.profile.active";
    private const int DefaultProfileIndex = 0;
    private const int MaxProfileCount = 3;

    public static int ActiveProfileIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(ActiveProfileKey, DefaultProfileIndex), 0, MaxProfileCount - 1);
        set
        {
            PlayerPrefs.SetInt(ActiveProfileKey, Mathf.Clamp(value, 0, MaxProfileCount - 1));
            PlayerPrefs.Save();
        }
    }

    public static string ActiveProfileId => $"profile_{ActiveProfileIndex + 1}";

    public static string GetProfileKey(string localKey)
    {
        return $"soulstone.{ActiveProfileId}.{localKey}";
    }

    public static void ClearActiveProfile()
    {
        SaveSystem.DeleteActiveProfile();
        string prefix = $"soulstone.{ActiveProfileId}.";
        PlayerPrefs.DeleteKey(prefix + "selected_character_id");
        PlayerPrefs.DeleteKey(prefix + "selected_map_id");
        PlayerPrefs.DeleteKey(prefix + "settings_language");

        CharacterRoster roster = Resources.Load<CharacterRoster>("CharacterRoster");
        if (roster != null && roster.Characters != null)
        {
            for (int i = 0; i < roster.Characters.Length; i++)
            {
                CharacterData character = roster.Characters[i];

                if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
                {
                    continue;
                }

                PlayerPrefs.DeleteKey(prefix + "selected_weapon_id." + character.CharacterId);
            }
        }

        PlayerPrefs.Save();
    }
}

using UnityEngine;

public static class SelectedCharacterStore
{
    private const string SelectedCharacterIdKey = "soulstone.selected_character_id";
    private const string DefaultRosterResourcePath = "CharacterRoster";

    private static CharacterRoster cachedRoster;

    public static CharacterRoster LoadRoster()
    {
        if (cachedRoster == null)
        {
            cachedRoster = Resources.Load<CharacterRoster>(DefaultRosterResourcePath);
        }

        return cachedRoster;
    }

    public static string GetSelectedCharacterId()
    {
        return PlayerPrefs.GetString(SelectedCharacterIdKey, string.Empty);
    }

    public static void SetSelectedCharacter(CharacterData characterData)
    {
        if (characterData == null || string.IsNullOrWhiteSpace(characterData.CharacterId))
        {
            return;
        }

        PlayerPrefs.SetString(SelectedCharacterIdKey, characterData.CharacterId);
        PlayerPrefs.Save();
    }

    public static CharacterData ResolveSelectedCharacter(CharacterData fallback)
    {
        return ResolveSelectedCharacter(LoadRoster(), fallback);
    }

    public static CharacterData ResolveSelectedCharacter(CharacterRoster roster, CharacterData fallback)
    {
        string selectedCharacterId = GetSelectedCharacterId();
        CharacterData selectedCharacter = roster != null ? roster.FindById(selectedCharacterId) : null;

        if (selectedCharacter != null)
        {
            return selectedCharacter;
        }

        CharacterData firstCharacter = roster != null ? roster.GetFirstCharacter() : null;

        if (firstCharacter != null)
        {
            SetSelectedCharacter(firstCharacter);
            return firstCharacter;
        }

        return fallback;
    }

    public static void ClearCachedRoster()
    {
        cachedRoster = null;
    }
}

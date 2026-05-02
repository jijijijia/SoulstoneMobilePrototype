using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Character Roster", fileName = "CharacterRoster")]
public class CharacterRoster : ScriptableObject
{
    [SerializeField] private CharacterData[] characters;

    public CharacterData[] Characters => characters;

    public CharacterData GetFirstCharacter()
    {
        if (characters == null)
        {
            return null;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                return characters[i];
            }
        }

        return null;
    }

    public CharacterData FindById(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || characters == null)
        {
            return null;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            CharacterData character = characters[i];

            if (character != null && string.Equals(character.CharacterId, characterId, System.StringComparison.OrdinalIgnoreCase))
            {
                return character;
            }
        }

        return null;
    }
}

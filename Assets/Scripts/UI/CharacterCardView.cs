using UnityEngine.Events;

public class CharacterCardView : MenuCardViewBase
{
    public void Configure(CharacterData character, string stats, UnityAction onClick)
    {
        string title = character != null ? character.DisplayName : "Unknown Hero";
        ConfigureBase(title, stats, onClick);
    }
}

using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterPreviewPanelView : UnityEngine.MonoBehaviour
{
    private Text titleText;
    private Text bodyText;
    private Button selectButton;

    public void Bind(Text title, Text body, Button select)
    {
        titleText = title;
        bodyText = body;
        selectButton = select;
    }

    public void Show(CharacterData previewedCharacter, CharacterData selectedCharacter, string body, UnityAction onSelect)
    {
        if (titleText != null)
        {
            titleText.text = previewedCharacter != null ? previewedCharacter.DisplayName : "Персонаж";
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (selectButton != null)
        {
            bool canSelect = previewedCharacter != null && previewedCharacter != selectedCharacter;
            selectButton.interactable = canSelect;
            selectButton.onClick.RemoveAllListeners();

            if (canSelect && onSelect != null)
            {
                selectButton.onClick.AddListener(onSelect);
            }
        }
    }
}

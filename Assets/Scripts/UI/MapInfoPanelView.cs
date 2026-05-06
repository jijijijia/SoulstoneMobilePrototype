using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapInfoPanelView : UnityEngine.MonoBehaviour
{
    private TMP_Text detailsText;
    private Button selectButton;

    public void Bind(TMP_Text details, Button select)
    {
        detailsText = details;
        selectButton = select;
    }

    public void ShowPlaceholder(string message)
    {
        if (detailsText != null)
        {
            detailsText.text = message;
        }

        if (selectButton != null)
        {
            selectButton.interactable = false;
            selectButton.onClick.RemoveAllListeners();
        }
    }

    public void ShowMap(string name, string difficulty, string biome, string enemies, string rewards, UnityAction onSelect)
    {
        if (detailsText != null)
        {
            detailsText.text =
                $"{name}\n" +
                $"Сложность: {difficulty}\n" +
                $"Биом: {biome}\n\n" +
                $"Враги: {enemies}\n" +
                $"Награды: {rewards}";
        }

        if (selectButton != null)
        {
            selectButton.interactable = onSelect != null;
            selectButton.onClick.RemoveAllListeners();

            if (onSelect != null)
            {
                selectButton.onClick.AddListener(onSelect);
            }
        }
    }
}

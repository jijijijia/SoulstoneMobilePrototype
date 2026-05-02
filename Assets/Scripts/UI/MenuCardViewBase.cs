using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MenuCardViewBase : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected Image background;
    [SerializeField] protected Text titleText;
    [SerializeField] protected Text bodyText;
    [SerializeField] protected Color normalColor = new(0.17f, 0.18f, 0.21f, 0.96f);
    [SerializeField] protected Color selectedColor = new(0.86f, 0.68f, 0.32f, 1f);

    public Button Button => button;

    protected virtual void Awake()
    {
        ResolveReferences();
    }

    protected void ConfigureBase(string title, string body, UnityAction onClick)
    {
        ResolveReferences();

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }
    }

    public virtual void SetSelected(bool selected)
    {
        ResolveReferences();

        Color color = selected ? selectedColor : normalColor;

        if (background != null)
        {
            background.color = color;
        }

        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.selectedColor = color;
            colors.highlightedColor = selected ? selectedColor : new Color(0.24f, 0.25f, 0.29f, 1f);
            colors.pressedColor = new Color(color.r * 0.82f, color.g * 0.82f, color.b * 0.82f, 1f);
            button.colors = colors;
        }
    }

    protected void ResolveReferences()
    {
        button = button != null ? button : GetComponent<Button>();
        background = background != null ? background : GetComponent<Image>();
        titleText = titleText != null ? titleText : FindText("Title");
        bodyText = bodyText != null ? bodyText : FindText("Body");

        if (button != null && background != null)
        {
            button.targetGraphic = background;
        }
    }

    private Text FindText(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Text>() : null;
    }
}

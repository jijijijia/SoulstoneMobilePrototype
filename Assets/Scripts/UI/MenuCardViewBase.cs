using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MenuCardViewBase : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected Image background;
    [SerializeField] protected Image iconImage;
    [SerializeField] protected TMP_Text titleText;
    [SerializeField] protected TMP_Text bodyText;
    [SerializeField] protected Text legacyTitleText;
    [SerializeField] protected Text legacyBodyText;
    [SerializeField] protected Color normalColor = new(0.17f, 0.18f, 0.21f, 0.96f);
    [SerializeField] protected Color selectedColor = new(0.86f, 0.68f, 0.32f, 1f);
    [SerializeField] protected Color lockedColor = new(0.05f, 0.05f, 0.06f, 0.82f);

    public Button Button => button;

    protected virtual void Awake()
    {
        ResolveReferences();
    }

    protected void ConfigureBase(string title, string body, UnityAction onClick)
    {
        ConfigureBase(title, body, null, onClick);
    }

    protected void ConfigureBase(string title, string body, Sprite icon, UnityAction onClick)
    {
        ResolveReferences();

        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = new Color(0.96f, 0.9f, 0.72f, 1f);
            titleText.fontStyle = FontStyles.Bold;
        }

        if (legacyTitleText != null)
        {
            legacyTitleText.text = title;
            legacyTitleText.color = new Color(0.96f, 0.9f, 0.72f, 1f);
            legacyTitleText.fontStyle = FontStyle.Bold;
            legacyTitleText.alignment = TextAnchor.UpperCenter;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
            bodyText.color = new Color(0.9f, 0.9f, 0.86f, 0.95f);
        }

        if (legacyBodyText != null)
        {
            legacyBodyText.text = body;
            legacyBodyText.color = new Color(0.9f, 0.9f, 0.86f, 0.95f);
            legacyBodyText.alignment = TextAnchor.MiddleCenter;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
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

    public virtual void SetLocked(bool locked, string lockedLabel = "LOCKED")
    {
        ResolveReferences();

        if (background != null && locked)
        {
            background.color = lockedColor;
        }

        if (button != null)
        {
            button.interactable = true;
        }

        string prefix = locked ? "LOCKED\n" : string.Empty;

        if (bodyText != null && locked && !bodyText.text.StartsWith(prefix))
        {
            bodyText.text = $"{prefix}{bodyText.text}\n\n{lockedLabel}";
            bodyText.color = new Color(0.72f, 0.72f, 0.72f, 0.92f);
        }

        if (legacyBodyText != null && locked && !legacyBodyText.text.StartsWith(prefix))
        {
            legacyBodyText.text = $"{prefix}{legacyBodyText.text}\n\n{lockedLabel}";
            legacyBodyText.color = new Color(0.72f, 0.72f, 0.72f, 0.92f);
        }
    }

    protected void ResolveReferences()
    {
        button = button != null ? button : GetComponent<Button>();
        background = background != null ? background : GetComponent<Image>();
        iconImage = iconImage != null ? iconImage : FindImage("Icon");
        titleText = titleText != null ? titleText : FindText("Title");
        bodyText = bodyText != null ? bodyText : FindText("Body");
        legacyTitleText = legacyTitleText != null ? legacyTitleText : FindLegacyText("Title");
        legacyBodyText = legacyBodyText != null ? legacyBodyText : FindLegacyText("Body");

        if (button != null && background != null)
        {
            button.targetGraphic = background;
        }
    }

    private TMP_Text FindText(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private Text FindLegacyText(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private Image FindImage(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Image>() : null;
    }
}

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameplayHudSceneBaker
{
    private const string GameplayScenePath = "Assets/Scenes/main.unity";
    private const string BakedVersionKey = "Soulstone.GameplayHudSceneBaker.BakedVersion";
    private const int BakedVersion = 1;
    private const int CooldownSlotCount = 6;

    [MenuItem("Soulstone/Bake Gameplay HUD Scene")]
    public static void BakeOpenGameplayScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.path != GameplayScenePath)
        {
            EditorSceneManager.OpenScene(GameplayScenePath);
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        DDHUDManager hudManager = Object.FindFirstObjectByType<DDHUDManager>();

        if (canvas == null || hudManager == null)
        {
            Debug.LogWarning("GameplayHudSceneBaker could not find Canvas or DDHUDManager in main scene.");
            return;
        }

        Transform hudPanel = FindChildRecursive(canvas.transform, "HUDPanel");

        if (hudPanel == null)
        {
            GameObject hudPanelObject = new("HUDPanel", typeof(RectTransform));
            hudPanelObject.transform.SetParent(canvas.transform, false);
            hudPanel = hudPanelObject.transform;
        }

        if (hudPanel is RectTransform hudRect)
        {
            Stretch(hudRect);
        }

        ConfigureExistingHudElements(hudPanel);
        EnsureStatsBgPanel(hudPanel);
        SkillCooldownSlotView[] slots = EnsureCooldownPanel(hudPanel);
        Button pauseButton = EnsurePauseButton(hudPanel);
        TMP_Text killsText = EnsureKillsText(hudPanel);
        AssignHudReferences(hudManager, slots, pauseButton, killsText);

        EditorUtility.SetDirty(hudManager);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorPrefs.SetInt(BakedVersionKey, BakedVersion);
        Debug.Log("Gameplay HUD baked: all HUD visual objects are saved in the scene Hierarchy.");
    }

    private static void ConfigureExistingHudElements(Transform hudPanel)
    {
        ConfigureText(hudPanel, "LevelText", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(12f, 64f), new Vector2(260f, 40f), "Level 1", 28, TextAlignmentOptions.Left, Color.white);
        ConfigureText(hudPanel, "ExperienceText", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(12f, 18f), new Vector2(360f, 32f), "XP: 0/5", 28, TextAlignmentOptions.Left, new Color(0.55f, 0.78f, 1f, 1f));
        ConfigureText(hudPanel, "HealthText", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(420f, 44f), "HP: 100/100", 28, TextAlignmentOptions.Center, Color.white);

        Transform slider = FindChildRecursive(hudPanel, "ExperienceSlider");

        if (slider is RectTransform sliderRect)
        {
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 0f);
            sliderRect.pivot = new Vector2(0.5f, 0f);
            sliderRect.anchoredPosition = new Vector2(0f, 10f);
            sliderRect.sizeDelta = new Vector2(-24f, 28f);
        }
    }

    private static void EnsureStatsBgPanel(Transform hudPanel)
    {
        const string bgName = "StatsBgPanel";
        Transform existing = FindChildRecursive(hudPanel, bgName);

        if (existing == null)
        {
            GameObject bgObject = new(bgName, typeof(RectTransform), typeof(Image));
            bgObject.transform.SetParent(hudPanel, false);
            bgObject.transform.SetAsFirstSibling();
            existing = bgObject.transform;
        }

        if (existing is RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(400f, 120f);
        }

        Image bg = existing.GetComponent<Image>();

        if (bg != null)
        {
            bg.color = new Color(0.11f, 0.12f, 0.145f, 0.72f);
        }
    }

    private static SkillCooldownSlotView[] EnsureCooldownPanel(Transform hudPanel)
    {
        Transform panel = FindChildRecursive(hudPanel, "ActiveSkillCooldownPanel");

        if (panel == null)
        {
            GameObject panelObject = new("ActiveSkillCooldownPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(HorizontalLayoutGroup));
            panelObject.transform.SetParent(hudPanel, false);
            panel = panelObject.transform;
        }

        ConfigureCooldownPanel(panel);
        SkillCooldownSlotView[] slots = panel.GetComponentsInChildren<SkillCooldownSlotView>(true);

        if (slots.Length >= CooldownSlotCount)
        {
            return slots;
        }

        for (int i = slots.Length; i < CooldownSlotCount; i++)
        {
            CreateCooldownSlot(panel, i);
        }

        return panel.GetComponentsInChildren<SkillCooldownSlotView>(true);
    }

    private static void ConfigureCooldownPanel(Transform panel)
    {
        if (panel is RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 52f);
            rect.sizeDelta = new Vector2(700f, 92f);
        }

        HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();

        if (layout != null)
        {
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }
    }

    private static void CreateCooldownSlot(Transform parent, int index)
    {
        GameObject slot = new($"SkillCooldownSlot_{index + 1}", typeof(RectTransform), typeof(Image), typeof(SkillCooldownSlotView));
        slot.transform.SetParent(parent, false);

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(110f, 88f);

        Image background = slot.GetComponent<Image>();
        background.color = new Color(0.04f, 0.045f, 0.055f, 0.82f);

        Image icon = CreateImage("Icon", slot.transform, new Color(1f, 1f, 1f, 0.9f), new Vector2(52f, 52f), new Vector2(0f, 10f));
        Image fill = CreateImage("CooldownFill", slot.transform, new Color(0.95f, 0.72f, 0.22f, 0.72f), new Vector2(52f, 52f), new Vector2(0f, 10f));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Radial360;
        fill.fillOrigin = 2;
        fill.fillAmount = 0f;

        TMP_Text nameText = CreateText("Name", slot.transform, string.Empty, 13, FontStyles.Bold, new Vector2(0f, -28f), new Vector2(106f, 20f));
        TMP_Text rankText = CreateText("Rank", slot.transform, string.Empty, 12, FontStyles.Bold, new Vector2(-38f, 34f), new Vector2(30f, 18f));
        TMP_Text cooldownText = CreateText("Cooldown", slot.transform, string.Empty, 12, FontStyles.Normal, new Vector2(30f, 34f), new Vector2(48f, 18f));

        SerializedObject serializedSlot = new(slot.GetComponent<SkillCooldownSlotView>());
        serializedSlot.FindProperty("iconImage").objectReferenceValue = icon;
        serializedSlot.FindProperty("cooldownFillImage").objectReferenceValue = fill;
        serializedSlot.FindProperty("nameText").objectReferenceValue = nameText;
        serializedSlot.FindProperty("rankText").objectReferenceValue = rankText;
        serializedSlot.FindProperty("cooldownText").objectReferenceValue = cooldownText;
        serializedSlot.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Image CreateImage(string name, Transform parent, Color color, Vector2 size, Vector2 position)
    {
        GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, FontStyles style, Vector2 position, Vector2 size)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private static void ConfigureText(Transform root, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, string text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        Transform textTransform = FindChildRecursive(root, name);

        if (textTransform == null || textTransform is not RectTransform rect)
        {
            return;
        }

        TMP_Text label = textTransform.GetComponent<TMP_Text>();

        if (label == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
    }

    private static Button EnsurePauseButton(Transform hudPanel)
    {
        Transform existing = FindChildRecursive(hudPanel, "PauseButton");
        GameObject buttonObject;

        if (existing != null)
        {
            buttonObject = existing.gameObject;
        }
        else
        {
            buttonObject = new("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(hudPanel, false);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(72f, 72f);

        Image bg = buttonObject.GetComponent<Image>();
        bg.color = new Color(0.11f, 0.12f, 0.145f, 0.88f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = bg;
        ColorBlock cb = button.colors;
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        button.colors = cb;

        TMP_Text label = CreateText("Label", buttonObject.transform, "II", 28, FontStyles.Bold, Vector2.zero, new Vector2(72f, 72f));
        label.color = new Color(0.86f, 0.68f, 0.32f, 1f);

        return button;
    }

    private static TMP_Text EnsureKillsText(Transform hudPanel)
    {
        Transform existing = FindChildRecursive(hudPanel, "KillsText");

        if (existing != null)
        {
            return existing.GetComponent<TMP_Text>();
        }

        TMP_Text label = CreateText("KillsText", hudPanel, "Убийств: 0", 24, FontStyles.Normal, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));

        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-108f, -20f);
        rect.sizeDelta = new Vector2(240f, 40f);
        label.alignment = TextAlignmentOptions.Right;
        label.color = Color.white;

        return label;
    }

    private static void AssignHudReferences(DDHUDManager hudManager, SkillCooldownSlotView[] slots, Button pauseButton, TMP_Text killsText)
    {
        Transform cooldownRoot = slots.Length > 0 ? slots[0].transform.parent : GameObject.Find("ActiveSkillCooldownPanel")?.transform;
        SerializedObject serializedHud = new(hudManager);
        serializedHud.FindProperty("activeSkillCooldownRoot").objectReferenceValue = cooldownRoot;

        SerializedProperty slotsProperty = serializedHud.FindProperty("activeSkillCooldownSlots");
        slotsProperty.arraySize = slots.Length;

        for (int i = 0; i < slots.Length; i++)
        {
            slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        }

        serializedHud.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        serializedHud.FindProperty("killsText").objectReferenceValue = killsText;
        serializedHud.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

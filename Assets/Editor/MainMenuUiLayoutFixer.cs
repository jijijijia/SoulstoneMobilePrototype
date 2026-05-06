using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuUiLayoutFixer
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Soulstone/Fix Main Menu UI Layout")]
    public static void FixMainMenuUiLayout()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("MainMenuUiLayoutFixer: Canvas not found.");
            return;
        }

        ConfigureCanvas(canvas);

        Transform backdrop = canvas.transform.Find("MainMenu_Backdrop");
        if (backdrop == null)
        {
            Debug.LogError("MainMenuUiLayoutFixer: MainMenu_Backdrop not found.");
            return;
        }

        Stretch(GetRect(backdrop));
        FixMenuPanel(backdrop);
        FixPages(backdrop);
        FixReadableText(backdrop);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("MainMenu UI layout fixed. Existing scene-authored pages were preserved and aligned.");
    }

    public static void FixMainMenuUiLayoutBatch()
    {
        FixMainMenuUiLayout();
        EditorApplication.Exit(0);
    }

    private static void ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void FixMenuPanel(Transform backdrop)
    {
        Transform menuPanel = backdrop.Find("MenuPanel");
        if (menuPanel == null)
        {
            return;
        }

        RectTransform rect = GetRect(menuPanel);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(175f, 0f);
        rect.sizeDelta = new Vector2(310f, 820f);

        Transform buttons = menuPanel.Find("ButtonsContainer");
        if (buttons == null)
        {
            return;
        }

        RectTransform buttonsRect = GetRect(buttons);
        buttonsRect.anchorMin = new Vector2(0f, 0f);
        buttonsRect.anchorMax = new Vector2(1f, 1f);
        buttonsRect.offsetMin = new Vector2(40f, 90f);
        buttonsRect.offsetMax = new Vector2(-40f, -190f);

        VerticalLayoutGroup layout = buttons.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        foreach (Button button in buttons.GetComponentsInChildren<Button>(true))
        {
            RectTransform buttonRect = GetRect(button.transform);
            buttonRect.sizeDelta = new Vector2(230f, 58f);

            LayoutElement layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredHeight = 58f;
            layoutElement.minHeight = 58f;
            FixButtonLabel(button.transform, 24f);
        }
    }

    private static void FixPages(Transform backdrop)
    {
        Transform pages = backdrop.Find("Pages");
        if (pages == null)
        {
            return;
        }

        RectTransform pagesRect = GetRect(pages);
        pagesRect.anchorMin = Vector2.zero;
        pagesRect.anchorMax = Vector2.one;
        pagesRect.offsetMin = new Vector2(360f, 90f);
        pagesRect.offsetMax = new Vector2(-55f, -90f);

        FixPage(pages, "HomePanel");
        FixPage(pages, "PlayPanel");
        FixPage(pages, "CharacterPanel");
        FixPage(pages, "WeaponPanel");
        FixPage(pages, "SkillTreePanel");
        FixPage(pages, "RunesPanel");
        FixPage(pages, "AchievementsPanel");
        FixPage(pages, "ProfilePanel");
        FixPage(pages, "SettingsPanel");

        FixPlayPanel(pages.Find("PlayPanel"));
        FixCharacterPanel(pages.Find("CharacterPanel"));
        FixWeaponPanel(pages.Find("WeaponPanel"));
        FixSkillTreePanel(pages.Find("SkillTreePanel"));
        FixSettingsPanel(pages.Find("SettingsPanel"));

        for (int i = 0; i < pages.childCount; i++)
        {
            pages.GetChild(i).gameObject.SetActive(false);
        }
    }

    private static void FixPage(Transform pages, string name)
    {
        Transform page = pages.Find(name);
        if (page == null)
        {
            return;
        }

        Stretch(GetRect(page));
    }

    private static void FixPlayPanel(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        RectTransform scroll = FindRect(panel, "MapScrollView");
        if (scroll != null)
        {
            scroll.anchorMin = new Vector2(0f, 1f);
            scroll.anchorMax = new Vector2(1f, 1f);
            scroll.offsetMin = new Vector2(10f, -245f);
            scroll.offsetMax = new Vector2(-10f, -10f);
        }

        ResizeCards(panel, "MapSlot_", new Vector2(190f, 185f));

        RectTransform details = FindRect(panel, "MapDetails");
        if (details != null)
        {
            details.anchorMin = new Vector2(0f, 0f);
            details.anchorMax = new Vector2(1f, 0f);
            details.pivot = new Vector2(0.5f, 0f);
            details.offsetMin = new Vector2(10f, 95f);
            details.offsetMax = new Vector2(-10f, 315f);
        }

        PlaceBottomButton(panel, "BackButton", new Vector2(150f, 35f), new Vector2(220f, 58f));
        PlaceBottomButton(panel, "BattleButton", new Vector2(430f, 35f), new Vector2(240f, 62f));
        FixButtonLabel(panel, "BackButton", 24f);
        FixButtonLabel(panel, "BattleButton", 24f);
        FixButtonLabel(panel, "MapSelectButton", 22f);
    }

    private static void FixCharacterPanel(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        RectTransform scroll = FindRect(panel, "CharacterScrollView");
        if (scroll != null)
        {
            scroll.anchorMin = Vector2.zero;
            scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(10f, 80f);
            scroll.offsetMax = new Vector2(-450f, -10f);
        }

        Transform grid = panel.Find("CharacterScrollView/Viewport/CharacterGrid");
        if (grid != null)
        {
            GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 3;
                layout.cellSize = new Vector2(190f, 190f);
                layout.spacing = new Vector2(18f, 18f);
                layout.padding = new RectOffset(4, 12, 4, 24);
            }

            ResizeCards(grid, "CharacterSlot_", new Vector2(190f, 190f));
        }

        RectTransform preview = FindRect(panel, "CharacterPreviewPanel");
        if (preview != null)
        {
            preview.anchorMin = new Vector2(1f, 0.5f);
            preview.anchorMax = new Vector2(1f, 0.5f);
            preview.pivot = new Vector2(1f, 0.5f);
            preview.anchoredPosition = new Vector2(-10f, 0f);
            preview.sizeDelta = new Vector2(410f, 500f);
        }

        PlaceBottomButton(panel, "BackButton", new Vector2(150f, 35f), new Vector2(220f, 58f));
        FixButtonLabel(panel, "BackButton", 24f);
        FixButtonLabel(panel, "CharacterSelectButton", 22f);
    }

    private static void FixWeaponPanel(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        RectTransform scroll = FindRect(panel, "WeaponScrollView");
        if (scroll != null)
        {
            scroll.anchorMin = Vector2.zero;
            scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(10f, 120f);
            scroll.offsetMax = new Vector2(-450f, -40f);
        }

        ResizeCards(panel, "WeaponSlot_", new Vector2(210f, 230f));

        RectTransform info = FindRect(panel, "WeaponInfoPanel");
        if (info != null)
        {
            info.anchorMin = new Vector2(1f, 0.5f);
            info.anchorMax = new Vector2(1f, 0.5f);
            info.pivot = new Vector2(1f, 0.5f);
            info.anchoredPosition = new Vector2(-10f, 0f);
            info.sizeDelta = new Vector2(410f, 330f);
        }

        PlaceBottomButton(panel, "BackButton", new Vector2(150f, 35f), new Vector2(220f, 58f));
        FixButtonLabel(panel, "BackButton", 24f);
    }

    private static void FixSkillTreePanel(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        RectTransform text = FindRect(panel, "SkillTreeText");
        if (text != null)
        {
            text.offsetMin = new Vector2(40f, 120f);
            text.offsetMax = new Vector2(-40f, -60f);
        }

        PlaceBottomButton(panel, "BackButton", new Vector2(150f, 35f), new Vector2(220f, 58f));
        FixButtonLabel(panel, "BackButton", 24f);
    }

    private static void FixSettingsPanel(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        PlaceBottomButton(panel, "BackButton", new Vector2(150f, 35f), new Vector2(220f, 58f));
        FixButtonLabel(panel, "BackButton", 24f);
        FixButtonLabel(panel, "QualityButton", 22f);
        FixButtonLabel(panel, "LanguageButton", 22f);
        FixButtonLabel(panel, "FullscreenButton", 22f);
    }

    private static void ResizeCards(Transform root, string prefix, Vector2 size)
    {
        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (!rect.name.StartsWith(prefix))
            {
                continue;
            }

            rect.sizeDelta = size;

            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }

            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            layout.minWidth = size.x;
            layout.minHeight = size.y;
            FixCardText(rect);
        }
    }

    private static void FixReadableText(Transform root)
    {
        foreach (Text text in root.GetComponentsInChildren<Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            string lowerName = text.name.ToLowerInvariant();
            bool isButtonLabel = lowerName.Contains("label") || IsInsideButton(text.transform);
            bool isTitle = lowerName.Contains("title") || lowerName.Contains("slot");

            if (isButtonLabel)
            {
                text.color = new Color(1f, 0.9f, 0.58f, 1f);
                text.fontSize = Mathf.Max(text.fontSize, 22);
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.transform.SetAsLastSibling();
                Stretch(GetRect(text.transform));
            }
            else if (isTitle)
            {
                text.color = new Color(0.98f, 0.92f, 0.72f, 1f);
                text.fontSize = Mathf.Max(text.fontSize, 18);
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.UpperCenter;
            }
            else
            {
                text.color = new Color(0.92f, 0.92f, 0.88f, 0.96f);
            }
        }

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.textWrappingMode = TextWrappingModes.Normal;

            string lowerName = text.name.ToLowerInvariant();
            bool isButtonLabel = lowerName.Contains("label") || IsInsideButton(text.transform);
            bool isTitle = lowerName.Contains("title") || lowerName.Contains("slot");

            if (isButtonLabel)
            {
                text.color = new Color(1f, 0.9f, 0.58f, 1f);
                text.fontSize = Mathf.Max(text.fontSize, 22f);
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.transform.SetAsLastSibling();
                Stretch(GetRect(text.transform));
            }
            else if (isTitle)
            {
                text.color = new Color(0.98f, 0.92f, 0.72f, 1f);
                text.fontSize = Mathf.Max(text.fontSize, 18f);
                text.fontStyle = FontStyles.Bold;
            }
            else
            {
                text.color = new Color(0.92f, 0.92f, 0.88f, 0.96f);
            }
        }
    }

    private static void FixCardText(RectTransform card)
    {
        Text legacyTitle = FindLegacyText(card, "Title");
        if (legacyTitle != null)
        {
            legacyTitle.color = new Color(0.98f, 0.92f, 0.72f, 1f);
            legacyTitle.fontSize = Mathf.Clamp(legacyTitle.fontSize, 18, 22);
            legacyTitle.fontStyle = FontStyle.Bold;
            legacyTitle.alignment = TextAnchor.UpperCenter;
            legacyTitle.transform.SetAsLastSibling();
        }

        Text legacyBody = FindLegacyText(card, "Body");
        if (legacyBody != null)
        {
            legacyBody.color = new Color(0.9f, 0.9f, 0.86f, 0.95f);
            legacyBody.fontSize = Mathf.Clamp(legacyBody.fontSize, 14, 18);
            legacyBody.alignment = TextAnchor.MiddleCenter;
            legacyBody.transform.SetAsLastSibling();
        }

        TMP_Text title = FindText(card, "Title");
        if (title != null)
        {
            title.color = new Color(0.98f, 0.92f, 0.72f, 1f);
            title.fontSize = Mathf.Clamp(title.fontSize, 18f, 22f);
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Top;
            title.transform.SetAsLastSibling();
        }

        TMP_Text body = FindText(card, "Body");
        if (body != null)
        {
            body.color = new Color(0.9f, 0.9f, 0.86f, 0.95f);
            body.fontSize = Mathf.Clamp(body.fontSize, 14f, 18f);
            body.alignment = TextAlignmentOptions.Center;
            body.transform.SetAsLastSibling();
        }
    }

    private static void FixButtonLabel(Transform root, string buttonName, float fontSize)
    {
        RectTransform button = FindRect(root, buttonName);
        if (button != null)
        {
            FixButtonLabel(button, fontSize);
        }
    }

    private static void FixButtonLabel(Transform button, float fontSize)
    {
        TMP_Text label = FindText(button, "Label");
        if (label == null)
        {
            label = button.GetComponentInChildren<TMP_Text>(true);
        }

        Text legacyLabel = FindLegacyText(button, "Label");
        if (legacyLabel == null)
        {
            legacyLabel = button.GetComponentInChildren<Text>(true);
        }

        if (label == null && legacyLabel == null)
        {
            return;
        }

        if (label != null)
        {
            label.color = new Color(1f, 0.9f, 0.58f, 1f);
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.transform.SetAsLastSibling();
            Stretch(GetRect(label.transform));
        }

        if (legacyLabel != null)
        {
            legacyLabel.color = new Color(1f, 0.9f, 0.58f, 1f);
            legacyLabel.fontSize = Mathf.RoundToInt(fontSize);
            legacyLabel.fontStyle = FontStyle.Bold;
            legacyLabel.alignment = TextAnchor.MiddleCenter;
            legacyLabel.raycastTarget = false;
            legacyLabel.transform.SetAsLastSibling();
            Stretch(GetRect(legacyLabel.transform));
        }
    }

    private static void PlaceBottomButton(Transform root, string name, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = FindRect(root, name);
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == name)
            {
                return rect;
            }
        }

        return null;
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == name)
            {
                return text;
            }
        }

        return null;
    }

    private static Text FindLegacyText(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Text text in root.GetComponentsInChildren<Text>(true))
        {
            if (text.name == name)
            {
                return text;
            }
        }

        return null;
    }

    private static RectTransform GetRect(Transform transform)
    {
        return transform.GetComponent<RectTransform>() ?? transform.gameObject.AddComponent<RectTransform>();
    }

    private static bool IsInsideButton(Transform transform)
    {
        Transform current = transform != null ? transform.parent : null;

        while (current != null)
        {
            if (current.GetComponent<Button>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void Stretch(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

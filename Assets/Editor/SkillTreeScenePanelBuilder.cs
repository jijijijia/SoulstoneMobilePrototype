using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SkillTreeScenePanelBuilder
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GeneratedRootName = "SkillTreeContentRoot";

    [MenuItem("Soulstone/UI/Build Scene Skill Tree Panel")]
    public static void BuildSceneSkillTreePanel()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        SkillTreeContentCreator.CreateDefaultSkillTree();
        SkillTreeData tree = AssetDatabase.LoadAssetAtPath<SkillTreeData>(SkillTreeContentCreator.DefaultTreePath);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Transform panel = canvas != null
            ? canvas.transform.Find("MainMenu_Backdrop/Pages/SkillTreePanel")
            : null;

        if (panel == null)
        {
            Debug.LogError("SkillTreeScenePanelBuilder: SkillTreePanel not found at Canvas/MainMenu_Backdrop/Pages/SkillTreePanel.");
            return;
        }

        Transform oldRoot = panel.Find(GeneratedRootName);

        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot.gameObject);
        }

        HideLegacyPlaceholder(panel);

        SkillTreePanelView view = panel.GetComponent<SkillTreePanelView>() ?? panel.gameObject.AddComponent<SkillTreePanelView>();
        RectTransform root = CreateRect(GeneratedRootName, panel);
        Stretch(root);

        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.035f, 0.055f, 0.09f, 0.78f);

        TMP_Text title = CreateText("TitleText", root, "Дерево навыков", 44f, FontStyles.Bold, TextAlignmentOptions.Center);
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(620f, 72f));

        RectTransform nodeArea = CreateRect("NodeArea", root);
        nodeArea.anchorMin = new Vector2(0f, 0f);
        nodeArea.anchorMax = new Vector2(0.7f, 1f);
        nodeArea.offsetMin = new Vector2(30f, 70f);
        nodeArea.offsetMax = new Vector2(-20f, -110f);

        RectTransform tooltipPanel = CreatePanel("TooltipPanel", root, new Color(0.055f, 0.06f, 0.09f, 0.9f));
        tooltipPanel.anchorMin = new Vector2(0.72f, 0.18f);
        tooltipPanel.anchorMax = new Vector2(1f, 0.88f);
        tooltipPanel.offsetMin = new Vector2(0f, 0f);
        tooltipPanel.offsetMax = new Vector2(-30f, 0f);

        TMP_Text tooltipText = CreateText("TooltipText", tooltipPanel, "Выберите узел дерева навыков.", 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        tooltipText.textWrappingMode = TextWrappingModes.Normal;
        tooltipText.overflowMode = TextOverflowModes.Ellipsis;
        tooltipText.rectTransform.anchorMin = Vector2.zero;
        tooltipText.rectTransform.anchorMax = Vector2.one;
        tooltipText.rectTransform.offsetMin = new Vector2(28f, 88f);
        tooltipText.rectTransform.offsetMax = new Vector2(-28f, -28f);

        Button improveButton = CreateButton("ImproveButton", tooltipPanel, "УЛУЧШИТЬ");
        Anchor((RectTransform)improveButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(240f, 58f));

        SkillTreeNodeData[] nodes = tree != null ? tree.Nodes : null;
        int count = Mathf.Max(nodes != null ? nodes.Length : 0, 7);
        SkillTreeNodeView[] nodeViews = new SkillTreeNodeView[count];
        Image[] lines = new Image[Mathf.Max(0, count - 1)];
        Vector2[] positions =
        {
            new(0f, 150f),
            new(-190f, 20f),
            new(190f, 20f),
            new(-280f, -145f),
            new(-95f, -145f),
            new(95f, -145f),
            new(280f, -145f)
        };

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateConnectionLine(nodeArea, positions[0], positions[Mathf.Min(i + 1, positions.Length - 1)], i);
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 position = positions[Mathf.Min(i, positions.Length - 1)];
            nodeViews[i] = CreateNodeView(nodeArea, i, position);
        }

        SerializedObject serialized = new(view);
        serialized.FindProperty("skillTreeData").objectReferenceValue = tree;
        SetObjectArray(serialized.FindProperty("nodeViews"), nodeViews);
        SetObjectArray(serialized.FindProperty("connectionLines"), lines);
        serialized.FindProperty("tooltipText").objectReferenceValue = tooltipText;
        serialized.FindProperty("improveButton").objectReferenceValue = improveButton;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Scene-authored SkillTreePanel built and linked to SkillTree_Default.");
    }

    private static void HideLegacyPlaceholder(Transform panel)
    {
        foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text != null && text.transform.parent == panel)
            {
                text.gameObject.SetActive(false);
            }
        }

        foreach (Text text in panel.GetComponentsInChildren<Text>(true))
        {
            if (text != null && text.transform.parent == panel)
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    private static SkillTreeNodeView CreateNodeView(Transform parent, int index, Vector2 position)
    {
        RectTransform rect = CreatePanel($"SkillTreeNode_{index + 1:00}", parent, new Color(0.08f, 0.09f, 0.12f, 0.92f));
        Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(150f, 112f));

        Button button = rect.gameObject.AddComponent<Button>();
        Image background = rect.GetComponent<Image>();
        button.targetGraphic = background;

        Image icon = CreatePanel("Icon", rect, new Color(0.86f, 0.68f, 0.32f, 1f)).GetComponent<Image>();
        Anchor((RectTransform)icon.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(42f, 42f));

        TMP_Text title = CreateText("Title", rect, $"Узел {index + 1}", 15f, FontStyles.Bold, TextAlignmentOptions.Center);
        Anchor(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -2f), new Vector2(-18f, 28f));

        TMP_Text level = CreateText("Level", rect, "0/1", 16f, FontStyles.Bold, TextAlignmentOptions.Center);
        Anchor(level.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(90f, 28f));

        GameObject locked = CreateStateLabel("LockedState", rect, "LOCK", new Color(0.05f, 0.05f, 0.06f, 0.78f));
        GameObject available = CreateStateLabel("AvailableState", rect, "NEW", new Color(0.85f, 0.58f, 0.08f, 0.42f));
        GameObject owned = CreateStateLabel("OwnedState", rect, "OK", new Color(0.1f, 0.5f, 0.18f, 0.45f));

        SkillTreeNodeView view = rect.gameObject.AddComponent<SkillTreeNodeView>();
        SerializedObject serialized = new(view);
        serialized.FindProperty("background").objectReferenceValue = background;
        serialized.FindProperty("iconImage").objectReferenceValue = icon;
        serialized.FindProperty("titleText").objectReferenceValue = title;
        serialized.FindProperty("levelText").objectReferenceValue = level;
        serialized.FindProperty("lockedState").objectReferenceValue = locked;
        serialized.FindProperty("availableState").objectReferenceValue = available;
        serialized.FindProperty("ownedState").objectReferenceValue = owned;
        serialized.FindProperty("button").objectReferenceValue = button;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    private static GameObject CreateStateLabel(string name, Transform parent, string text, Color color)
    {
        RectTransform rect = CreatePanel(name, parent, color);
        Stretch(rect);

        TMP_Text label = CreateText("Label", rect, text, 18f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        rect.gameObject.SetActive(false);
        return rect.gameObject;
    }

    private static Image CreateConnectionLine(Transform parent, Vector2 from, Vector2 to, int index)
    {
        RectTransform rect = CreateRect($"ConnectionLine_{index + 1:00}", parent);
        Image line = rect.gameObject.AddComponent<Image>();
        line.color = new Color(0.68f, 0.55f, 0.28f, 0.65f);

        Vector2 midpoint = (from + to) * 0.5f;
        Vector2 direction = to - from;
        Anchor(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), midpoint, new Vector2(direction.magnitude, 5f));
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        rect.SetAsFirstSibling();
        return line;
    }

    private static Button CreateButton(string name, Transform parent, string labelText)
    {
        RectTransform rect = CreatePanel(name, parent, new Color(0.07f, 0.11f, 0.18f, 0.98f));
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();

        TMP_Text label = CreateText("Label", rect, labelText, 22f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        return button;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);
        TMP_Text label = gameObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = new Color(1f, 0.92f, 0.72f, 1f);
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return rect;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<RectTransform>();
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetObjectArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        values ??= new T[0];
        property.arraySize = values.Length;

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}

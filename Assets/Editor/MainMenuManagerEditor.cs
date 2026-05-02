using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(MainMenuManager))]
public class MainMenuManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Scene Layout Tools", EditorStyles.boldLabel);

        MainMenuManager menuManager = (MainMenuManager)target;

        if (GUILayout.Button("Rebuild Scene Menu Layout"))
        {
            menuManager.RebuildSceneMenuLayout();
        }

        if (GUILayout.Button("Clear Scene Menu Layout"))
        {
            menuManager.ClearSceneMenuLayout();
        }

        if (GUILayout.Button("Create And Assign Default Card Prefabs"))
        {
            CreateAndAssignDefaultCardPrefabs(menuManager);
        }

        EditorGUILayout.HelpBox(
            "Use Rebuild to create the editable menu frame inside the scene Canvas. " +
            "Runtime data lists are still filled from assets automatically.",
            MessageType.Info);
    }

    private void CreateAndAssignDefaultCardPrefabs(MainMenuManager menuManager)
    {
        const string folder = "Assets/Prefabs/UI";

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        MapCardView mapCard = CreateCardPrefab<MapCardView>($"{folder}/MapCardView.prefab", new Vector2(220f, 210f));
        CharacterCardView characterCard = CreateCardPrefab<CharacterCardView>($"{folder}/CharacterCardView.prefab", new Vector2(260f, 290f));
        WeaponCardView weaponCard = CreateCardPrefab<WeaponCardView>($"{folder}/WeaponCardView.prefab", new Vector2(280f, 290f));

        SerializedObject serializedMenu = new(menuManager);
        serializedMenu.FindProperty("mapCardPrefab").objectReferenceValue = mapCard;
        serializedMenu.FindProperty("characterCardPrefab").objectReferenceValue = characterCard;
        serializedMenu.FindProperty("weaponCardPrefab").objectReferenceValue = weaponCard;
        serializedMenu.ApplyModifiedProperties();

        EditorUtility.SetDirty(menuManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(menuManager.gameObject.scene);
        AssetDatabase.SaveAssets();
    }

    private T CreateCardPrefab<T>(string path, Vector2 size) where T : MenuCardViewBase
    {
        GameObject root = new(typeof(T).Name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(T));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.17f, 0.18f, 0.21f, 0.96f);

        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;

        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;

        CreateTextChild("Title", root.transform, 24, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(18f, -70f), new Vector2(-18f, -16f), new Vector2(0f, 1f), new Vector2(1f, 1f));
        CreateTextChild("Body", root.transform, 17, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(18f, 18f), new Vector2(-18f, -82f), Vector2.zero, Vector2.one);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab.GetComponent<T>();
    }

    private void CreateTextChild(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }
}

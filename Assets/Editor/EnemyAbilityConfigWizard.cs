using System.IO;
using UnityEditor;
using UnityEngine;

public class EnemyAbilityConfigWizard : EditorWindow
{
    private const string DefaultOutputFolder = "Assets/Data/EnemyAbilities";

    private string abilityId = "enemy_melee_01";
    private string displayName = "Melee Strike";
    private EnemyAbilityDeliveryType deliveryType = EnemyAbilityDeliveryType.MeleeContact;

    private float preferredDistance = 1.4f;
    private float cooldown = 1f;
    private float damageMultiplier = 1f;
    private float rangeTolerance = 0.35f;

    private GameObject projectilePrefab;
    private float projectileSpeed = 10f;
    private float projectileLifetime = 4f;
    private float projectileScale = 0.4f;
    private Color projectileColor = Color.green;

    private float areaRadius = 2.4f;
    private GameObject areaVisualPrefab;
    private float areaVisualLifetime = 0.25f;

    private string outputFolder = DefaultOutputFolder;

    [MenuItem("Soulstone/Create Enemy Ability Config Wizard")]
    public static void ShowWindow()
    {
        EnemyAbilityConfigWizard window = GetWindow<EnemyAbilityConfigWizard>("Enemy Ability");
        window.minSize = new Vector2(400f, 560f);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Enemy Ability Config Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates an EnemyAbilityConfig ScriptableObject asset.", MessageType.Info);

        EditorGUILayout.Space(8f);
        DrawIdentitySection();
        EditorGUILayout.Space(8f);
        DrawCombatSection();
        EditorGUILayout.Space(8f);
        DrawDeliverySection();
        EditorGUILayout.Space(8f);
        DrawOutputSection();
        EditorGUILayout.Space(12f);

        GUI.enabled = !EditorApplication.isCompiling && !EditorApplication.isUpdating;

        if (GUILayout.Button("Create Enemy Ability", GUILayout.Height(36f)))
        {
            CreateAbilityConfig();
        }

        GUI.enabled = true;
    }

    private void DrawIdentitySection()
    {
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
        abilityId = EditorGUILayout.TextField("Ability ID", abilityId);
        displayName = EditorGUILayout.TextField("Display Name", displayName);
        deliveryType = (EnemyAbilityDeliveryType)EditorGUILayout.EnumPopup("Delivery Type", deliveryType);
    }

    private void DrawCombatSection()
    {
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        preferredDistance = EditorGUILayout.FloatField("Preferred Distance", preferredDistance);
        rangeTolerance = EditorGUILayout.FloatField("Range Tolerance", rangeTolerance);
        cooldown = EditorGUILayout.FloatField("Cooldown (s)", cooldown);
        damageMultiplier = EditorGUILayout.FloatField("Damage Multiplier", damageMultiplier);
    }

    private void DrawDeliverySection()
    {
        switch (deliveryType)
        {
            case EnemyAbilityDeliveryType.Projectile:
                DrawProjectileSection();
                break;

            case EnemyAbilityDeliveryType.AreaPulse:
                DrawAreaSection();
                break;
        }
    }

    private void DrawProjectileSection()
    {
        EditorGUILayout.LabelField("Projectile", EditorStyles.boldLabel);
        projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", projectilePrefab, typeof(GameObject), false);
        projectileSpeed = EditorGUILayout.FloatField("Speed", projectileSpeed);
        projectileLifetime = EditorGUILayout.FloatField("Lifetime", projectileLifetime);
        projectileScale = EditorGUILayout.FloatField("Scale", projectileScale);
        projectileColor = EditorGUILayout.ColorField("Color", projectileColor);
    }

    private void DrawAreaSection()
    {
        EditorGUILayout.LabelField("Area Pulse", EditorStyles.boldLabel);
        areaRadius = EditorGUILayout.FloatField("Radius", areaRadius);
        areaVisualPrefab = (GameObject)EditorGUILayout.ObjectField("Visual Prefab", areaVisualPrefab, typeof(GameObject), false);
        areaVisualLifetime = EditorGUILayout.FloatField("Visual Lifetime", areaVisualLifetime);
    }

    private void DrawOutputSection()
    {
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        outputFolder = EditorGUILayout.TextField("Folder", outputFolder);
    }

    private void CreateAbilityConfig()
    {
        if (string.IsNullOrWhiteSpace(abilityId))
        {
            EditorUtility.DisplayDialog("Invalid ID", "Ability ID must not be empty.", "OK");
            return;
        }

        EnsureFolder(outputFolder);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{abilityId}.asset");
        EnemyAbilityConfig config = ScriptableObject.CreateInstance<EnemyAbilityConfig>();
        AssetDatabase.CreateAsset(config, assetPath);

        SerializedObject serializedObject = new(config);
        serializedObject.FindProperty("abilityId").stringValue = abilityId;
        serializedObject.FindProperty("displayName").stringValue = displayName;
        serializedObject.FindProperty("deliveryType").enumValueIndex = (int)deliveryType;
        serializedObject.FindProperty("preferredDistance").floatValue = preferredDistance;
        serializedObject.FindProperty("cooldown").floatValue = Mathf.Max(0.1f, cooldown);
        serializedObject.FindProperty("damageMultiplier").floatValue = Mathf.Max(0f, damageMultiplier);
        serializedObject.FindProperty("rangeTolerance").floatValue = Mathf.Max(0f, rangeTolerance);
        serializedObject.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
        serializedObject.FindProperty("projectileSpeed").floatValue = projectileSpeed;
        serializedObject.FindProperty("projectileLifetime").floatValue = projectileLifetime;
        serializedObject.FindProperty("projectileScale").floatValue = projectileScale;
        serializedObject.FindProperty("projectileColor").colorValue = projectileColor;
        serializedObject.FindProperty("areaRadius").floatValue = areaRadius;
        serializedObject.FindProperty("areaVisualPrefab").objectReferenceValue = areaVisualPrefab;
        serializedObject.FindProperty("areaVisualLifetime").floatValue = areaVisualLifetime;
        serializedObject.FindProperty("statuses").arraySize = 0;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);

        EditorUtility.DisplayDialog("Created", $"Enemy ability '{displayName}' created at {assetPath}", "OK");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}

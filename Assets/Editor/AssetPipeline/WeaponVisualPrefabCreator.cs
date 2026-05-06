using UnityEditor;
using UnityEngine;

public static class WeaponVisualPrefabCreator
{
    private const string OutputFolder = "Assets/_Project/Prefabs/Weapons";

    private static readonly WeaponVisualSource[] Sources =
    {
        new("WeaponVisual_Axe_1H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/axe_1handed.fbx"),
        new("WeaponVisual_Axe_2H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/axe_2handed.fbx"),
        new("WeaponVisual_Bow", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/bow.fbx"),
        new("WeaponVisual_Bow_WithString", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/bow_withString.fbx"),
        new("WeaponVisual_Crossbow_1H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/crossbow_1handed.fbx"),
        new("WeaponVisual_Crossbow_2H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/crossbow_2handed.fbx"),
        new("WeaponVisual_Dagger", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/dagger.fbx"),
        new("WeaponVisual_Shield_Badge", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/shield_badge.fbx"),
        new("WeaponVisual_Shield_Round", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/shield_round.fbx"),
        new("WeaponVisual_Shield_Square", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/shield_square.fbx"),
        new("WeaponVisual_Staff", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/staff.fbx"),
        new("WeaponVisual_Sword_1H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/sword_1handed.fbx"),
        new("WeaponVisual_Sword_2H", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/sword_2handed.fbx"),
        new("WeaponVisual_Wand", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/wand.fbx"),
        new("WeaponVisual_Arrow_Bow", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/arrow_bow.fbx"),
        new("WeaponVisual_Arrow_Crossbow", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Assets/fbx(unity)/arrow_crossbow.fbx"),
        new("WeaponVisual_SkeletonBow", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/assets/fbx/skeleton_bow.fbx"),
        new("WeaponVisual_SkeletonStaff", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/assets/fbx/skeleton_staff.fbx"),
        new("WeaponVisual_SkeletonShield", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/assets/fbx/shield.fbx"),
        new("WeaponVisual_SkeletonArrow_Green", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/assets/fbx/arrow_green.fbx"),
        new("WeaponVisual_SkeletonArrow_Purple", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/assets/fbx/arrow_purple.fbx"),
    };

    [MenuItem("Soulstone/Assets/Create Weapon Visual Prefabs")]
    public static void CreateWeaponVisualPrefabs()
    {
        EnsureFolder(OutputFolder);

        foreach (WeaponVisualSource source in Sources)
        {
            CreatePrefab(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {Sources.Length} weapon visual prefabs in {OutputFolder}.");
    }

    public static void CreateWeaponVisualPrefabsBatch()
    {
        CreateWeaponVisualPrefabs();
        EditorApplication.Exit(0);
    }

    private static void CreatePrefab(WeaponVisualSource source)
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(source.ModelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning($"Weapon visual source not found: {source.ModelPath}");
            return;
        }

        string prefabPath = $"{OutputFolder}/{source.PrefabName}.prefab";
        GameObject root = new GameObject(source.PrefabName);

        GameObject modelRoot = new GameObject("ModelRoot");
        modelRoot.transform.SetParent(root.transform, false);

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        modelInstance.name = "VisualModel";
        modelInstance.transform.SetParent(modelRoot.transform, false);

        CreateSocket(root.transform, "GripSocket", Vector3.zero);
        CreateSocket(root.transform, "TipSocket", new Vector3(0f, 0.75f, 0f));
        CreateSocket(root.transform, "TrailSocket", new Vector3(0f, 0.4f, 0f));
        CreateSocket(root.transform, "ProjectileSpawnSocket", new Vector3(0f, 0.15f, 0.45f));

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateSocket(Transform parent, string socketName, Vector3 localPosition)
    {
        GameObject socket = new GameObject(socketName);
        socket.transform.SetParent(parent, false);
        socket.transform.localPosition = localPosition;
    }

    private static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private readonly struct WeaponVisualSource
    {
        public readonly string PrefabName;
        public readonly string ModelPath;

        public WeaponVisualSource(string prefabName, string modelPath)
        {
            PrefabName = prefabName;
            ModelPath = modelPath;
        }
    }
}

using UnityEditor;
using UnityEngine;

public static class EnemyVisualPrefabCreator
{
    private const string OutputFolder = "Assets/_Project/Prefabs/Enemies";

    private static readonly EnemyVisualSource[] Sources =
    {
        new("EnemyVisual_SkeletonArcher", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_archer.fbx"),
        new("EnemyVisual_SkeletonMage", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_mage.fbx"),
        new("EnemyVisual_SkeletonMinion", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_minion.fbx"),
        new("EnemyVisual_SkeletonWarrior", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_warrior.fbx"),
        new("EnemyVisual_SkeletonArcher_Broken", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_archer_broken.fbx"),
        new("EnemyVisual_SkeletonMage_Broken", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_mage_broken.fbx"),
        new("EnemyVisual_SkeletonMinion_Broken", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_minion_broken.fbx"),
        new("EnemyVisual_SkeletonWarrior_Broken", "Assets/Art/Imported/KayKit Character Pack - Skeletons 1.0/Models/characters/fbx/character_skeleton_warrior_broken.fbx"),
    };

    [MenuItem("Soulstone/Assets/Create Enemy Visual Prefabs")]
    public static void CreateEnemyVisualPrefabs()
    {
        EnsureFolder(OutputFolder);

        foreach (EnemyVisualSource source in Sources)
        {
            CreatePrefab(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {Sources.Length} enemy visual prefabs in {OutputFolder}.");
    }

    public static void CreateEnemyVisualPrefabsBatch()
    {
        CreateEnemyVisualPrefabs();
        EditorApplication.Exit(0);
    }

    private static void CreatePrefab(EnemyVisualSource source)
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(source.ModelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning($"Enemy visual source not found: {source.ModelPath}");
            return;
        }

        string prefabPath = $"{OutputFolder}/{source.PrefabName}.prefab";
        GameObject root = new GameObject(source.PrefabName);

        GameObject modelRoot = new GameObject("ModelRoot");
        modelRoot.transform.SetParent(root.transform, false);

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        modelInstance.name = "VisualModel";
        modelInstance.transform.SetParent(modelRoot.transform, false);

        CreateSocket(root.transform, "AttackSocket", new Vector3(0f, 1.1f, 0.35f));
        CreateSocket(root.transform, "ProjectileSocket", new Vector3(0f, 1.25f, 0.45f));
        CreateSocket(root.transform, "HitVfxSocket", new Vector3(0f, 1f, 0f));
        CreateSocket(root.transform, "GroundSocket", Vector3.zero);

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

    private readonly struct EnemyVisualSource
    {
        public readonly string PrefabName;
        public readonly string ModelPath;

        public EnemyVisualSource(string prefabName, string modelPath)
        {
            PrefabName = prefabName;
            ModelPath = modelPath;
        }
    }
}

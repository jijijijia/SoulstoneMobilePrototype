using System.IO;
using UnityEditor;
using UnityEngine;

public static class CharacterVisualPrefabCreator
{
    private const string OutputFolder = "Assets/_Project/Prefabs/Characters";

    private static readonly CharacterVisualSource[] Sources =
    {
        new("HeroVisual_Barbarian", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Barbarian.fbx"),
        new("HeroVisual_Knight", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Knight.fbx"),
        new("HeroVisual_Mage", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Mage.fbx"),
        new("HeroVisual_Ranger", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Ranger.fbx"),
        new("HeroVisual_Rogue", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Rogue.fbx"),
        new("HeroVisual_RogueHooded", "Assets/Art/Imported/KayKit_Adventurers_2.0_FREE/KayKit_Adventurers_2.0_FREE/Characters/fbx/Rogue_Hooded.fbx"),
    };

    [MenuItem("Soulstone/Assets/Create Character Visual Prefabs")]
    public static void CreateCharacterVisualPrefabs()
    {
        EnsureFolder(OutputFolder);

        foreach (CharacterVisualSource source in Sources)
        {
            CreatePrefab(source);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {Sources.Length} character visual prefabs in {OutputFolder}.");
    }

    public static void CreateCharacterVisualPrefabsBatch()
    {
        CreateCharacterVisualPrefabs();
        EditorApplication.Exit(0);
    }

    private static void CreatePrefab(CharacterVisualSource source)
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(source.ModelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning($"Character visual source not found: {source.ModelPath}");
            return;
        }

        string prefabPath = $"{OutputFolder}/{source.PrefabName}.prefab";
        GameObject root = new GameObject(source.PrefabName);

        GameObject modelRoot = new GameObject("ModelRoot");
        modelRoot.transform.SetParent(root.transform, false);

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        modelInstance.name = "VisualModel";
        modelInstance.transform.SetParent(modelRoot.transform, false);

        CreateSocket(root.transform, "WeaponSocket_RightHand", new Vector3(0.35f, 1.05f, 0.08f));
        CreateSocket(root.transform, "WeaponSocket_LeftHand", new Vector3(-0.35f, 1.05f, 0.08f));
        CreateSocket(root.transform, "WeaponSocket_Back", new Vector3(0f, 1.35f, -0.22f));
        CreateSocket(root.transform, "VfxSocket_Chest", new Vector3(0f, 1.15f, 0f));
        CreateSocket(root.transform, "VfxSocket_Ground", Vector3.zero);

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

    private readonly struct CharacterVisualSource
    {
        public readonly string PrefabName;
        public readonly string ModelPath;

        public CharacterVisualSource(string prefabName, string modelPath)
        {
            PrefabName = prefabName;
            ModelPath = modelPath;
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

public static class ImportedVisualDataValidator
{
    [MenuItem("Soulstone/Assets/Validate Imported Visual Data Links")]
    public static void ValidateImportedVisualDataLinks()
    {
        ValidateCharacters();
        ValidateWeapons();
        ValidateEnemies();
        Debug.Log("Imported visual data validation completed.");
    }

    public static void ValidateImportedVisualDataLinksBatch()
    {
        ValidateImportedVisualDataLinks();
        EditorApplication.Exit(0);
    }

    private static void ValidateCharacters()
    {
        foreach (string path in Directory.GetFiles("Assets/Data/Characters", "*.asset", SearchOption.AllDirectories))
        {
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (data == null)
            {
                continue;
            }

            if (data.ModelPrefab == null)
            {
                Debug.LogWarning($"Character has no visual model prefab: {path}", data);
            }
        }
    }

    private static void ValidateWeapons()
    {
        foreach (string path in Directory.GetFiles("Assets/Data/Weapons", "*.asset", SearchOption.AllDirectories))
        {
            WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (data == null)
            {
                continue;
            }

            if (data.VisualPrefab == null)
            {
                Debug.LogWarning($"Weapon has no visual prefab: {path}", data);
            }
        }
    }

    private static void ValidateEnemies()
    {
        foreach (string path in Directory.GetFiles("Assets/Data/Enemies", "*.asset", SearchOption.TopDirectoryOnly))
        {
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (data == null)
            {
                continue;
            }

            if (data.Prefab == null)
            {
                Debug.LogWarning($"Enemy has no gameplay prefab: {path}", data);
                continue;
            }

            EnemyAgent agent = data.Prefab.GetComponent<EnemyAgent>();
            CharacterController controller = data.Prefab.GetComponent<CharacterController>();
            Transform importedVisual = data.Prefab.transform.Find("ImportedVisual");

            if (agent == null)
            {
                Debug.LogWarning($"Enemy prefab has no EnemyAgent: {path} -> {AssetDatabase.GetAssetPath(data.Prefab)}", data.Prefab);
            }

            if (controller == null)
            {
                Debug.LogWarning($"Enemy prefab has no CharacterController: {path} -> {AssetDatabase.GetAssetPath(data.Prefab)}", data.Prefab);
            }

            if (importedVisual == null)
            {
                Debug.LogWarning($"Enemy prefab has no ImportedVisual child: {path} -> {AssetDatabase.GetAssetPath(data.Prefab)}", data.Prefab);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

public static class EnvironmentPrefabCreator
{
    private const string OutputFolder = "Assets/_Project/Prefabs/Environment";
    private const string DungeonRemastered = "Assets/Art/Imported/KayKit_DungeonRemastered_1.1_FREE/KayKit_DungeonRemastered_1.1_FREE/Assets/fbx(unity)";
    private const string NaturePack = "Assets/Art/Imported/Ultimate Nature Pack by Quaternius/FBX";
    private const string ModularDungeon = "Assets/Art/Imported/Updated Modular Dungeon - May 2019-20260504T134550Z-3-001/Updated Modular Dungeon - May 2019/FBX";

    private static readonly EnvironmentSource[] Sources =
    {
        new("Dungeon_FloorTile_Large", $"{DungeonRemastered}/floor_tile_large.fbx"),
        new("Dungeon_FloorTile_Small", $"{DungeonRemastered}/floor_tile_small.fbx"),
        new("Dungeon_FloorTile_BrokenA", $"{DungeonRemastered}/floor_tile_small_broken_A.fbx"),
        new("Dungeon_FloorTile_BrokenB", $"{DungeonRemastered}/floor_tile_small_broken_B.fbx"),
        new("Dungeon_FloorTile_Grate", $"{DungeonRemastered}/floor_tile_grate.fbx"),
        new("Dungeon_FloorDirt_Large", $"{DungeonRemastered}/floor_dirt_large.fbx"),
        new("Dungeon_Wall", $"{DungeonRemastered}/wall.fbx"),
        new("Dungeon_Wall_Corner", $"{DungeonRemastered}/wall_corner.fbx"),
        new("Dungeon_Wall_Broken", $"{DungeonRemastered}/wall_broken.fbx"),
        new("Dungeon_Wall_Cracked", $"{DungeonRemastered}/wall_cracked.fbx"),
        new("Dungeon_Wall_Doorway", $"{DungeonRemastered}/wall_doorway.fbx"),
        new("Dungeon_Wall_Gated", $"{DungeonRemastered}/wall_gated.fbx"),
        new("Dungeon_Wall_Pillar", $"{DungeonRemastered}/wall_pillar.fbx"),
        new("Dungeon_Wall_Shelves", $"{DungeonRemastered}/wall_shelves.fbx"),
        new("Dungeon_Pillar", $"{DungeonRemastered}/pillar.fbx"),
        new("Dungeon_Pillar_Decorated", $"{DungeonRemastered}/pillar_decorated.fbx"),
        new("Dungeon_Stairs", $"{DungeonRemastered}/stairs.fbx"),
        new("Dungeon_Stairs_Wide", $"{DungeonRemastered}/stairs_wide.fbx"),
        new("Dungeon_Torch", $"{DungeonRemastered}/torch.fbx"),
        new("Dungeon_Torch_Lit", $"{DungeonRemastered}/torch_lit.fbx"),
        new("Dungeon_Torch_Mounted", $"{DungeonRemastered}/torch_mounted.fbx"),
        new("Dungeon_Banner_Blue", $"{DungeonRemastered}/banner_blue.fbx"),
        new("Dungeon_Banner_Red", $"{DungeonRemastered}/banner_red.fbx"),
        new("Dungeon_Banner_Shield_Blue", $"{DungeonRemastered}/banner_shield_blue.fbx"),
        new("Dungeon_Barrel_Large", $"{DungeonRemastered}/barrel_large.fbx"),
        new("Dungeon_Barrel_Small", $"{DungeonRemastered}/barrel_small.fbx"),
        new("Dungeon_Crate_Stacked", $"{DungeonRemastered}/crates_stacked.fbx"),
        new("Dungeon_Chest", $"{DungeonRemastered}/chest.fbx"),
        new("Dungeon_Chest_Gold", $"{DungeonRemastered}/chest_gold.fbx"),
        new("Dungeon_Table_Long", $"{DungeonRemastered}/table_long.fbx"),
        new("Dungeon_Table_Medium", $"{DungeonRemastered}/table_medium.fbx"),
        new("Dungeon_Table_Small", $"{DungeonRemastered}/table_small.fbx"),

        new("ModularDungeon_Arch", $"{ModularDungeon}/Arch.fbx"),
        new("ModularDungeon_ArchDoor", $"{ModularDungeon}/Arch_Door.fbx"),
        new("ModularDungeon_Column", $"{ModularDungeon}/Column.fbx"),
        new("ModularDungeon_Floor", $"{ModularDungeon}/Floor_Modular.fbx"),
        new("ModularDungeon_Wall", $"{ModularDungeon}/Wall_Modular.fbx"),
        new("ModularDungeon_Torch", $"{ModularDungeon}/Torch.fbx"),
        new("ModularDungeon_TableBig", $"{ModularDungeon}/Table_Big.fbx"),
        new("ModularDungeon_StatueHorse", $"{ModularDungeon}/Statue_Horse.fbx"),

        new("Nature_CommonTree_1", $"{NaturePack}/CommonTree_1.fbx"),
        new("Nature_CommonTree_Dead_1", $"{NaturePack}/CommonTree_Dead_1.fbx"),
        new("Nature_PineTree_1", $"{NaturePack}/PineTree_1.fbx"),
        new("Nature_BirchTree_1", $"{NaturePack}/BirchTree_1.fbx"),
        new("Nature_Bush_1", $"{NaturePack}/Bush_1.fbx"),
        new("Nature_BushBerries_1", $"{NaturePack}/BushBerries_1.fbx"),
        new("Nature_Grass", $"{NaturePack}/Grass.fbx"),
        new("Nature_Grass_Short", $"{NaturePack}/Grass_Short.fbx"),
        new("Nature_Rock_1", $"{NaturePack}/Rock_1.fbx"),
        new("Nature_Rock_2", $"{NaturePack}/Rock_2.fbx"),
        new("Nature_Rock_Moss_1", $"{NaturePack}/Rock_Moss_1.fbx"),
        new("Nature_TreeStump", $"{NaturePack}/TreeStump.fbx"),
        new("Nature_WoodLog", $"{NaturePack}/WoodLog.fbx"),
        new("Nature_Plant_1", $"{NaturePack}/Plant_1.fbx"),
    };

    [MenuItem("Soulstone/Assets/Create Environment Prefabs")]
    public static void CreateEnvironmentPrefabs()
    {
        EnsureFolder(OutputFolder);

        int created = 0;
        foreach (EnvironmentSource source in Sources)
        {
            if (CreatePrefab(source))
            {
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {created}/{Sources.Length} environment prefabs in {OutputFolder}.");
    }

    public static void CreateEnvironmentPrefabsBatch()
    {
        CreateEnvironmentPrefabs();
        EditorApplication.Exit(0);
    }

    private static bool CreatePrefab(EnvironmentSource source)
    {
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(source.ModelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning($"Environment source not found: {source.ModelPath}");
            return false;
        }

        string prefabPath = $"{OutputFolder}/{source.PrefabName}.prefab";
        GameObject root = new GameObject(source.PrefabName);

        GameObject modelRoot = new GameObject("ModelRoot");
        modelRoot.transform.SetParent(root.transform, false);

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        modelInstance.name = "VisualModel";
        modelInstance.transform.SetParent(modelRoot.transform, false);

        CreateMarker(root.transform, "PlacementPivot", Vector3.zero);
        CreateMarker(root.transform, "BoundsHint", Vector3.up);
        CreateMarker(root.transform, "NavObstacleHint", new Vector3(0f, 0.5f, 0f));

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return true;
    }

    private static void CreateMarker(Transform parent, string markerName, Vector3 localPosition)
    {
        GameObject marker = new GameObject(markerName);
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = localPosition;
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

    private readonly struct EnvironmentSource
    {
        public readonly string PrefabName;
        public readonly string ModelPath;

        public EnvironmentSource(string prefabName, string modelPath)
        {
            PrefabName = prefabName;
            ModelPath = modelPath;
        }
    }
}

using UnityEditor;
using UnityEngine;

public static class ImportedVisualDataConnector
{
    private const string CharacterVisualFolder = "Assets/_Project/Prefabs/Characters";
    private const string EnemyVisualFolder = "Assets/_Project/Prefabs/Enemies";
    private const string EnemyGameplayFolder = "Assets/_Project/Prefabs/Enemies/Gameplay";
    private const string WeaponVisualFolder = "Assets/_Project/Prefabs/Weapons";

    [MenuItem("Soulstone/Assets/Connect Imported Visuals To Data")]
    public static void ConnectImportedVisualsToData()
    {
        EnsureFolder(EnemyGameplayFolder);

        ConnectCharacters();
        ConnectWeapons();
        ConnectEnemies();
        ValidateMaps();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Connected imported visual prefabs to data assets.");
    }

    public static void ConnectImportedVisualsToDataBatch()
    {
        ConnectImportedVisualsToData();
        EditorApplication.Exit(0);
    }

    private static void ConnectCharacters()
    {
        AssignObject("Assets/Data/Characters/Hero_Assassin.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_RogueHooded.prefab");
        AssignObject("Assets/Data/Characters/Hero_Default.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Knight.prefab");
        AssignObject("Assets/Data/Characters/Hero_Knight.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Knight.prefab");
        AssignObject("Assets/Data/Characters/Hero_Necromancer.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Mage.prefab");
        AssignObject("Assets/Data/Characters/Hero_Paladin.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Knight.prefab");
        AssignObject("Assets/Data/Characters/Hero_Pyromancer.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Mage.prefab");
        AssignObject("Assets/Data/Characters/Hero_RagnarIronfang.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Barbarian.prefab");
        AssignObject("Assets/Data/Characters/Hero_Ranger.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Ranger.prefab");
        AssignObject("Assets/Data/Characters/Hero_RuneSmith.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Barbarian.prefab");
        AssignObject("Assets/Data/Characters/Hero_Viking.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Barbarian.prefab");
        AssignObject("Assets/Data/Characters/StormSentinel/Hero_StormSentinel.asset", "modelPrefab", $"{CharacterVisualFolder}/HeroVisual_Ranger.prefab");
    }

    private static void ConnectWeapons()
    {
        AssignObject("Assets/Data/Weapons/Weapon_AssassinBlades.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Dagger.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_BoneScepter.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_SkeletonStaff.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_FlameStaff.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Staff.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_GhostWand.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Wand.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_HolyHammer.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Sword_2H.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_KnightSword.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Sword_1H.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_Longbow.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Bow_WithString.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_Ragnar_GreatAxe.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Axe_2H.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_RuneHammer.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Axe_1H.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_SoulScythe.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_SkeletonStaff.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_Starter.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Sword_1H.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_StormBow.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Bow_WithString.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_ThunderSpear.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Staff.prefab");
        AssignObject("Assets/Data/Weapons/Weapon_VikingAxe.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Axe_2H.prefab");
        AssignObject("Assets/Data/Weapons/StormSentinel/Weapon_StormSigil.asset", "visualPrefab", $"{WeaponVisualFolder}/WeaponVisual_Wand.prefab");
    }

    private static void ConnectEnemies()
    {
        GameObject basic = CreateEnemyGameplayPrefab(
            "EnemyGameplay_BasicSkeletonMinion",
            "Assets/Prefabs/Enemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonMinion.prefab");
        GameObject fast = CreateEnemyGameplayPrefab(
            "EnemyGameplay_FastSkeletonArcher",
            "Assets/Prefabs/FastEnemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonArcher.prefab");
        GameObject tank = CreateEnemyGameplayPrefab(
            "EnemyGameplay_TankSkeletonWarrior",
            "Assets/Prefabs/TankEnemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonWarrior.prefab");
        GameObject mage = CreateEnemyGameplayPrefab(
            "EnemyGameplay_SkeletonMage",
            "Assets/Prefabs/FastEnemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonMage.prefab");
        GameObject elite = CreateEnemyGameplayPrefab(
            "EnemyGameplay_EliteSkeletonWarrior",
            "Assets/Prefabs/Enemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonWarrior.prefab");
        GameObject miniBoss = CreateEnemyGameplayPrefab(
            "EnemyGameplay_MiniBossSkeletonWarriorBroken",
            "Assets/Prefabs/Enemy.prefab",
            $"{EnemyVisualFolder}/EnemyVisual_SkeletonWarrior_Broken.prefab");

        AssignAssetObject("Assets/Data/Enemies/Enemy_Basic.asset", "prefab", basic);
        AssignAssetObject("Assets/Data/Enemies/Enemy_Fast.asset", "prefab", fast);
        AssignAssetObject("Assets/Data/Enemies/Enemy_Tank.asset", "prefab", tank);
        AssignAssetObject("Assets/Data/Enemies/Enemy_FrostShaman.asset", "prefab", mage);
        AssignAssetObject("Assets/Data/Enemies/Enemy_PoisonSpitter.asset", "prefab", fast);
        AssignAssetObject("Assets/Data/Enemies/Enemy_Elite.asset", "prefab", elite);
        AssignAssetObject("Assets/Data/Enemies/Enemy_MiniBoss.asset", "prefab", miniBoss);
    }

    private static void ValidateMaps()
    {
        EnsureMapPrefab("Assets/Data/Maps/Map_AshenQuarry.asset", "Assets/Prefabs/Maps/Map_AshenQuarry.prefab");
        EnsureMapPrefab("Assets/Data/Maps/Map_BoneCitadel.asset", "Assets/Prefabs/Maps/Map_BoneCitadel.prefab");
        EnsureMapPrefab("Assets/Data/Maps/Map_ForgottenPlains.asset", "Assets/Prefabs/Maps/Map_ForgottenPlains.prefab");
        EnsureMapPrefab("Assets/Data/Maps/Map_StormSpire.asset", "Assets/Prefabs/Maps/Map_StormSpire.prefab");
        EnsureMapPrefab("Assets/Data/Maps/Map_VenomMarsh.asset", "Assets/Prefabs/Maps/Map_VenomMarsh.prefab");
    }

    private static GameObject CreateEnemyGameplayPrefab(string prefabName, string basePrefabPath, string visualPrefabPath)
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
        if (basePrefab == null || visualPrefab == null)
        {
            Debug.LogWarning($"Cannot create enemy gameplay prefab {prefabName}. Base: {basePrefabPath}, Visual: {visualPrefabPath}");
            return null;
        }

        string prefabPath = $"{EnemyGameplayFolder}/{prefabName}.prefab";
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        root.name = prefabName;
        PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        DisableRootPrimitiveRenderer(root);
        EnsureEnemyRuntimeComponents(root);

        Transform oldVisual = root.transform.Find("ImportedVisual");
        if (oldVisual != null)
        {
            Object.DestroyImmediate(oldVisual.gameObject);
        }

        GameObject visualRoot = new GameObject("ImportedVisual");
        visualRoot.transform.SetParent(root.transform, false);
        visualRoot.transform.localScale = GetInverseScale(root.transform.localScale);

        GameObject visualInstance = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
        visualInstance.name = "VisualPrefab";
        visualInstance.transform.SetParent(visualRoot.transform, false);
        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    private static Vector3 GetInverseScale(Vector3 scale)
    {
        return new Vector3(
            Mathf.Approximately(scale.x, 0f) ? 1f : 1f / scale.x,
            Mathf.Approximately(scale.y, 0f) ? 1f : 1f / scale.y,
            Mathf.Approximately(scale.z, 0f) ? 1f : 1f / scale.z);
    }

    private static void DisableRootPrimitiveRenderer(GameObject root)
    {
        MeshRenderer renderer = root.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private static void EnsureEnemyRuntimeComponents(GameObject root)
    {
        if (root.GetComponent<CharacterController>() == null)
        {
            root.AddComponent<CharacterController>();
        }

        if (root.GetComponent<EnemyAgent>() == null)
        {
            root.AddComponent<EnemyAgent>();
        }
    }

    private static void EnsureMapPrefab(string dataPath, string prefabPath)
    {
        Object mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (mapPrefab == null)
        {
            Debug.LogWarning($"Expected map prefab not found: {prefabPath}");
            return;
        }

        SerializedObject serializedObject = LoadSerializedObject(dataPath);
        if (serializedObject == null)
        {
            return;
        }

        SerializedProperty property = serializedObject.FindProperty("mapPrefab");
        if (property != null && property.objectReferenceValue == null)
        {
            property.objectReferenceValue = mapPrefab;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AssignObject(string assetPath, string propertyName, string objectPath)
    {
        Object targetObject = AssetDatabase.LoadAssetAtPath<Object>(objectPath);
        if (targetObject == null)
        {
            Debug.LogWarning($"Target object not found for {assetPath}.{propertyName}: {objectPath}");
            return;
        }

        AssignAssetObject(assetPath, propertyName, targetObject);
    }

    private static void AssignAssetObject(string assetPath, string propertyName, Object targetObject)
    {
        SerializedObject serializedObject = LoadSerializedObject(assetPath);
        if (serializedObject == null || targetObject == null)
        {
            return;
        }

        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Property {propertyName} not found on {assetPath}");
            return;
        }

        property.objectReferenceValue = targetObject;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(serializedObject.targetObject);
    }

    private static SerializedObject LoadSerializedObject(string assetPath)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (asset == null)
        {
            Debug.LogWarning($"Data asset not found: {assetPath}");
            return null;
        }

        return new SerializedObject(asset);
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
}

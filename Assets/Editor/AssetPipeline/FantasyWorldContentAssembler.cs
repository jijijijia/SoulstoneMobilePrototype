using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FantasyWorldContentAssembler
{
    private const string EnvironmentFolder = "Assets/_Project/Prefabs/Environment";
    private const string MapOutputFolder = "Assets/_Project/Prefabs/Maps";
    private const string MaterialFolder = "Assets/_Project/Materials/Maps";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Soulstone/Assets/Assemble Fantasy Menu And Maps")]
    public static void AssembleFantasyMenuAndMaps()
    {
        EnsureFolder(MapOutputFolder);
        EnsureFolder(MaterialFolder);

        MapBuildResult crypt = CreateIronCandleCrypt();
        MapBuildResult forge = CreateEmberVault();
        MapBuildResult wilds = CreateElderrootWilds();
        MapBuildResult ruins = CreateStormglassRuins();
        MapBuildResult marsh = CreateRotfenHollow();

        UpdateMapData("Assets/Data/Maps/Map_BoneCitadel.asset", crypt);
        UpdateMapData("Assets/Data/Maps/Map_AshenQuarry.asset", forge);
        UpdateMapData("Assets/Data/Maps/Map_ForgottenPlains.asset", wilds);
        UpdateMapData("Assets/Data/Maps/Map_StormSpire.asset", ruins);
        UpdateMapData("Assets/Data/Maps/Map_VenomMarsh.asset", marsh);

        RebuildMainMenuScene();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Fantasy menu scene and map prefabs assembled.");
    }

    public static void AssembleFantasyMenuAndMapsBatch()
    {
        AssembleFantasyMenuAndMaps();
        EditorApplication.Exit(0);
    }

    private static MapBuildResult CreateIronCandleCrypt()
    {
        GameObject root = CreateMapRoot("Map_IronCandleCrypt", new Color(0.21f, 0.21f, 0.23f), out Transform environment);
        BuildDungeonBoundary(environment, fullWalls: true);

        Place(environment, "Dungeon_Banner_Shield_Blue", new Vector3(-5.6f, 0f, 8.6f), Quaternion.Euler(0f, 180f, 0f), 1.1f);
        Place(environment, "Dungeon_Banner_Blue", new Vector3(5.6f, 0f, 8.6f), Quaternion.Euler(0f, 180f, 0f), 1f);
        Place(environment, "Dungeon_Chest_Gold", new Vector3(6f, 0f, 5.5f), Quaternion.Euler(0f, -35f, 0f), 1f);
        Place(environment, "Dungeon_Torch_Lit", new Vector3(-8.2f, 0.2f, 7.8f), Quaternion.identity, 1f);
        Place(environment, "Dungeon_Torch_Lit", new Vector3(8.2f, 0.2f, 7.8f), Quaternion.identity, 1f);

        CreatePrimitiveProp(environment, "IronCandle_A", PrimitiveType.Cylinder, new Vector3(-4f, 0.7f, -2.5f), new Vector3(0.35f, 1.4f, 0.35f), GetMaterial("MAT_IronCandle", new Color(0.42f, 0.39f, 0.34f)));
        CreatePrimitiveProp(environment, "IronCandle_B", PrimitiveType.Cylinder, new Vector3(4f, 0.7f, -3.6f), new Vector3(0.35f, 1.4f, 0.35f), GetMaterial("MAT_IronCandle", new Color(0.42f, 0.39f, 0.34f)));
        CreatePrimitiveProp(environment, "CandleFlame_A", PrimitiveType.Sphere, new Vector3(-4f, 1.55f, -2.5f), new Vector3(0.25f, 0.25f, 0.25f), GetMaterial("MAT_FlameOrange", new Color(1f, 0.34f, 0.04f), true));
        CreatePrimitiveProp(environment, "CandleFlame_B", PrimitiveType.Sphere, new Vector3(4f, 1.55f, -3.6f), new Vector3(0.25f, 0.25f, 0.25f), GetMaterial("MAT_FlameOrange", new Color(1f, 0.34f, 0.04f), true));

        string path = SaveMap(root, "Map_IronCandleCrypt");
        return new MapBuildResult(
            "iron_candle_crypt",
            "Склеп Железных Свечей",
            "Средняя",
            "Подземелье / нежить",
            "Каменные залы, скелеты-воины, маги и редкие элитные стражи.",
            "Осколки душ и повышенный шанс боевых улучшений.",
            path,
            new Vector3(0f, 1.08f, -4f),
            1,
            1f,
            1f,
            1f);
    }

    private static MapBuildResult CreateEmberVault()
    {
        GameObject root = CreateMapRoot("Map_EmberVault", new Color(0.28f, 0.17f, 0.12f), out Transform environment);
        BuildDungeonBoundary(environment, fullWalls: false);

        Place(environment, "Dungeon_Table_Long", new Vector3(-5.5f, 0f, 3.2f), Quaternion.Euler(0f, 20f, 0f), 1f);
        Place(environment, "Dungeon_Barrel_Large", new Vector3(5.7f, 0f, 4.2f), Quaternion.identity, 1f);
        Place(environment, "Dungeon_Crate_Stacked", new Vector3(7.1f, 0f, -5.5f), Quaternion.identity, 1f);
        Place(environment, "Dungeon_Pillar_Decorated", new Vector3(-7f, 0f, -7f), Quaternion.identity, 1f);
        Place(environment, "Dungeon_Pillar_Decorated", new Vector3(7f, 0f, -7f), Quaternion.identity, 1f);

        CreatePrimitiveProp(environment, "LavaVent_A", PrimitiveType.Cylinder, new Vector3(-3.5f, 0.05f, -5.2f), new Vector3(1.7f, 0.08f, 1.7f), GetMaterial("MAT_EmberLava", new Color(1f, 0.22f, 0.02f), true));
        CreatePrimitiveProp(environment, "LavaVent_B", PrimitiveType.Cylinder, new Vector3(3.2f, 0.05f, 5.5f), new Vector3(1.3f, 0.08f, 1.3f), GetMaterial("MAT_EmberLava", new Color(1f, 0.22f, 0.02f), true));
        CreatePointLight(environment, "LavaGlow_A", new Vector3(-3.5f, 1f, -5.2f), new Color(1f, 0.34f, 0.08f), 3f, 6f);
        CreatePointLight(environment, "LavaGlow_B", new Vector3(3.2f, 1f, 5.5f), new Color(1f, 0.34f, 0.08f), 3f, 6f);

        string path = SaveMap(root, "Map_EmberVault");
        return new MapBuildResult(
            "ember_vault",
            "Пылающее Хранилище",
            "Средняя+",
            "Кузня / камень / огонь",
            "Арена с жаровыми разломами, быстрыми врагами и тяжёлыми стражами.",
            "Больше золота и шанс огненных улучшений.",
            path,
            new Vector3(0f, 1.08f, 0f),
            2,
            1.05f,
            1.08f,
            1.05f);
    }

    private static MapBuildResult CreateElderrootWilds()
    {
        GameObject root = CreateMapRoot("Map_ElderrootWilds", new Color(0.22f, 0.36f, 0.22f), out Transform environment);
        BuildOpenArena(environment, "Elderroot", GetMaterial("MAT_ElderrootMark", new Color(0.18f, 0.5f, 0.23f)));
        ScatterNature(environment, dead: false);

        CreatePrimitiveProp(environment, "Moonwell_Water", PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 6f), new Vector3(2.2f, 0.06f, 2.2f), GetMaterial("MAT_MoonwellWater", new Color(0.1f, 0.58f, 0.85f), true));
        Place(environment, "Nature_Rock_Moss_1", new Vector3(-2.4f, 0f, 5.6f), Quaternion.Euler(0f, 30f, 0f), 1.1f);
        Place(environment, "Nature_Rock_Moss_1", new Vector3(2.4f, 0f, 6.1f), Quaternion.Euler(0f, -35f, 0f), 1.1f);
        CreatePointLight(environment, "MoonwellLight", new Vector3(0f, 1f, 6f), new Color(0.2f, 0.75f, 1f), 2.5f, 7f);

        string path = SaveMap(root, "Map_ElderrootWilds");
        return new MapBuildResult(
            "elderroot_wilds",
            "Чаща Старого Корня",
            "Лёгкая",
            "Лес / руины",
            "Просторная лесная карта с деревьями, камнями и дальними врагами.",
            "Повышенный опыт и шанс природных улучшений.",
            path,
            new Vector3(0f, 1.08f, 0f),
            1,
            0.95f,
            0.95f,
            1.08f);
    }

    private static MapBuildResult CreateStormglassRuins()
    {
        GameObject root = CreateMapRoot("Map_StormglassRuins", new Color(0.2f, 0.23f, 0.32f), out Transform environment);
        BuildOpenArena(environment, "Stormglass", GetMaterial("MAT_StormRune", new Color(0.18f, 0.48f, 1f), true));

        Place(environment, "ModularDungeon_Arch", new Vector3(0f, 0f, 8.5f), Quaternion.identity, 1.2f);
        Place(environment, "ModularDungeon_Column", new Vector3(-7f, 0f, 4f), Quaternion.identity, 1.1f);
        Place(environment, "ModularDungeon_Column", new Vector3(7f, 0f, 4f), Quaternion.identity, 1.1f);
        Place(environment, "Dungeon_Wall_Broken", new Vector3(-6.2f, 0f, -7.3f), Quaternion.Euler(0f, 30f, 0f), 1f);
        Place(environment, "Dungeon_Wall_Cracked", new Vector3(6.2f, 0f, -6.7f), Quaternion.Euler(0f, -40f, 0f), 1f);

        CreateCrystal(environment, "StormglassCrystal_A", new Vector3(-3.5f, 0.8f, 2f), 1.6f);
        CreateCrystal(environment, "StormglassCrystal_B", new Vector3(4.2f, 0.8f, -2.4f), 1.5f);
        CreateCrystal(environment, "StormglassCrystal_C", new Vector3(0f, 0.6f, -6f), 1.2f);
        CreatePointLight(environment, "StormglassLight", new Vector3(0f, 4.5f, 0f), new Color(0.35f, 0.55f, 1f), 2.2f, 12f);

        string path = SaveMap(root, "Map_StormglassRuins");
        return new MapBuildResult(
            "stormglass_ruins",
            "Руины Грозового Стекла",
            "Сложная",
            "Руины / гроза",
            "Разрушенный аванпост с открытыми проходами, шаманами и элитными волнами.",
            "Шанс молниевых навыков и повышенные награды за боссов.",
            path,
            new Vector3(0f, 1.08f, -2f),
            3,
            1.15f,
            1.12f,
            1.1f);
    }

    private static MapBuildResult CreateRotfenHollow()
    {
        GameObject root = CreateMapRoot("Map_RotfenHollow", new Color(0.17f, 0.28f, 0.18f), out Transform environment);
        BuildOpenArena(environment, "Rotfen", GetMaterial("MAT_RotfenMark", new Color(0.2f, 0.55f, 0.2f), true));
        ScatterNature(environment, dead: true);

        CreatePrimitiveProp(environment, "PoisonPool_A", PrimitiveType.Cylinder, new Vector3(-4.5f, 0.04f, -4.5f), new Vector3(2f, 0.06f, 2f), GetMaterial("MAT_PoisonPool", new Color(0.18f, 0.72f, 0.2f), true));
        CreatePrimitiveProp(environment, "PoisonPool_B", PrimitiveType.Cylinder, new Vector3(5f, 0.04f, 5f), new Vector3(1.7f, 0.06f, 1.7f), GetMaterial("MAT_PoisonPool", new Color(0.18f, 0.72f, 0.2f), true));
        Place(environment, "Nature_WoodLog", new Vector3(1.5f, 0f, 7.4f), Quaternion.Euler(0f, 80f, 0f), 1.1f);
        Place(environment, "Nature_TreeStump", new Vector3(-7f, 0f, 2.4f), Quaternion.identity, 1f);
        CreatePointLight(environment, "PoisonGlow_A", new Vector3(-4.5f, 0.6f, -4.5f), new Color(0.25f, 1f, 0.25f), 1.8f, 5f);
        CreatePointLight(environment, "PoisonGlow_B", new Vector3(5f, 0.6f, 5f), new Color(0.25f, 1f, 0.25f), 1.8f, 5f);

        string path = SaveMap(root, "Map_RotfenHollow");
        return new MapBuildResult(
            "rotfen_hollow",
            "Гнилая Низина",
            "Сложная",
            "Болото / яд",
            "Тёмная болотная арена с ядовитыми лужами, стрелками и магами.",
            "Шанс ядовитых улучшений и больше душ за элиту.",
            path,
            new Vector3(0f, 1.08f, 0f),
            4,
            1.18f,
            1.15f,
            1.12f);
    }

    private static GameObject CreateMapRoot(string rootName, Color groundColor, out Transform environment)
    {
        GameObject root = new GameObject(rootName);
        environment = new GameObject("Environment").transform;
        environment.SetParent(root.transform, false);
        CreateGround(environment, groundColor);
        CreateMapMarkers(root.transform);
        return root;
    }

    private static void CreateGround(Transform parent, Color color)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(parent, false);
        ground.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        ground.transform.localScale = new Vector3(40f, 0.2f, 40f);
        ground.GetComponent<Renderer>().sharedMaterial = GetMaterial("MAT_MapGround_" + ColorUtility.ToHtmlStringRGB(color), color);
    }

    private static void BuildDungeonBoundary(Transform parent, bool fullWalls)
    {
        for (int x = -4; x <= 4; x++)
        {
            string northWall = x == 0 && !fullWalls ? "Dungeon_Wall_Doorway" : "Dungeon_Wall";
            Place(parent, northWall, new Vector3(x * 2f, 0f, 10f), Quaternion.identity, 1f);
            Place(parent, "Dungeon_Wall", new Vector3(x * 2f, 0f, -10f), Quaternion.Euler(0f, 180f, 0f), 1f);
        }

        for (int z = -4; z <= 4; z++)
        {
            Place(parent, "Dungeon_Wall_Corner", new Vector3(-10f, 0f, z * 2f), Quaternion.Euler(0f, 90f, 0f), 1f);
            Place(parent, "Dungeon_Wall_Corner", new Vector3(10f, 0f, z * 2f), Quaternion.Euler(0f, -90f, 0f), 1f);
        }

        Place(parent, "Dungeon_Pillar", new Vector3(-7f, 0f, 7f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Pillar", new Vector3(7f, 0f, 7f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Pillar_Decorated", new Vector3(-7f, 0f, -7f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Pillar_Decorated", new Vector3(7f, 0f, -7f), Quaternion.identity, 1f);
    }

    private static void BuildOpenArena(Transform parent, string prefix, Material groundAccent)
    {
        for (int i = 0; i < 16; i++)
        {
            float angle = i * Mathf.PI * 2f / 16f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 17f, 0f, Mathf.Sin(angle) * 17f);
            string prefab = i % 3 == 0 ? "Nature_Rock_1" : "Nature_Rock_2";
            Place(parent, prefab, position, Quaternion.Euler(0f, i * 23f, 0f), 1.1f);
        }

        CreatePrimitiveProp(parent, $"{prefix}_CenterRune", PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0f), new Vector3(2f, 0.05f, 2f), groundAccent);
    }

    private static void ScatterNature(Transform parent, bool dead)
    {
        string mainTree = dead ? "Nature_CommonTree_Dead_1" : "Nature_CommonTree_1";
        Place(parent, mainTree, new Vector3(-8f, 0f, 6f), Quaternion.Euler(0f, 20f, 0f), 1.2f);
        Place(parent, mainTree, new Vector3(8f, 0f, -5f), Quaternion.Euler(0f, -35f, 0f), 1.2f);
        Place(parent, dead ? "Nature_TreeStump" : "Nature_PineTree_1", new Vector3(6f, 0f, 7f), Quaternion.identity, 1f);
        Place(parent, "Nature_Bush_1", new Vector3(-5f, 0f, -7f), Quaternion.identity, 1f);
        Place(parent, "Nature_BushBerries_1", new Vector3(4f, 0f, -8f), Quaternion.identity, 1f);
        Place(parent, "Nature_Grass", new Vector3(-2f, 0f, 8f), Quaternion.identity, 1f);
        Place(parent, "Nature_Rock_Moss_1", new Vector3(8f, 0f, 2f), Quaternion.identity, 1f);
        Place(parent, "Nature_WoodLog", new Vector3(-8f, 0f, -2f), Quaternion.Euler(0f, 35f, 0f), 1f);
    }

    private static void CreateCrystal(Transform parent, string name, Vector3 position, float height)
    {
        GameObject crystal = CreatePrimitiveProp(parent, name, PrimitiveType.Cylinder, position, new Vector3(0.45f, height, 0.45f), GetMaterial("MAT_StormglassCrystal", new Color(0.18f, 0.55f, 1f), true));
        crystal.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    private static void CreateMapMarkers(Transform root)
    {
        GameObject markers = new GameObject("GameplayMarkers");
        markers.transform.SetParent(root, false);
        CreateMarker(markers.transform, "PlayerStart", new Vector3(0f, 0.05f, 0f), Color.green);
        CreateMarker(markers.transform, "SpawnRing_North", new Vector3(0f, 0.05f, 18f), Color.red);
        CreateMarker(markers.transform, "SpawnRing_East", new Vector3(18f, 0.05f, 0f), Color.red);
        CreateMarker(markers.transform, "SpawnRing_South", new Vector3(0f, 0.05f, -18f), Color.red);
        CreateMarker(markers.transform, "SpawnRing_West", new Vector3(-18f, 0.05f, 0f), Color.red);
    }

    private static void RebuildMainMenuScene()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

        GameObject oldEnvironment = GameObject.Find("Environment");
        if (oldEnvironment != null)
        {
            Object.DestroyImmediate(oldEnvironment);
        }

        GameObject environment = new GameObject("Environment");
        BuildMenuDungeonRoom(environment.transform);
        RebuildMenuLighting();
        ConfigureMenuCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildMenuDungeonRoom(Transform parent)
    {
        CreatePrimitiveProp(parent, "MenuFloor", PrimitiveType.Cube, new Vector3(0f, -0.1f, 0f), new Vector3(14f, 0.2f, 9f), GetMaterial("MAT_MenuFloor", new Color(0.15f, 0.1f, 0.07f)));

        for (int x = -5; x <= 5; x++)
        {
            Place(parent, "Dungeon_Wall", new Vector3(x * 1.5f, 0f, 4.2f), Quaternion.identity, 0.9f);
        }

        for (int z = -2; z <= 2; z++)
        {
            Place(parent, "Dungeon_Wall_Corner", new Vector3(-6.5f, 0f, z * 1.7f), Quaternion.Euler(0f, 90f, 0f), 0.9f);
            Place(parent, "Dungeon_Wall_Corner", new Vector3(6.5f, 0f, z * 1.7f), Quaternion.Euler(0f, -90f, 0f), 0.9f);
        }

        Place(parent, "ModularDungeon_ArchDoor", new Vector3(-3.6f, 0f, 4.05f), Quaternion.identity, 1.1f);
        Place(parent, "Dungeon_Banner_Shield_Blue", new Vector3(0.5f, 1.7f, 3.85f), Quaternion.identity, 1.2f);
        Place(parent, "Dungeon_Banner_Red", new Vector3(3.6f, 1.6f, 3.85f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Torch_Lit", new Vector3(-5.4f, 1.1f, 3.7f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Torch_Lit", new Vector3(5.4f, 1.1f, 3.7f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Table_Medium", new Vector3(4.5f, 0f, 1.2f), Quaternion.Euler(0f, -20f, 0f), 1f);
        Place(parent, "Dungeon_Chest", new Vector3(-5.2f, 0f, 1.2f), Quaternion.Euler(0f, 25f, 0f), 0.9f);
        Place(parent, "Dungeon_Barrel_Small", new Vector3(-4.7f, 0f, -1.3f), Quaternion.identity, 1f);
        Place(parent, "Dungeon_Crate_Stacked", new Vector3(5.4f, 0f, -1.5f), Quaternion.Euler(0f, -15f, 0f), 0.85f);

        CreatePrimitiveProp(parent, "MenuPotion_Blue", PrimitiveType.Sphere, new Vector3(4.1f, 0.9f, 1f), new Vector3(0.18f, 0.25f, 0.18f), GetMaterial("MAT_PotionBlue", new Color(0.08f, 0.3f, 1f), true));
        CreatePrimitiveProp(parent, "MenuPotion_Green", PrimitiveType.Sphere, new Vector3(4.6f, 0.9f, 1.05f), new Vector3(0.18f, 0.25f, 0.18f), GetMaterial("MAT_PotionGreen", new Color(0.1f, 0.8f, 0.25f), true));
        CreatePrimitiveProp(parent, "MenuPotion_Red", PrimitiveType.Sphere, new Vector3(5.1f, 0.9f, 1f), new Vector3(0.18f, 0.25f, 0.18f), GetMaterial("MAT_PotionRed", new Color(1f, 0.08f, 0.05f), true));

        CreatePrimitiveProp(parent, "MenuMagicCircle", PrimitiveType.Cylinder, new Vector3(2.45f, 0.02f, -0.2f), new Vector3(1.45f, 0.04f, 1.45f), GetMaterial("MAT_MenuMagicCircle", new Color(1f, 0.25f, 0f), true));
        CreatePointLight(parent, "MenuMagicCircleLight", new Vector3(2.45f, 0.6f, -0.2f), new Color(1f, 0.32f, 0.05f), 4f, 4f);
    }

    private static void RebuildMenuLighting()
    {
        GameObject oldLighting = GameObject.Find("Lighting");
        if (oldLighting != null)
        {
            Object.DestroyImmediate(oldLighting);
        }

        GameObject lighting = new GameObject("Lighting");
        GameObject directional = new GameObject("Directional Light");
        directional.transform.SetParent(lighting.transform, false);
        Light light = directional.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.42f;
        light.color = new Color(1f, 0.75f, 0.55f);
        directional.transform.rotation = Quaternion.Euler(50f, -25f, 0f);

        CreatePointLight(lighting.transform, "TorchLight_Left", new Vector3(-5.4f, 1.7f, 3.2f), new Color(1f, 0.45f, 0.12f), 3f, 5f);
        CreatePointLight(lighting.transform, "TorchLight_Right", new Vector3(5.4f, 1.7f, 3.2f), new Color(1f, 0.45f, 0.12f), 3f, 5f);
    }

    private static void ConfigureMenuCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.transform.position = new Vector3(0.25f, 2.15f, -6.8f);
        camera.transform.rotation = Quaternion.Euler(11f, -4f, 0f);
        camera.fieldOfView = 48f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 100f;
    }

    private static void UpdateMapData(string dataPath, MapBuildResult result)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(dataPath);
        if (asset == null)
        {
            Debug.LogWarning($"MapData not found: {dataPath}");
            return;
        }

        SerializedObject so = new SerializedObject(asset);
        SetString(so, "mapId", result.MapId);
        SetString(so, "displayName", result.DisplayName);
        SetString(so, "difficulty", result.Difficulty);
        SetString(so, "biome", result.Biome);
        SetString(so, "enemyDescription", result.EnemyDescription);
        SetString(so, "rewardsDescription", result.RewardsDescription);
        SetInt(so, "recommendedLevel", result.RecommendedLevel);
        SetFloat(so, "experienceMultiplier", result.ExperienceMultiplier);
        SetFloat(so, "enemyHealthMultiplier", result.EnemyHealthMultiplier);
        SetFloat(so, "enemyDamageMultiplier", result.EnemyDamageMultiplier);
        SetObject(so, "mapPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(result.PrefabPath));
        SetVector3(so, "playerSpawnPosition", result.PlayerSpawnPosition);
        SetFloat(so, "playerSpawnClearRadius", 2f);
        SetBool(so, "overrideMapBounds", true);
        SetVector2(so, "mapMin", new Vector2(-20f, -20f));
        SetVector2(so, "mapMax", new Vector2(20f, 20f));
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    private static string SaveMap(GameObject root, string prefabName)
    {
        string path = $"{MapOutputFolder}/{prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return path;
    }

    private static void Place(Transform parent, string prefabName, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentFolder}/{prefabName}.prefab");
        if (prefab == null)
        {
            Debug.LogWarning($"Environment prefab not found: {prefabName}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = prefabName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = position;
        instance.transform.localRotation = rotation;
        instance.transform.localScale = Vector3.one * scale;
    }

    private static GameObject CreatePrimitiveProp(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        GameObject prop = GameObject.CreatePrimitive(type);
        prop.name = name;
        prop.transform.SetParent(parent, false);
        prop.transform.localPosition = position;
        prop.transform.localScale = scale;
        prop.GetComponent<Renderer>().sharedMaterial = material;
        return prop;
    }

    private static void CreateMarker(Transform parent, string name, Vector3 position, Color color)
    {
        CreatePrimitiveProp(parent, name, PrimitiveType.Cylinder, position, new Vector3(0.6f, 0.04f, 0.6f), GetMaterial("MAT_" + name, color));
    }

    private static void CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static Material GetMaterial(string name, Color color, bool emission = false)
    {
        EnsureFolder(MaterialFolder);
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader)
        {
            name = name,
            color = color
        };

        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.5f);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SetString(SerializedObject so, string propertyName, string value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetObject(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetVector3(SerializedObject so, string propertyName, Vector3 value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }

    private static void SetVector2(SerializedObject so, string propertyName, Vector2 value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2Value = value;
        }
    }

    private static void SetBool(SerializedObject so, string propertyName, bool value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetInt(SerializedObject so, string propertyName, int value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetFloat(SerializedObject so, string propertyName, float value)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
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

    private readonly struct MapBuildResult
    {
        public readonly string MapId;
        public readonly string DisplayName;
        public readonly string Difficulty;
        public readonly string Biome;
        public readonly string EnemyDescription;
        public readonly string RewardsDescription;
        public readonly string PrefabPath;
        public readonly Vector3 PlayerSpawnPosition;
        public readonly int RecommendedLevel;
        public readonly float ExperienceMultiplier;
        public readonly float EnemyHealthMultiplier;
        public readonly float EnemyDamageMultiplier;

        public MapBuildResult(string mapId, string displayName, string difficulty, string biome, string enemyDescription, string rewardsDescription, string prefabPath, Vector3 playerSpawnPosition, int recommendedLevel, float experienceMultiplier, float enemyHealthMultiplier, float enemyDamageMultiplier)
        {
            MapId = mapId;
            DisplayName = displayName;
            Difficulty = difficulty;
            Biome = biome;
            EnemyDescription = enemyDescription;
            RewardsDescription = rewardsDescription;
            PrefabPath = prefabPath;
            PlayerSpawnPosition = playerSpawnPosition;
            RecommendedLevel = recommendedLevel;
            ExperienceMultiplier = experienceMultiplier;
            EnemyHealthMultiplier = enemyHealthMultiplier;
            EnemyDamageMultiplier = enemyDamageMultiplier;
        }
    }
}

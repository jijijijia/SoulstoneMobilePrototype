using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AssetIntegrationTestSceneCreator
{
    private const string ScenePath = "Assets/_Project/Scenes/AssetIntegrationTest.unity";
    private const string EnvironmentFolder = "Assets/_Project/Prefabs/Environment";
    private const string MarkerMaterialFolder = "Assets/_Project/Materials/EditorMarkers";

    [MenuItem("Soulstone/Assets/Create Asset Integration Test Scene")]
    public static void CreateAssetIntegrationTestScene()
    {
        EnsureFolder("Assets/_Project/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "AssetIntegrationTest";

        GameObject root = new GameObject("AssetIntegrationTestScene");
        GameObject environment = CreateChild(root.transform, "Environment");
        GameObject dungeon = CreateChild(environment.transform, "DungeonArena");
        GameObject nature = CreateChild(environment.transform, "NaturePreview");
        GameObject gameplayMarkers = CreateChild(root.transform, "GameplayMarkers");
        GameObject lighting = CreateChild(root.transform, "Lighting");

        BuildDungeonArena(dungeon.transform);
        BuildNaturePreview(nature.transform);
        BuildGameplayMarkers(gameplayMarkers.transform);
        BuildLighting(lighting.transform);
        BuildCamera(root.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created asset integration test scene at {ScenePath}.");
    }

    public static void CreateAssetIntegrationTestSceneBatch()
    {
        CreateAssetIntegrationTestScene();
        EditorApplication.Exit(0);
    }

    private static void BuildDungeonArena(Transform parent)
    {
        GameObject floorTile = LoadEnvironmentPrefab("Dungeon_FloorTile_Large");
        GameObject brokenTileA = LoadEnvironmentPrefab("Dungeon_FloorTile_BrokenA");
        GameObject grateTile = LoadEnvironmentPrefab("Dungeon_FloorTile_Grate");

        const int halfSize = 4;
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int z = -halfSize; z <= halfSize; z++)
            {
                GameObject source = floorTile;
                if ((x + z) % 9 == 0 && brokenTileA != null)
                {
                    source = brokenTileA;
                }
                else if ((x - z) % 11 == 0 && grateTile != null)
                {
                    source = grateTile;
                }

                InstantiatePrefab(source, parent, new Vector3(x * 2f, 0f, z * 2f), Quaternion.identity, $"Floor_{x}_{z}");
            }
        }

        GameObject wall = LoadEnvironmentPrefab("Dungeon_Wall");
        GameObject wallBroken = LoadEnvironmentPrefab("Dungeon_Wall_Broken");
        GameObject wallDoorway = LoadEnvironmentPrefab("Dungeon_Wall_Doorway");
        GameObject pillar = LoadEnvironmentPrefab("Dungeon_Pillar");

        for (int i = -4; i <= 4; i++)
        {
            InstantiatePrefab(i == 0 ? wallDoorway : wall, parent, new Vector3(i * 2f, 0f, 10f), Quaternion.identity, $"BackWall_{i}");
            InstantiatePrefab(i == -2 ? wallBroken : wall, parent, new Vector3(i * 2f, 0f, -10f), Quaternion.Euler(0f, 180f, 0f), $"FrontWall_{i}");
            InstantiatePrefab(wall, parent, new Vector3(-10f, 0f, i * 2f), Quaternion.Euler(0f, 90f, 0f), $"LeftWall_{i}");
            InstantiatePrefab(wall, parent, new Vector3(10f, 0f, i * 2f), Quaternion.Euler(0f, -90f, 0f), $"RightWall_{i}");
        }

        InstantiatePrefab(pillar, parent, new Vector3(-6f, 0f, 6f), Quaternion.identity, "Pillar_NorthWest");
        InstantiatePrefab(pillar, parent, new Vector3(6f, 0f, 6f), Quaternion.identity, "Pillar_NorthEast");
        InstantiatePrefab(pillar, parent, new Vector3(-6f, 0f, -6f), Quaternion.identity, "Pillar_SouthWest");
        InstantiatePrefab(pillar, parent, new Vector3(6f, 0f, -6f), Quaternion.identity, "Pillar_SouthEast");

        PlaceDungeonProps(parent);
    }

    private static void PlaceDungeonProps(Transform parent)
    {
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Torch_Lit"), parent, new Vector3(-7.5f, 1.5f, 9.5f), Quaternion.identity, "Torch_Left");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Torch_Lit"), parent, new Vector3(7.5f, 1.5f, 9.5f), Quaternion.identity, "Torch_Right");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Banner_Shield_Blue"), parent, new Vector3(0f, 1.8f, 9.6f), Quaternion.identity, "Banner_BackWall");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Chest_Gold"), parent, new Vector3(-3f, 0f, 7f), Quaternion.Euler(0f, 180f, 0f), "Chest_Test");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Barrel_Large"), parent, new Vector3(4f, 0f, 7f), Quaternion.identity, "Barrel_Test");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Crate_Stacked"), parent, new Vector3(6f, 0f, -3f), Quaternion.identity, "Crates_Test");
        InstantiatePrefab(LoadEnvironmentPrefab("Dungeon_Table_Medium"), parent, new Vector3(-6f, 0f, -3f), Quaternion.Euler(0f, 25f, 0f), "Table_Test");
    }

    private static void BuildNaturePreview(Transform parent)
    {
        parent.position = new Vector3(16f, 0f, 0f);

        InstantiatePrefab(LoadEnvironmentPrefab("Nature_CommonTree_1"), parent, new Vector3(0f, 0f, 0f), Quaternion.identity, "CommonTree");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_PineTree_1"), parent, new Vector3(3f, 0f, 2f), Quaternion.identity, "PineTree");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_BirchTree_1"), parent, new Vector3(-3f, 0f, 2f), Quaternion.identity, "BirchTree");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_Rock_1"), parent, new Vector3(2.5f, 0f, -2f), Quaternion.identity, "Rock");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_Rock_Moss_1"), parent, new Vector3(-2.5f, 0f, -2f), Quaternion.identity, "MossRock");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_Bush_1"), parent, new Vector3(0f, 0f, -3f), Quaternion.identity, "Bush");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_Grass"), parent, new Vector3(1f, 0f, -4f), Quaternion.identity, "Grass");
        InstantiatePrefab(LoadEnvironmentPrefab("Nature_Plant_1"), parent, new Vector3(-1f, 0f, -4f), Quaternion.identity, "Plant");
    }

    private static void BuildGameplayMarkers(Transform parent)
    {
        CreateMarker(parent, "PlayerStart", new Vector3(0f, 0f, 0f), Color.green);
        CreateMarker(parent, "EnemySpawnRing_North", new Vector3(0f, 0f, 8f), Color.red);
        CreateMarker(parent, "EnemySpawnRing_East", new Vector3(8f, 0f, 0f), Color.red);
        CreateMarker(parent, "EnemySpawnRing_South", new Vector3(0f, 0f, -8f), Color.red);
        CreateMarker(parent, "EnemySpawnRing_West", new Vector3(-8f, 0f, 0f), Color.red);
        CreateMarker(parent, "BossSpawnPreview", new Vector3(0f, 0f, 6f), new Color(0.6f, 0.1f, 1f));
    }

    private static void BuildLighting(Transform parent)
    {
        GameObject directional = new GameObject("Directional Light");
        directional.transform.SetParent(parent, false);
        Light directionalLight = directional.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = 1.2f;
        directionalLight.color = new Color(1f, 0.86f, 0.68f);
        directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        CreatePointLight(parent, "WarmDungeonLight", new Vector3(0f, 4f, 4f), new Color(1f, 0.52f, 0.18f), 5f, 14f);
        CreatePointLight(parent, "NaturePreviewLight", new Vector3(16f, 4f, 1f), new Color(0.75f, 0.95f, 1f), 2f, 10f);
    }

    private static void BuildCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(parent, false);
        cameraObject.transform.position = new Vector3(0f, 13f, -15f);
        cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 45f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 200f;

        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateMarker(Transform parent, string markerName, Vector3 position, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = markerName;
        marker.transform.SetParent(parent, false);
        marker.transform.position = position + new Vector3(0f, 0.03f, 0f);
        marker.transform.localScale = new Vector3(0.6f, 0.04f, 0.6f);

        marker.GetComponent<Renderer>().sharedMaterial = GetOrCreateMarkerMaterial(markerName, color);
    }

    private static void CreatePointLight(Transform parent, string lightName, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(lightName);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position;

        Light pointLight = lightObject.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = color;
        pointLight.intensity = intensity;
        pointLight.range = range;
    }

    private static GameObject CreateChild(Transform parent, string childName)
    {
        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject LoadEnvironmentPrefab(string prefabName)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>($"{EnvironmentFolder}/{prefabName}.prefab");
    }

    private static void InstantiatePrefab(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, string objectName)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"Skipping missing prefab for scene object: {objectName}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = position;
        instance.transform.localRotation = rotation;
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

    private static Material GetOrCreateMarkerMaterial(string markerName, Color color)
    {
        EnsureFolder(MarkerMaterialFolder);

        string materialName = $"MAT_{markerName}";
        string materialPath = $"{MarkerMaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
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
            name = materialName,
            color = color
        };

        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }
}

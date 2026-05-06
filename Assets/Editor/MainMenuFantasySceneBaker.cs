using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuFantasySceneBaker
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BakedVersionKey = "Soulstone.MainMenuFantasySceneBaker.BakedVersion";
    private const int BakedVersion = 2;
    private const string MaterialFolder = "Assets/Materials/MainMenu";
    private const string MenuPanelSpritePath = "Assets/Art/UI/ornate_fantasy_ui_panel_transparent.png";
    private const string ButtonSpritePath = "Assets/Art/UI/button_no_text_transparent.png";

    private static Material darkStone;
    private static Material mediumStone;
    private static Material wood;
    private static Material darkBlue;
    private static Material buttonBlue;
    private static Material gold;
    private static Material magicOrange;
    private static Material fireYellow;
    private static Material potionBlue;
    private static Material potionGreen;
    private static Material potionRed;
    private static Material darkCloth;
    private static Material creamCloth;

    [MenuItem("Soulstone/Bake Fantasy Main Menu Scene")]
    public static void BakeOpenMainMenu()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.path != MainMenuScenePath)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath);
        }

        EnsureMaterials();
        GameObject worldRoot = EnsureRoot("MainMenu_3DRoom");
        ClearChildren(worldRoot.transform);

        Transform environment = CreateGroup(worldRoot.transform, "Environment");
        Transform characterPreview = CreateGroup(worldRoot.transform, "CharacterPreview");
        Transform lighting = CreateGroup(worldRoot.transform, "Lighting");

        BuildEnvironment(environment);
        BuildCharacterPreview(characterPreview, lighting);
        ConfigureLighting(lighting);
        ConfigureCamera();
        ConfigureGlobalVolume();

        MainMenuManager manager = Object.FindFirstObjectByType<MainMenuManager>();

        if (manager != null)
        {
            manager.RebuildSceneMenuLayout();
        }

        ConfigureCanvasUi();

        if (manager != null)
        {
            manager.RefreshSelectedHeroPreviewInScene();
        }

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorPrefs.SetInt(BakedVersionKey, BakedVersion);
        Debug.Log("Fantasy main menu baked: dungeon environment, lighting, hero preview, magic circle and UI styling are saved in Hierarchy.");
    }

    private static void BuildEnvironment(Transform parent)
    {
        Transform floor = CreateGroup(parent, "Floor");
        CreateCube(floor, "FloorBase", new Vector3(0f, -0.1f, 0f), Vector3.zero, new Vector3(12f, 0.2f, 8f), darkStone);
        CreateFloorTiles(floor);

        Transform walls = CreateGroup(parent, "Walls");
        CreateCube(walls, "BackWall", new Vector3(0f, 2.5f, 3.5f), Vector3.zero, new Vector3(12f, 5f, 0.3f), darkStone);
        CreateCube(walls, "LeftWall", new Vector3(-6f, 2.5f, 0f), Vector3.zero, new Vector3(0.3f, 5f, 8f), darkStone);
        CreateCube(walls, "RightWall", new Vector3(6f, 2.5f, 0f), Vector3.zero, new Vector3(0.3f, 5f, 8f), darkStone);
        CreateWallBlocks(walls);

        Transform pillars = CreateGroup(parent, "Pillars");
        CreateCube(pillars, "Pillar_Left", new Vector3(-3.6f, 1.45f, 3.28f), Vector3.zero, new Vector3(0.45f, 2.9f, 0.45f), mediumStone);
        CreateCube(pillars, "Pillar_Right", new Vector3(3.55f, 1.45f, 3.28f), Vector3.zero, new Vector3(0.45f, 2.9f, 0.45f), mediumStone);
        CreateCube(pillars, "SideBeam_Left", new Vector3(-5.82f, 3.0f, 0.1f), Vector3.zero, new Vector3(0.18f, 0.42f, 7.2f), mediumStone);
        CreateCube(pillars, "SideBeam_Right", new Vector3(5.82f, 3.0f, 0.1f), Vector3.zero, new Vector3(0.18f, 0.42f, 7.2f), mediumStone);

        Transform arch = CreateGroup(parent, "Arch");
        CreateArch(arch, new Vector3(-3.95f, 1.3f, 3.2f));

        Transform banners = CreateGroup(parent, "Banners");
        CreateBanner(banners, "Banner_Center", new Vector3(0.9f, 2.05f, 3.28f), 0f);
        CreateBanner(banners, "Banner_Right", new Vector3(4.15f, 1.95f, 3.28f), 0f);

        Transform torches = CreateGroup(parent, "Torches");
        CreateTorch(torches, "Torch_Left", new Vector3(-5.15f, 1.35f, 2.55f), Quaternion.Euler(0f, 0f, -24f));
        CreateTorch(torches, "Torch_Right", new Vector3(3.55f, 1.35f, 2.8f), Quaternion.Euler(0f, 0f, 24f));

        Transform table = CreateGroup(parent, "Table");
        CreateTable(table);

        Transform potions = CreateGroup(parent, "Potions");
        CreatePotion(potions, "Potion_Blue", new Vector3(4.02f, 0.9f, 1.62f), potionBlue);
        CreatePotion(potions, "Potion_Green", new Vector3(4.42f, 0.9f, 1.72f), potionGreen);
        CreatePotion(potions, "Potion_Red", new Vector3(4.72f, 0.9f, 1.44f), potionRed);
    }

    private static void BuildCharacterPreview(Transform parent, Transform lighting)
    {
        Transform heroRoot = CreateGroup(parent, "SelectedHeroPreview");
        heroRoot.localPosition = new Vector3(2.5f, 0.03f, 0f);
        heroRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

        MenuCharacterIdle idle = heroRoot.GetComponent<MenuCharacterIdle>() ?? heroRoot.gameObject.AddComponent<MenuCharacterIdle>();
        EditorUtility.SetDirty(idle);

        BuildPlaceholderHero(heroRoot);
        CreateMagicCircle(parent);

        GameObject lightObject = new("MagicCircleLight", typeof(Light));
        lightObject.transform.SetParent(lighting, false);
        lightObject.transform.position = new Vector3(2.5f, 0.32f, 0f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.35f, 0f, 1f);
        light.intensity = 4.2f;
        light.range = 4.5f;
        light.shadows = LightShadows.Soft;
    }

    private static void BuildPlaceholderHero(Transform parent)
    {
        Transform model = CreateGroup(parent, "PlayerModel");
        CreateCapsule(model, "Body_Robe", new Vector3(0f, 0.88f, 0f), Vector3.zero, new Vector3(0.62f, 1.0f, 0.62f), darkCloth);
        CreateSphere(model, "Hood", new Vector3(0f, 1.72f, 0f), Vector3.zero, new Vector3(0.58f, 0.5f, 0.58f), creamCloth);
        CreateSphere(model, "Face_Shadow", new Vector3(0f, 1.66f, -0.08f), Vector3.zero, new Vector3(0.36f, 0.28f, 0.08f), darkStone);
        CreateCube(model, "Belt", new Vector3(0f, 0.95f, -0.02f), Vector3.zero, new Vector3(0.74f, 0.1f, 0.08f), gold);
        CreateCube(model, "Shoulder_Left", new Vector3(-0.43f, 1.25f, 0f), new Vector3(0f, 0f, 20f), new Vector3(0.28f, 0.16f, 0.22f), gold);
        CreateCube(model, "Shoulder_Right", new Vector3(0.43f, 1.25f, 0f), new Vector3(0f, 0f, -20f), new Vector3(0.28f, 0.16f, 0.22f), gold);
        CreateCube(model, "BackWeapon_Handle", new Vector3(0.34f, 1.05f, 0.22f), new Vector3(0f, 0f, -28f), new Vector3(0.07f, 1.35f, 0.07f), wood);
        CreateCube(model, "BackWeapon_Blade", new Vector3(0.62f, 1.62f, 0.23f), new Vector3(0f, 0f, -28f), new Vector3(0.18f, 0.44f, 0.045f), gold);
    }

    private static void CreateMagicCircle(Transform parent)
    {
        Transform circleRoot = CreateGroup(parent, "MagicCircle");
        circleRoot.position = new Vector3(2.5f, 0.035f, 0f);

        RotateObject rotate = circleRoot.GetComponent<RotateObject>() ?? circleRoot.gameObject.AddComponent<RotateObject>();
        EditorUtility.SetDirty(rotate);

        CreateCylinder(circleRoot, "MagicCircle_GlowDisk", Vector3.zero, Vector3.zero, new Vector3(1.45f, 0.012f, 1.45f), magicOrange);
        CreateCylinder(circleRoot, "MagicCircle_OuterRing", new Vector3(0f, 0.012f, 0f), Vector3.zero, new Vector3(1.22f, 0.014f, 1.22f), magicOrange);
        CreateCylinder(circleRoot, "MagicCircle_InnerRing", new Vector3(0f, 0.024f, 0f), Vector3.zero, new Vector3(0.78f, 0.014f, 0.78f), magicOrange);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            CreateCube(circleRoot, $"MagicRune_{i + 1}", Vector3.zero, new Vector3(0f, angle, 0f), new Vector3(0.05f, 0.018f, 1.25f), magicOrange);
        }

        CreateEmbers(circleRoot, "MagicCircle_Embers", new Vector3(0f, 0.08f, 0f), 18, new Color(1f, 0.34f, 0f, 1f));
    }

    private static void ConfigureLighting(Transform lighting)
    {
        Light directional = Object.FindFirstObjectByType<Light>();

        if (directional == null || directional.type != LightType.Directional)
        {
            GameObject lightObject = new("Directional Light", typeof(Light));
            directional = lightObject.GetComponent<Light>();
        }

        directional.name = "Directional Light";
        directional.transform.SetParent(lighting, false);
        directional.transform.rotation = Quaternion.Euler(44f, -30f, 0f);
        directional.type = LightType.Directional;
        directional.color = new Color(0.96f, 0.78f, 0.58f, 1f);
        directional.intensity = 0.42f;
        directional.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.035f, 0.04f, 0.06f, 1f);
    }

    private static void ConfigureCamera()
    {
        Camera camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();

        if (camera == null)
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            camera = cameraObject.GetComponent<Camera>();
        }

        camera.name = "Main Camera";
        camera.transform.position = new Vector3(0.5f, 2.2f, -7f);
        camera.transform.rotation = Quaternion.Euler(10f, -5f, 0f);
        camera.fieldOfView = 48f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.018f, 0.026f, 1f);
        camera.allowHDR = true;
    }

    private static void ConfigureCanvasUi()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform root = canvas.transform.Find("MainMenu_Backdrop");

        if (root == null)
        {
            return;
        }

        RemoveObsoleteTopText(root);
        Transform leftMenu = ConsolidateMenuPanel(root);

        if (leftMenu != null)
        {
            leftMenu.name = "MenuPanel";
            leftMenu.SetAsFirstSibling();
            RectTransform panelRect = leftMenu.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0.5f);
            panelRect.anchorMax = new Vector2(0f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(260f, 0f);
            panelRect.sizeDelta = new Vector2(500f, 850f);

            Image panelImage = leftMenu.GetComponent<Image>() ?? leftMenu.gameObject.AddComponent<Image>();
            Sprite panelSprite = LoadMenuPanelSprite();

            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Simple;
                panelImage.preserveAspect = false;
                panelImage.color = Color.white;
            }
            else
            {
                panelImage.color = Hex("#071425", 0.86f);
                Outline outline = leftMenu.GetComponent<Outline>() ?? leftMenu.gameObject.AddComponent<Outline>();
                outline.effectColor = Hex("#B9822B");
                outline.effectDistance = new Vector2(4f, -4f);
            }

            Outline existingOutline = leftMenu.GetComponent<Outline>();

            if (panelSprite != null && existingOutline != null)
            {
                Object.DestroyImmediate(existingOutline);
            }

            StyleMenuPanel(leftMenu);
            ApplyButtonSpriteToAllButtons(root);
        }
    }

    private static void RemoveObsoleteTopText(Transform root)
    {
        Transform obsolete = root.Find("Header");

        if (obsolete != null)
        {
            Object.DestroyImmediate(obsolete.gameObject);
        }
    }

    private static Transform ConsolidateMenuPanel(Transform root)
    {
        Transform preferred = root.Find("MenuPanel") ?? root.Find("LeftMenu");

        if (preferred == null)
        {
            return null;
        }

        preferred.name = "MenuPanel";

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);

            if (child == preferred)
            {
                continue;
            }

            if (child.name == "LeftMenu" || child.name == "MenuPanel")
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        return preferred;
    }

    private static Sprite LoadMenuPanelSprite()
    {
        return LoadSpriteAsset(MenuPanelSpritePath);
    }

    private static Sprite LoadButtonSprite()
    {
        return LoadSpriteAsset(ButtonSpritePath);
    }

    private static Sprite LoadSpriteAsset(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null)
        {
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static void ApplyButtonSpriteToAllButtons(Transform root)
    {
        Sprite buttonSprite = LoadButtonSprite();

        if (buttonSprite == null)
        {
            return;
        }

        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            ApplyButtonSprite(buttons[i], buttonSprite);
        }
    }

    private static void ApplyButtonSprite(Button button, Sprite buttonSprite)
    {
        if (button == null || buttonSprite == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>() ?? button.gameObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.color = Color.white;
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.92f, 0.72f, 1f);
        colors.pressedColor = new Color(0.82f, 0.72f, 0.55f, 1f);
        colors.selectedColor = new Color(1f, 0.88f, 0.62f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;

        Outline outline = button.GetComponent<Outline>();

        if (outline != null)
        {
            Object.DestroyImmediate(outline);
        }
    }

    private static void StyleMenuPanel(Transform panel)
    {
        Text logo = FindChild<Text>(panel, "Logo");

        if (logo != null)
        {
            logo.name = "TitleText";
            logo.text = "SOULSTONE\nPROTOTYPE";
            logo.fontSize = 58;
            logo.alignment = TextAnchor.MiddleCenter;
            logo.color = Hex("#F4D58D");
            Shadow shadow = logo.GetComponent<Shadow>() ?? logo.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(3f, -3f);
        }

        Transform buttonsContainer = panel.Find("MenuButtons");

        if (buttonsContainer != null)
        {
            buttonsContainer.name = "ButtonsContainer";
            VerticalLayoutGroup layout = buttonsContainer.GetComponent<VerticalLayoutGroup>();

            if (layout != null)
            {
                layout.spacing = 22f;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            StyleButton(buttonsContainer, "PlayMenuButton", "PlayButton", "Играть");
            StyleButton(buttonsContainer, "CharacterMenuButton", "CharacterButton", "Персонаж");
            StyleButton(buttonsContainer, "WeaponMenuButton", "WeaponButton", "Оружие");
            StyleButton(buttonsContainer, "SkillTreeMenuButton", "SkillTreeButton", "Дерево навыков");
            StyleButton(buttonsContainer, "SettingsMenuButton", "SettingsButton", "Настройки");
            StyleButton(buttonsContainer, "ExitMenuButton", "ExitButton", "Выход");
        }
    }

    private static void StyleButton(Transform parent, string oldName, string newName, string text)
    {
        Transform buttonTransform = parent.Find(oldName) ?? parent.Find(newName);

        if (buttonTransform == null)
        {
            return;
        }

        buttonTransform.name = newName;
        RectTransform rect = buttonTransform.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(390f, 75f);

        LayoutElement layout = buttonTransform.GetComponent<LayoutElement>() ?? buttonTransform.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 390f;
        layout.preferredHeight = 75f;

        Image image = buttonTransform.GetComponent<Image>() ?? buttonTransform.gameObject.AddComponent<Image>();
        Sprite buttonSprite = LoadButtonSprite();

        if (buttonSprite != null)
        {
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = Color.white;
        }
        else
        {
            image.color = Hex("#111E34");
        }

        Button button = buttonTransform.GetComponent<Button>();

        if (buttonSprite != null)
        {
            ApplyButtonSprite(button, buttonSprite);
        }
        else
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Hex("#111E34");
            colors.highlightedColor = Hex("#1F3458");
            colors.pressedColor = Hex("#0B1220");
            colors.selectedColor = Hex("#1F3458");
            button.colors = colors;

            Outline outline = buttonTransform.GetComponent<Outline>() ?? buttonTransform.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#B9822B");
            outline.effectDistance = new Vector2(2f, -2f);
        }

        Text label = FindChild<Text>(buttonTransform, "Label") ?? buttonTransform.GetComponentInChildren<Text>(true);

        if (label != null)
        {
            label.text = text;
            label.fontSize = 26;
            label.color = Hex("#F3DFA3");
            label.alignment = TextAnchor.MiddleCenter;
        }
    }

    private static void ConfigureGlobalVolume()
    {
        GameObject volumeObject = GameObject.Find("Global Volume");

        if (volumeObject == null)
        {
            volumeObject = new GameObject("Global Volume", typeof(Volume));
        }

        Volume volume = volumeObject.GetComponent<Volume>() ?? volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;

        VolumeProfile profile = volume.profile;

        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "MainMenu_GlobalVolumeProfile";
            string path = $"{MaterialFolder}/MainMenu_GlobalVolumeProfile.asset";
            AssetDatabase.CreateAsset(profile, AssetDatabase.GenerateUniqueAssetPath(path));
            volume.profile = profile;
        }

        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>();
        }

        bloom.active = true;
        bloom.intensity.Override(0.85f);
        bloom.threshold.Override(0.92f);

        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>();
        }

        vignette.active = true;
        vignette.intensity.Override(0.32f);
        vignette.smoothness.Override(0.52f);

        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>();
        }

        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(-0.2f);
        colorAdjustments.contrast.Override(22f);
        colorAdjustments.saturation.Override(8f);
    }

    private static void CreateFloorTiles(Transform parent)
    {
        for (int x = -5; x <= 5; x++)
        {
            for (int z = -3; z <= 4; z++)
            {
                float shade = ((x + z) & 1) == 0 ? 1f : 0.82f;
                Material material = shade > 0.9f ? mediumStone : darkStone;
                CreateCube(parent, $"FloorTile_{x}_{z}", new Vector3(x * 0.92f, 0.025f, z * 0.82f + 0.4f), Vector3.zero, new Vector3(0.82f, 0.035f, 0.72f), material);
            }
        }
    }

    private static void CreateWallBlocks(Transform parent)
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = -6; x <= 6; x++)
            {
                float offset = y % 2 == 0 ? 0f : 0.42f;
                Material material = ((x + y) & 1) == 0 ? mediumStone : darkStone;
                CreateCube(parent, $"BackWallStone_{x}_{y}", new Vector3(x * 0.86f + offset, y * 0.46f + 0.38f, 3.31f), Vector3.zero, new Vector3(0.78f, 0.34f, 0.08f), material);
            }
        }
    }

    private static void CreateArch(Transform parent, Vector3 center)
    {
        CreateCube(parent, "DarkPassage", center + new Vector3(0f, -0.35f, -0.05f), Vector3.zero, new Vector3(1.32f, 1.62f, 0.08f), darkCloth);
        CreateCube(parent, "ArchColumn_Left", center + new Vector3(-0.78f, -0.28f, 0f), Vector3.zero, new Vector3(0.25f, 1.76f, 0.18f), mediumStone);
        CreateCube(parent, "ArchColumn_Right", center + new Vector3(0.78f, -0.28f, 0f), Vector3.zero, new Vector3(0.25f, 1.76f, 0.18f), mediumStone);

        for (int i = 0; i < 7; i++)
        {
            float angle = Mathf.Lerp(205f, 335f, i / 6f);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = center + new Vector3(Mathf.Cos(radians) * 0.84f, Mathf.Sin(radians) * 0.62f + 0.2f, 0f);
            CreateCube(parent, $"ArchStone_{i + 1}", position, new Vector3(0f, 0f, angle + 90f), new Vector3(0.32f, 0.2f, 0.16f), mediumStone);
        }
    }

    private static void CreateBanner(Transform parent, string name, Vector3 position, float yRotation)
    {
        Transform root = CreateGroup(parent, name);
        root.position = position;
        root.rotation = Quaternion.Euler(0f, yRotation, 0f);
        CreateCube(root, "Rod", new Vector3(0f, 0.82f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.045f, 0.68f, 0.045f), gold);
        CreateCube(root, "Cloth", Vector3.zero, Vector3.zero, new Vector3(0.78f, 1.38f, 0.035f), darkBlue);
        CreateCube(root, "BottomPoint", new Vector3(0f, -0.66f, 0f), new Vector3(0f, 0f, 45f), new Vector3(0.38f, 0.38f, 0.032f), darkBlue);
        CreateCube(root, "WeaponMark_Left", new Vector3(-0.12f, 0.12f, -0.05f), new Vector3(0f, 0f, 42f), new Vector3(0.08f, 0.58f, 0.028f), gold);
        CreateCube(root, "WeaponMark_Right", new Vector3(0.12f, 0.12f, -0.055f), new Vector3(0f, 0f, -42f), new Vector3(0.08f, 0.58f, 0.028f), gold);
        CreateCube(root, "WeaponMark_Tip", new Vector3(0f, 0.43f, -0.06f), Vector3.zero, new Vector3(0.12f, 0.32f, 0.028f), gold);
    }

    private static void CreateTorch(Transform parent, string name, Vector3 position, Quaternion rotation)
    {
        Transform root = CreateGroup(parent, name);
        root.position = position;
        root.rotation = rotation;
        CreateCylinder(root, "Holder", new Vector3(0f, -0.14f, 0f), new Vector3(0f, 0f, 24f), new Vector3(0.08f, 0.48f, 0.08f), wood);
        CreateCylinder(root, "Cup", new Vector3(0f, 0.14f, -0.02f), Vector3.zero, new Vector3(0.18f, 0.16f, 0.18f), gold);
        CreateSphere(root, "Flame_Core", new Vector3(0f, 0.42f, -0.04f), Vector3.zero, new Vector3(0.2f, 0.35f, 0.2f), fireYellow);
        CreateSphere(root, "Flame_Outer", new Vector3(0f, 0.36f, -0.05f), Vector3.zero, new Vector3(0.34f, 0.52f, 0.34f), magicOrange);

        GameObject lightObject = new("Point Light", typeof(Light), typeof(TorchFlicker));
        lightObject.transform.SetParent(root, false);
        lightObject.transform.localPosition = new Vector3(0f, 0.42f, -0.55f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.48f, 0.16f, 1f);
        light.intensity = 3.2f;
        light.range = 5f;
        light.shadows = LightShadows.Soft;
        CreateEmbers(root, "Embers", new Vector3(0f, 0.35f, -0.05f), 12, new Color(1f, 0.5f, 0.12f, 1f));
    }

    private static void CreateTable(Transform parent)
    {
        CreateCube(parent, "TableTop", new Vector3(4.3f, 0.68f, 1.6f), new Vector3(0f, -8f, 0f), new Vector3(1.55f, 0.14f, 0.62f), wood);
        CreateCube(parent, "Leg_01", new Vector3(3.72f, 0.34f, 1.35f), Vector3.zero, new Vector3(0.12f, 0.68f, 0.12f), wood);
        CreateCube(parent, "Leg_02", new Vector3(4.88f, 0.34f, 1.35f), Vector3.zero, new Vector3(0.12f, 0.68f, 0.12f), wood);
        CreateCube(parent, "Leg_03", new Vector3(3.72f, 0.34f, 1.85f), Vector3.zero, new Vector3(0.12f, 0.68f, 0.12f), wood);
        CreateCube(parent, "Leg_04", new Vector3(4.88f, 0.34f, 1.85f), Vector3.zero, new Vector3(0.12f, 0.68f, 0.12f), wood);
        CreateCube(parent, "Spellbook", new Vector3(4.58f, 0.81f, 1.95f), new Vector3(0f, -18f, 0f), new Vector3(0.48f, 0.07f, 0.34f), darkBlue);
    }

    private static void CreatePotion(Transform parent, string name, Vector3 position, Material material)
    {
        Transform root = CreateGroup(parent, name);
        root.position = position;
        CreateSphere(root, "BottleBody", Vector3.zero, Vector3.zero, new Vector3(0.2f, 0.25f, 0.2f), material);
        CreateCylinder(root, "BottleNeck", new Vector3(0f, 0.23f, 0f), Vector3.zero, new Vector3(0.06f, 0.18f, 0.06f), material);
        CreateCylinder(root, "Cork", new Vector3(0f, 0.43f, 0f), Vector3.zero, new Vector3(0.07f, 0.08f, 0.07f), wood);
    }

    private static void CreateEmbers(Transform parent, string name, Vector3 position, int maxParticles, Color color)
    {
        GameObject particles = new(name, typeof(ParticleSystem));
        particles.transform.SetParent(parent, false);
        particles.transform.localPosition = position;
        ParticleSystem system = particles.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = system.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 0.18f;
        main.startSize = 0.035f;
        main.startColor = color;
        main.maxParticles = maxParticles;
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = Mathf.Max(4, maxParticles / 3f);
        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.45f;
    }

    private static void EnsureMaterials()
    {
        Directory.CreateDirectory(MaterialFolder);
        darkStone = EnsureMaterial("MM_DarkStone", Hex("#1E1C1B"));
        mediumStone = EnsureMaterial("MM_MediumStone", Hex("#302923"));
        wood = EnsureMaterial("MM_Wood", Hex("#4A2B16"));
        darkBlue = EnsureMaterial("MM_DarkBlueBanner", Hex("#0D1A2E"));
        buttonBlue = EnsureMaterial("MM_ButtonBlue", Hex("#111E34"));
        gold = EnsureMaterial("MM_GoldBronze", Hex("#B9822B"));
        magicOrange = EnsureMaterial("MM_MagicOrange_Emission", Hex("#FF5A00"), true, 3f);
        fireYellow = EnsureMaterial("MM_FireYellow_Emission", Hex("#FFD45A"), true, 2.5f);
        potionBlue = EnsureMaterial("MM_PotionBlue_Emission", new Color(0.05f, 0.25f, 0.75f, 1f), true, 1.4f);
        potionGreen = EnsureMaterial("MM_PotionGreen_Emission", new Color(0.08f, 0.55f, 0.25f, 1f), true, 1.4f);
        potionRed = EnsureMaterial("MM_PotionRed_Emission", new Color(0.75f, 0.06f, 0.04f, 1f), true, 1.4f);
        darkCloth = EnsureMaterial("MM_DarkCloth", new Color(0.16f, 0.07f, 0.04f, 1f));
        creamCloth = EnsureMaterial("MM_CreamHood", new Color(0.86f, 0.72f, 0.52f, 1f));
        AssetDatabase.SaveAssets();
    }

    private static Material EnsureMaterial(string name, Color color, bool emission = false, float emissionIntensity = 1f)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") != null ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.SetColor("_BaseColor", color);

        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emissionIntensity);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject EnsureRoot(string name)
    {
        GameObject root = GameObject.Find(name);

        if (root == null)
        {
            root = new GameObject(name);
        }

        return root;
    }

    private static Transform CreateGroup(Transform parent, string name)
    {
        GameObject group = new(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        return CreatePrimitive(parent, name, PrimitiveType.Cube, position, rotation, scale, material);
    }

    private static GameObject CreateSphere(Transform parent, string name, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        return CreatePrimitive(parent, name, PrimitiveType.Sphere, position, rotation, scale, material);
    }

    private static GameObject CreateCapsule(Transform parent, string name, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        return CreatePrimitive(parent, name, PrimitiveType.Capsule, position, rotation, scale, material);
    }

    private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        return CreatePrimitive(parent, name, PrimitiveType.Cylinder, position, rotation, scale, material);
    }

    private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = position;
        primitive.transform.localRotation = Quaternion.Euler(rotation);
        primitive.transform.localScale = scale;

        Renderer renderer = primitive.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        return primitive;
    }

    private static T FindChild<T>(Transform parent, string childName) where T : Component
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
            {
                return child.GetComponent<T>();
            }

            T nested = FindChild<T>(child, childName);

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static Color Hex(string hex, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        color.a = alpha;
        return color;
    }
}

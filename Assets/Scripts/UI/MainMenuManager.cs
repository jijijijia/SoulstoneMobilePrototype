using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuManager : MonoBehaviour
{
    private enum MenuScreen
    {
        Home,
        Play,
        Character,
        Weapon,
        SkillTree,
        Settings
    }

    private struct MapEntry
    {
        public string Id;
        public string Name;
        public string Difficulty;
        public string Biome;
        public string Enemies;
        public string Rewards;

        public MapEntry(string id, string name, string difficulty, string biome, string enemies, string rewards)
        {
            Id = id;
            Name = name;
            Difficulty = difficulty;
            Biome = biome;
            Enemies = enemies;
            Rewards = rewards;
        }
    }

    private const string MasterVolumeKey = "settings.master_volume";
    private const string MusicVolumeKey = "settings.music_volume";
    private const string SfxVolumeKey = "settings.sfx_volume";
    private const string QualityKey = "settings.quality";
    private const string FullscreenKey = "settings.fullscreen";
    private const string LanguageKey = "settings.language";
    private const string GeneratedRootName = "MainMenu_Backdrop";
    private const string WorldRootName = "MainMenu_3DRoom";
    private const string HeroWorldRootName = "SelectedHeroPreview";

    [SerializeField] private string gameplaySceneName = "main";
    [SerializeField] private bool buildUiOnStart = true;
    [SerializeField] private CharacterRoster characterRoster;
    [Header("Card Prefabs")]
    [SerializeField] private MapCardView mapCardPrefab;
    [SerializeField] private CharacterCardView characterCardPrefab;
    [SerializeField] private WeaponCardView weaponCardPrefab;
    [SerializeField] private Color backgroundColor = new(0.055f, 0.06f, 0.075f, 1f);
    [SerializeField] private Color panelColor = new(0.11f, 0.12f, 0.145f, 0.94f);
    [SerializeField] private Color cardColor = new(0.17f, 0.18f, 0.21f, 0.96f);
    [SerializeField] private Color accentColor = new(0.86f, 0.68f, 0.32f, 1f);
    [SerializeField] private Color mutedColor = new(0.44f, 0.47f, 0.53f, 1f);

    private readonly List<CharacterData> characters = new();
    private readonly List<Button> characterButtons = new();
    private readonly List<Button> weaponButtons = new();
    private readonly List<Button> mapButtons = new();
    private readonly List<CharacterCardView> characterCardViews = new();
    private readonly List<WeaponCardView> weaponCardViews = new();
    private readonly List<MapCardView> mapCardViews = new();
    private readonly List<MapEntry> maps = new()
    {
        new("forgotten_plains", "Забытые равнины", "Низкая", "Равнины", "Стаи слабых врагов, быстрые преследователи", "Осколки души, базовое оружие"),
        new("ashen_quarry", "Пепельный карьер", "Средняя", "Камень и пепел", "Танки, огненные враги, мини-боссы", "Камни улучшений, редкие пассивки"),
        new("venom_marsh", "Ядовитые топи", "Средняя", "Болото", "Дальние ядовики, замедляющие враги", "Ядовитые навыки, ресурсы статусов"),
        new("storm_spire", "Грозовой шпиль", "Высокая", "Башня шторма", "Быстрые элиты, молниевые кастеры", "Редкие навыки молнии"),
        new("bone_citadel", "Костяная цитадель", "Очень высокая", "Некрополь", "Толпы, элиты, усиленные боссы", "Эпические награды")
    };

    private Canvas canvas;
    private Font defaultFont;
    private Transform contentRoot;
    private Transform leftMenuRoot;
    private GameObject mapDetailsPanel;
    private Text titleText;
    private Text subtitleText;
    private Text heroPreviewText;
    private Text characterPreviewTitleText;
    private Text characterPreviewBodyText;
    private Button characterSelectButton;
    private Text mapDetailsText;
    private Button battleButton;
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Transform menuWorldRoot;
    private Transform heroWorldRoot;

    private CharacterData selectedCharacter;
    private CharacterData previewedCharacter;
    private WeaponData selectedWeapon;
    private string selectedMapId;
    private MenuScreen currentScreen;
    private MapEntry pendingMap;

    private void Start()
    {
        if (!buildUiOnStart)
        {
            return;
        }

        InitializeMenu(clearExistingGeneratedUi: false, forceRebuildGeneratedUi: false);
    }

    [ContextMenu("Rebuild Menu Preview")]
    public void RebuildSceneMenuLayout()
    {
        InitializeMenu(clearExistingGeneratedUi: true, forceRebuildGeneratedUi: true);
        MarkSceneDirtyInEditor();
    }

    [ContextMenu("Refresh Selected Hero Preview")]
    public void RefreshSelectedHeroPreviewInScene()
    {
        defaultFont = defaultFont != null ? defaultFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        characterRoster = characterRoster != null ? characterRoster : SelectedCharacterStore.LoadRoster();
        selectedCharacter = SelectedCharacterStore.ResolveSelectedCharacter(characterRoster, selectedCharacter);

        GameObject existing = GameObject.Find(WorldRootName);

        if (existing == null)
        {
            EnsureWorldBackdrop();
            MarkSceneDirtyInEditor();
            return;
        }

        menuWorldRoot = existing.transform;
        heroWorldRoot = menuWorldRoot.Find(HeroWorldRootName);

        if (heroWorldRoot == null)
        {
            GameObject heroRootObject = new(HeroWorldRootName);
            heroRootObject.transform.SetParent(menuWorldRoot, false);
            heroWorldRoot = heroRootObject.transform;
        }

        heroWorldRoot.position = new Vector3(2.65f, 0.03f, 0.55f);
        heroWorldRoot.rotation = Quaternion.Euler(0f, -18f, 0f);
        RefreshWorldHero();
        MarkSceneDirtyInEditor();
    }

    [ContextMenu("Clear Menu Preview")]
    public void ClearSceneMenuLayout()
    {
        EnsureCanvas();
        ClearGeneratedMenu();
        MarkSceneDirtyInEditor();
    }

    private void InitializeMenu(bool clearExistingGeneratedUi, bool forceRebuildGeneratedUi)
    {
        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        characterRoster = characterRoster != null ? characterRoster : SelectedCharacterStore.LoadRoster();
        LoadMapEntries();
        selectedCharacter = SelectedCharacterStore.ResolveSelectedCharacter(characterRoster, null);
        selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(selectedCharacter);
        selectedMapId = SelectedLoadoutStore.GetSelectedMapId();

        CacheCharacters();
        EnsureEventSystem();
        EnsureCanvas();

        bool canReuseSceneLayout = !forceRebuildGeneratedUi && TryBindSceneLayout();

        if (!canReuseSceneLayout)
        {
            if (Application.isPlaying && !forceRebuildGeneratedUi)
            {
                Debug.LogError("MainMenu scene is missing baked UI layout. Open MainMenu scene and use MainMenuManager -> Rebuild Scene Menu Layout.", this);
                return;
            }

            if (clearExistingGeneratedUi || forceRebuildGeneratedUi)
            {
                ClearGeneratedMenu();
            }

            BuildRoot();
        }

        EnsureWorldBackdrop();
        BindMenuButtons();
        ShowScreen(MenuScreen.Home);
        LoadSettings();
    }

    public void PlayGame()
    {
        if (selectedCharacter != null)
        {
            SelectedCharacterStore.SetSelectedCharacter(selectedCharacter);
        }

        if (selectedCharacter != null && selectedWeapon != null)
        {
            SelectedLoadoutStore.SetSelectedWeapon(selectedCharacter, selectedWeapon);
        }

        if (!string.IsNullOrWhiteSpace(selectedMapId))
        {
            SelectedLoadoutStore.SetSelectedMap(selectedMapId);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void BuildRoot()
    {
        GameObject backdrop = CreatePanel(GeneratedRootName, canvas.transform, new Color(0f, 0f, 0f, 0f));
        Stretch(backdrop.GetComponent<RectTransform>());

        CreateHeroPreview(backdrop.transform);
        CreateLeftMenu(backdrop.transform);

        GameObject header = new("Header", typeof(RectTransform));
        header.transform.SetParent(backdrop.transform, false);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.offsetMin = new Vector2(360f, -140f);
        headerRect.offsetMax = new Vector2(-80f, -36f);

        titleText = CreateText("ScreenTitle", header.transform, string.Empty, 54, FontStyle.Bold);
        Stretch(titleText.rectTransform);
        titleText.alignment = TextAnchor.UpperLeft;

        subtitleText = CreateText("ScreenSubtitle", header.transform, string.Empty, 22, FontStyle.Normal);
        subtitleText.rectTransform.anchorMin = new Vector2(0f, 0f);
        subtitleText.rectTransform.anchorMax = new Vector2(1f, 0f);
        subtitleText.rectTransform.pivot = new Vector2(0f, 0f);
        subtitleText.rectTransform.offsetMin = new Vector2(4f, 0f);
        subtitleText.rectTransform.offsetMax = new Vector2(0f, 42f);
        subtitleText.color = new Color(0.82f, 0.84f, 0.88f, 1f);

        GameObject content = CreatePanel("Content", backdrop.transform, new Color(0f, 0f, 0f, 0f));
        contentRoot = content.transform;
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = new Vector2(340f, 80f);
        contentRect.offsetMax = new Vector2(-70f, -190f);
    }

    private void CreateHeroPreview(Transform parent)
    {
        GameObject preview = CreatePanel("HeroPreview", parent, new Color(0f, 0f, 0f, 0f));
        RectTransform rect = preview.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(-480f, 40f);
        rect.offsetMax = new Vector2(-50f, -860f);

        Text silhouette = CreateText("HeroSilhouette", preview.transform, string.Empty, 64, FontStyle.Bold);
        silhouette.rectTransform.anchorMin = new Vector2(0f, 0f);
        silhouette.rectTransform.anchorMax = new Vector2(1f, 1f);
        silhouette.rectTransform.offsetMin = new Vector2(30f, 80f);
        silhouette.rectTransform.offsetMax = new Vector2(-30f, -130f);
        silhouette.alignment = TextAnchor.MiddleCenter;
        silhouette.color = new Color(1f, 1f, 1f, 0f);

        heroPreviewText = CreateText("HeroPreviewText", preview.transform, string.Empty, 22, FontStyle.Bold);
        heroPreviewText.rectTransform.anchorMin = new Vector2(0f, 0f);
        heroPreviewText.rectTransform.anchorMax = new Vector2(1f, 0f);
        heroPreviewText.rectTransform.offsetMin = new Vector2(26f, 28f);
        heroPreviewText.rectTransform.offsetMax = new Vector2(-26f, 130f);
        heroPreviewText.alignment = TextAnchor.LowerLeft;
        heroPreviewText.color = new Color(1f, 1f, 1f, 0.78f);
    }

    private void CreateLeftMenu(Transform parent)
    {
        GameObject rail = CreatePanel("LeftMenu", parent, new Color(0.015f, 0.02f, 0.035f, 0.70f));
        RectTransform rect = rail.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(42f, 190f);
        rect.offsetMax = new Vector2(360f, -170f);

        Outline outline = rail.AddComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(4f, -4f);

        Text logo = CreateText("Logo", rail.transform, "SOULSTONE\nPROTOTYPE", 30, FontStyle.Bold);
        logo.rectTransform.anchorMin = new Vector2(0f, 1f);
        logo.rectTransform.anchorMax = new Vector2(1f, 1f);
        logo.rectTransform.offsetMin = new Vector2(26f, -118f);
        logo.rectTransform.offsetMax = new Vector2(-26f, -24f);
        logo.alignment = TextAnchor.MiddleCenter;
        logo.color = accentColor;

        GameObject menu = new("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        menu.transform.SetParent(rail.transform, false);
        leftMenuRoot = menu.transform;
        RectTransform menuRect = menu.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0f, 1f);
        menuRect.anchorMax = new Vector2(1f, 1f);
        menuRect.offsetMin = new Vector2(28f, -500f);
        menuRect.offsetMax = new Vector2(-28f, -145f);

        VerticalLayoutGroup layout = menu.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateMenuButton("PlayMenuButton", "Играть", () => ShowScreen(MenuScreen.Play));
        CreateMenuButton("CharacterMenuButton", "Персонаж", () => ShowScreen(MenuScreen.Character));
        CreateMenuButton("WeaponMenuButton", "Оружие", () => ShowScreen(MenuScreen.Weapon));
        CreateMenuButton("SkillTreeMenuButton", "Дерево навыков", () => ShowScreen(MenuScreen.SkillTree));
        CreateMenuButton("SettingsMenuButton", "Настройки", () => ShowScreen(MenuScreen.Settings));
        CreateMenuButton("ExitMenuButton", "Выход", QuitGame);
    }

    private void ShowScreen(MenuScreen screen)
    {
        currentScreen = screen;
        ClearContent();
        RefreshHeroPreview();

        switch (screen)
        {
            case MenuScreen.Play:
                SetHeader("Выбор карты", "Выбери арену, изучи условия и нажми В БОЙ.");
                BuildPlayScreen();
                break;
            case MenuScreen.Character:
                SetHeader("Персонаж", "Выбор героя сохраняется и применяется при старте боя.");
                BuildCharacterScreen();
                break;
            case MenuScreen.Weapon:
                SetHeader("Оружие", "Оружие задаёт стартовый активный навык и доступный пул навыков.");
                BuildWeaponScreen();
                break;
            case MenuScreen.SkillTree:
                SetHeader("Дерево навыков", "Мета-прогрессия между забегами. Пока это рабочий прототип экрана.");
                BuildSkillTreeScreen();
                break;
            case MenuScreen.Settings:
                SetHeader("Настройки", "Звук, графика, язык, экран и клавиши.");
                BuildSettingsScreen();
                LoadSettings();
                break;
            default:
                SetHeader("Главное меню", "Последний выбранный герой и оружие показаны справа.");
                BuildHomeScreen();
                break;
        }
    }

    private void BuildHomeScreen()
    {
        // The home screen intentionally leaves the 3D room unobstructed.
    }

    private void BuildPlayScreen()
    {
        GameObject row = CreateLayout("MapCarousel", contentRoot, true, 18f);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.offsetMin = new Vector2(0f, -250f);
        rowRect.offsetMax = new Vector2(0f, -20f);

        mapButtons.Clear();
        mapCardViews.Clear();

        for (int i = 0; i < maps.Count; i++)
        {
            MapEntry map = maps[i];
            Button button = CreateMapCard(row.transform, map);

            if (button != null)
            {
                mapButtons.Add(button);
            }
        }

        mapDetailsPanel = CreatePanel("MapDetails", contentRoot, panelColor);
        RectTransform detailsRect = mapDetailsPanel.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0f, 0f);
        detailsRect.anchorMax = new Vector2(1f, 0f);
        detailsRect.offsetMin = new Vector2(0f, 120f);
        detailsRect.offsetMax = new Vector2(0f, 360f);

        mapDetailsText = CreateText("MapDetailsText", mapDetailsPanel.transform, "Выбери карту в карусели выше.", 22, FontStyle.Normal);
        mapDetailsText.rectTransform.anchorMin = Vector2.zero;
        mapDetailsText.rectTransform.anchorMax = Vector2.one;
        mapDetailsText.rectTransform.offsetMin = new Vector2(28f, 28f);
        mapDetailsText.rectTransform.offsetMax = new Vector2(-280f, -24f);
        mapDetailsText.alignment = TextAnchor.UpperLeft;

        Button selectButton = CreateButton(mapDetailsPanel.transform, "ВЫБРАТЬ", () => SelectMap(pendingMap.Id), new Vector2(0f, 28f), new Vector2(220f, 54f));
        RectTransform selectRect = selectButton.GetComponent<RectTransform>();
        selectRect.anchorMin = new Vector2(1f, 0f);
        selectRect.anchorMax = new Vector2(1f, 0f);
        selectRect.pivot = new Vector2(1f, 0f);
        selectRect.anchoredPosition = new Vector2(-28f, 24f);

        battleButton = CreateButton(contentRoot, "В БОЙ", PlayGame, new Vector2(0f, 36f), new Vector2(260f, 64f));
        RectTransform battleRect = battleButton.GetComponent<RectTransform>();
        battleRect.anchorMin = new Vector2(0.5f, 0f);
        battleRect.anchorMax = new Vector2(0.5f, 0f);

        CreateButton(contentRoot, "НАЗАД", () => ShowScreen(MenuScreen.Home), new Vector2(-300f, 36f), new Vector2(220f, 56f));

        pendingMap = string.IsNullOrWhiteSpace(selectedMapId) ? maps[0] : FindMap(selectedMapId);

        if (!string.IsNullOrWhiteSpace(selectedMapId))
        {
            OpenMapDetails(pendingMap);
        }

        RefreshMapButtons();
    }

    private void BuildCharacterScreen()
    {
        previewedCharacter = selectedCharacter != null ? selectedCharacter : (characters.Count > 0 ? characters[0] : null);

        GameObject scrollView = CreatePanel("CharacterScrollView", contentRoot, new Color(0f, 0f, 0f, 0f));
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        Stretch(scrollRectTransform);
        scrollRectTransform.offsetMax = new Vector2(-430f, 0f);

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 34f;

        GameObject viewport = new("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        scrollRect.viewport = viewportRect;

        GameObject grid = new("CharacterGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        grid.transform.SetParent(viewport.transform, false);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.offsetMin = new Vector2(0f, 0f);
        gridRect.offsetMax = new Vector2(0f, 0f);

        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.cellSize = new Vector2(220f, 230f);
        layout.spacing = new Vector2(18f, 18f);
        layout.padding = new RectOffset(0, 12, 0, 24);
        layout.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = gridRect;

        characterButtons.Clear();
        characterCardViews.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            string stats = BuildCharacterStats(character);
            Button button = CreateCharacterCard(grid.transform, character, stats);

            if (button != null)
            {
                characterButtons.Add(button);
            }
        }

        CreateCharacterPreviewPanel();
        RefreshCharacterButtons();
        RefreshCharacterPreviewPanel();
    }

    private void BuildWeaponScreen()
    {
        if (selectedCharacter == null)
        {
            CreateInfoCard("Нет персонажа", "Сначала выбери персонажа.", Vector2.zero, new Vector2(520f, 220f));
            return;
        }

        GameObject grid = CreateLayout("WeaponGrid", contentRoot, true, 18f);
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        Stretch(gridRect);
        gridRect.offsetMax = new Vector2(-430f, 0f);

        weaponButtons.Clear();
        weaponCardViews.Clear();
        WeaponData[] weapons = selectedCharacter.AvailableWeapons;

        if (weapons != null)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponData weapon = weapons[i];

                if (weapon == null)
                {
                    continue;
                }

                Button button = CreateWeaponCard(grid.transform, weapon, BuildWeaponDescription(weapon));

                if (button != null)
                {
                    weaponButtons.Add(button);
                }
            }
        }

        CreateInfoCard("Оружие героя", "Выбранное оружие привязывается к текущему персонажу.\n\nОно определяет стартовый активный навык и навыки, которые смогут выпадать на уровне.", Vector2.zero, new Vector2(390f, 330f), rightPinned: true);
        RefreshWeaponButtons();
    }

    private void BuildSkillTreeScreen()
    {
        GameObject tree = CreateLayout("SkillTree", contentRoot, true, 22f);
        RectTransform treeRect = tree.GetComponent<RectTransform>();
        treeRect.anchorMin = new Vector2(0f, 0.5f);
        treeRect.anchorMax = new Vector2(1f, 0.5f);
        treeRect.offsetMin = new Vector2(0f, -110f);
        treeRect.offsetMax = new Vector2(0f, 110f);

        CreateTreeNode(tree.transform, "Уровень 1\n+HP\nСтоимость: 50");
        CreateTreeNode(tree.transform, "Уровень 2\n+Шанс редкости\nСтоимость: 120");
        CreateTreeNode(tree.transform, "Уровень 3\n+Стартовый уровень\nСтоимость: 250");
        CreateTreeNode(tree.transform, "Уровень 4\n+Пул навыков\nСтоимость: 400");

        CreateInfoCard("Пока заглушка", "Следующий этап: добавить ресурс «Осколки души», реальные узлы, связи между ними и сохранение купленных улучшений.", new Vector2(0f, -250f), new Vector2(760f, 160f));
    }

    private void BuildSettingsScreen()
    {
        masterSlider = CreateSliderRow("Общая громкость", new Vector2(0f, 160f), SetMasterVolume);
        musicSlider = CreateSliderRow("Музыка", new Vector2(0f, 80f), SetMusicVolume);
        sfxSlider = CreateSliderRow("Эффекты", new Vector2(0f, 0f), SetSfxVolume);

        CreateSettingButton("Качество графики", PlayerPrefs.GetString(QualityKey, "Среднее"), new Vector2(-260f, -110f), () => CycleSetting(QualityKey, new[] { "Низкое", "Среднее", "Высокое" }));
        CreateSettingButton("Язык", PlayerPrefs.GetString(LanguageKey, "Русский"), new Vector2(260f, -110f), () => CycleSetting(LanguageKey, new[] { "Русский", "English" }));
        CreateSettingButton("Полный экран", PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1 ? "Да" : "Нет", new Vector2(-260f, -210f), ToggleFullscreen);
        CreateInfoCard("Горячие клавиши", "Движение: WASD\nРывок: Space\nПауза/выбор улучшений: через UI", new Vector2(260f, -210f), new Vector2(420f, 120f));
    }

    private void OpenMapDetails(MapEntry map)
    {
        pendingMap = map;

        if (mapDetailsText != null)
        {
            mapDetailsText.text =
                $"{map.Name}\n" +
                $"Сложность: {map.Difficulty}\n" +
                $"Биом: {map.Biome}\n\n" +
                $"Враги: {map.Enemies}\n" +
                $"Награды: {map.Rewards}";
        }
    }

    private void SelectMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        selectedMapId = mapId;
        SelectedLoadoutStore.SetSelectedMap(mapId);
        RefreshMapButtons();
        RefreshHeroPreview();
    }

    private void SelectCharacter(CharacterData character)
    {
        if (character == null)
        {
            return;
        }

        selectedCharacter = character;
        SelectedCharacterStore.SetSelectedCharacter(character);
        selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(character);
        previewedCharacter = character;
        RefreshCharacterButtons();
        RefreshWeaponButtons();
        RefreshCharacterPreviewPanel();
        RefreshHeroPreview();
    }

    private void PreviewCharacter(CharacterData character)
    {
        if (character == null)
        {
            return;
        }

        previewedCharacter = character;
        RefreshCharacterButtons();
        RefreshCharacterPreviewPanel();
    }

    private void ConfirmPreviewedCharacter()
    {
        SelectCharacter(previewedCharacter);
    }

    private void SelectWeapon(WeaponData weapon)
    {
        if (selectedCharacter == null || weapon == null)
        {
            return;
        }

        selectedWeapon = weapon;
        SelectedLoadoutStore.SetSelectedWeapon(selectedCharacter, weapon);
        RefreshWeaponButtons();
        RefreshHeroPreview();
    }

    private void RefreshHeroPreview()
    {
        if (heroPreviewText == null)
        {
            RefreshWorldHero();
            return;
        }

        heroPreviewText.text = string.Empty;
        RefreshWorldHero();
    }

    private string GetLoadoutSummary()
    {
        MapEntry map = FindMap(selectedMapId);
        string characterName = selectedCharacter != null ? selectedCharacter.DisplayName : "Не выбран";
        string weaponName = selectedWeapon != null ? selectedWeapon.DisplayName : "Не выбрано";
        string mapName = !string.IsNullOrWhiteSpace(map.Name) ? map.Name : "Не выбрана";
        return $"Персонаж: {characterName}\nОружие: {weaponName}\nКарта: {mapName}";
    }

    private string BuildCharacterStats(CharacterData character)
    {
        if (character == null)
        {
            return string.Empty;
        }

        float hp = GetStat(character.BaseStats, StatType.MaxHealth);
        float speed = GetStat(character.BaseStats, StatType.MoveSpeed);
        float defense = GetStat(character.BaseStats, StatType.Defense);
        float parry = GetStat(character.BaseStats, StatType.ParryChance);
        return $"HP: {hp:0}\nСкорость: {speed:0.0}\nЗащита: {defense:0}\nПарирование: {parry:P0}";
    }

    private string BuildCharacterPreview(CharacterData character)
    {
        if (character == null)
        {
            return "Выбери героя в сетке слева.";
        }

        float hp = GetStat(character.BaseStats, StatType.MaxHealth);
        float speed = GetStat(character.BaseStats, StatType.MoveSpeed);
        float pickup = GetStat(character.BaseStats, StatType.PickupRadius);
        float defense = GetStat(character.BaseStats, StatType.Defense);
        float parry = GetStat(character.BaseStats, StatType.ParryChance);
        float xp = GetStat(character.BaseStats, StatType.ExperienceMultiplier, 1f);
        float dash = GetStat(character.BaseStats, StatType.DashCharges);
        float regen = GetStat(character.BaseStats, StatType.HealthRegenPercent);
        float revives = GetStat(character.BaseStats, StatType.LifeTotemCount);
        string weapon = character.StartingWeapon != null ? character.StartingWeapon.DisplayName : "нет";

        return
            $"Оружие: {weapon}\n\n" +
            $"Здоровье: {hp:0}\n" +
            $"Скорость: {speed:0.0}\n" +
            $"Радиус опыта: {pickup:0.0}\n" +
            $"Защита: {defense:0}\n" +
            $"Парирование: {parry:P0}\n" +
            $"Опыт: x{xp:0.00}\n" +
            $"Рывки: {dash:0}\n" +
            $"Регенерация: {regen:P1}\n" +
            $"Воскрешения: {revives:0}";
    }

    private string BuildWeaponDescription(WeaponData weapon)
    {
        string startingSkill = weapon.StartingSkill != null ? weapon.StartingSkill.DisplayName : "нет";
        int skillCount = weapon.UniqueSkillPool != null ? weapon.UniqueSkillPool.Length : 0;
        return $"Стартовый навык: {startingSkill}\nНавыков в пуле: {skillCount}\n\nМодификаторы оружия будут отображаться здесь.";
    }

    private void RefreshCharacterButtons()
    {
        for (int i = 0; i < characterButtons.Count; i++)
        {
            bool previewed = i < characters.Count && characters[i] == previewedCharacter;
            SetButtonSelected(characterButtons[i], previewed);

            if (i < characterCardViews.Count && characterCardViews[i] != null)
            {
                characterCardViews[i].SetSelected(previewed);
            }
        }
    }

    private void RefreshWeaponButtons()
    {
        if (weaponButtons.Count == 0 || selectedCharacter == null || selectedCharacter.AvailableWeapons == null)
        {
            return;
        }

        int buttonIndex = 0;

        for (int i = 0; i < selectedCharacter.AvailableWeapons.Length && buttonIndex < weaponButtons.Count; i++)
        {
            WeaponData weapon = selectedCharacter.AvailableWeapons[i];

            if (weapon == null)
            {
                continue;
            }

            SetButtonSelected(weaponButtons[buttonIndex], weapon == selectedWeapon);

            if (buttonIndex < weaponCardViews.Count && weaponCardViews[buttonIndex] != null)
            {
                weaponCardViews[buttonIndex].SetSelected(weapon == selectedWeapon);
            }

            buttonIndex++;
        }
    }

    private void RefreshMapButtons()
    {
        for (int i = 0; i < mapButtons.Count; i++)
        {
            bool selected = i < maps.Count && maps[i].Id == selectedMapId;
            SetButtonSelected(mapButtons[i], selected);

            if (i < mapCardViews.Count && mapCardViews[i] != null)
            {
                mapCardViews[i].SetSelected(selected);
            }
        }

        if (battleButton != null)
        {
            battleButton.interactable = !string.IsNullOrWhiteSpace(selectedMapId);
        }
    }

    private void SetButtonSelected(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        Color color = selected ? accentColor : cardColor;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.selectedColor = color;
        colors.highlightedColor = selected ? new Color(0.95f, 0.76f, 0.38f, 1f) : new Color(0.24f, 0.25f, 0.29f, 1f);
        colors.pressedColor = new Color(color.r * 0.82f, color.g * 0.82f, color.b * 0.82f, 1f);
        button.colors = colors;

        if (button.targetGraphic is Image image)
        {
            image.color = color;
        }
    }

    private Slider CreateSliderRow(string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction<float> onChanged)
    {
        CreateText(label + "Label", contentRoot, label, 22, FontStyle.Bold, anchoredPosition + new Vector2(-250f, 18f), new Vector2(280f, 36f));

        GameObject sliderObject = new(label + "Slider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(contentRoot, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 24f);

        GameObject background = CreatePanel("Background", sliderObject.transform, new Color(0.18f, 0.19f, 0.23f, 1f));
        Stretch(background.GetComponent<RectTransform>());

        GameObject fillArea = new("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        GameObject fill = CreatePanel("Fill", fillArea.transform, accentColor);
        Stretch(fill.GetComponent<RectTransform>());

        GameObject handleArea = new("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        Stretch(handleArea.GetComponent<RectTransform>());

        GameObject handle = CreatePanel("Handle", handleArea.transform, Color.white);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 34f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.onValueChanged.AddListener(onChanged);
        return slider;
    }

    private Button CreateMenuButton(string objectName, string label, UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(leftMenuRoot, label, action, Vector2.zero, new Vector2(244f, 54f));
        button.gameObject.name = objectName;
        LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 54f;
        return button;
    }

    private Button CreateMapCard(Transform parent, MapEntry map)
    {
        if (mapCardPrefab == null)
        {
            return CreateCardButton(parent, map.Name, $"{map.Biome}\nСложность: {map.Difficulty}", () => OpenMapDetails(map), new Vector2(220f, 210f));
        }

        MapCardView view = Instantiate(mapCardPrefab, parent);
        view.name = $"MapCard_{map.Id}";
        view.Configure(map.Name, map.Biome, map.Difficulty, () => OpenMapDetails(map));
        mapCardViews.Add(view);
        EnsureLayoutSize(view.gameObject, new Vector2(220f, 210f));
        return view.Button;
    }

    private Button CreateCharacterCard(Transform parent, CharacterData character, string stats)
    {
        if (characterCardPrefab == null)
        {
            return CreateCardButton(parent, character.DisplayName, stats, () => PreviewCharacter(character), new Vector2(220f, 230f));
        }

        CharacterCardView view = Instantiate(characterCardPrefab, parent);
        view.name = $"CharacterCard_{character.name}";
        view.Configure(character, stats, () => PreviewCharacter(character));
        characterCardViews.Add(view);
        EnsureLayoutSize(view.gameObject, new Vector2(220f, 230f));
        return view.Button;
    }

    private Button CreateWeaponCard(Transform parent, WeaponData weapon, string description)
    {
        if (weaponCardPrefab == null)
        {
            return CreateCardButton(parent, weapon.DisplayName, description, () => SelectWeapon(weapon), new Vector2(280f, 290f));
        }

        WeaponCardView view = Instantiate(weaponCardPrefab, parent);
        view.name = $"WeaponCard_{weapon.name}";
        view.Configure(weapon, description, () => SelectWeapon(weapon));
        weaponCardViews.Add(view);
        EnsureLayoutSize(view.gameObject, new Vector2(280f, 290f));
        return view.Button;
    }

    private static void EnsureLayoutSize(GameObject target, Vector2 size)
    {
        RectTransform rect = target.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.sizeDelta = size;
        }

        LayoutElement layout = target.GetComponent<LayoutElement>();
        layout = layout != null ? layout : target.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
    }

    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = CreatePanel(label + "Button", parent, accentColor);
        buttonObject.AddComponent<Button>();
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(action);

        Text text = CreateText(label + "Text", buttonObject.transform, label, 24, FontStyle.Bold);
        Stretch(text.rectTransform);
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.08f, 0.07f, 0.05f, 1f);
        return button;
    }

    private Button CreateCardButton(Transform parent, string title, string body, UnityEngine.Events.UnityAction action, Vector2 size)
    {
        Button button = CreateButton(parent, string.Empty, action, Vector2.zero, size);
        button.GetComponent<Image>().color = cardColor;

        Text titleText = CreateText("Title", button.transform, title, 24, FontStyle.Bold);
        titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        titleText.rectTransform.offsetMin = new Vector2(18f, -70f);
        titleText.rectTransform.offsetMax = new Vector2(-18f, -16f);
        titleText.alignment = TextAnchor.UpperCenter;

        Text bodyText = CreateText("Body", button.transform, body, 17, FontStyle.Normal);
        bodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        bodyText.rectTransform.offsetMin = new Vector2(18f, 18f);
        bodyText.rectTransform.offsetMax = new Vector2(-18f, -82f);
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.color = new Color(0.9f, 0.91f, 0.94f, 1f);

        return button;
    }

    private void CreateInfoCard(string title, string body, Vector2 position, Vector2 size, bool rightPinned = false)
    {
        GameObject card = CreatePanel(title + "Card", contentRoot, panelColor);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = rightPinned ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0.5f);
        rect.anchorMax = rightPinned ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0.5f);
        rect.pivot = rightPinned ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = rightPinned ? new Vector2(0f, position.y) : position;
        rect.sizeDelta = size;

        Text titleText = CreateText("Title", card.transform, title, 28, FontStyle.Bold);
        titleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        titleText.rectTransform.offsetMin = new Vector2(24f, -70f);
        titleText.rectTransform.offsetMax = new Vector2(-24f, -18f);

        Text bodyText = CreateText("Body", card.transform, body, 20, FontStyle.Normal);
        bodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        bodyText.rectTransform.anchorMax = new Vector2(1f, 1f);
        bodyText.rectTransform.offsetMin = new Vector2(24f, 24f);
        bodyText.rectTransform.offsetMax = new Vector2(-24f, -86f);
        bodyText.alignment = TextAnchor.UpperLeft;
    }

    private void CreateCharacterPreviewPanel()
    {
        GameObject card = CreatePanel("CharacterPreviewCard", contentRoot, panelColor);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(390f, 520f);

        characterPreviewTitleText = CreateText("Title", card.transform, string.Empty, 30, FontStyle.Bold);
        characterPreviewTitleText.rectTransform.anchorMin = new Vector2(0f, 1f);
        characterPreviewTitleText.rectTransform.anchorMax = new Vector2(1f, 1f);
        characterPreviewTitleText.rectTransform.offsetMin = new Vector2(24f, -62f);
        characterPreviewTitleText.rectTransform.offsetMax = new Vector2(-24f, -14f);
        characterPreviewTitleText.alignment = TextAnchor.UpperCenter;

        Text figureText = CreateText("Figure", card.transform, "ФИГУРКА\nГЕРОЯ", 42, FontStyle.Bold);
        figureText.rectTransform.anchorMin = new Vector2(0f, 0.48f);
        figureText.rectTransform.anchorMax = new Vector2(1f, 0.86f);
        figureText.rectTransform.offsetMin = new Vector2(24f, 0f);
        figureText.rectTransform.offsetMax = new Vector2(-24f, 0f);
        figureText.alignment = TextAnchor.MiddleCenter;
        figureText.color = new Color(1f, 1f, 1f, 0.14f);

        characterPreviewBodyText = CreateText("Body", card.transform, string.Empty, 19, FontStyle.Normal);
        characterPreviewBodyText.rectTransform.anchorMin = new Vector2(0f, 0f);
        characterPreviewBodyText.rectTransform.anchorMax = new Vector2(1f, 0.48f);
        characterPreviewBodyText.rectTransform.offsetMin = new Vector2(24f, 92f);
        characterPreviewBodyText.rectTransform.offsetMax = new Vector2(-24f, -10f);
        characterPreviewBodyText.alignment = TextAnchor.UpperLeft;

        characterSelectButton = CreateButton(card.transform, "ВЫБРАТЬ", ConfirmPreviewedCharacter, Vector2.zero, new Vector2(240f, 58f));
        RectTransform buttonRect = characterSelectButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 24f);
    }

    private void RefreshCharacterPreviewPanel()
    {
        if (characterPreviewTitleText != null)
        {
            characterPreviewTitleText.text = previewedCharacter != null ? previewedCharacter.DisplayName : "Персонаж";
        }

        if (characterPreviewBodyText != null)
        {
            characterPreviewBodyText.text = BuildCharacterPreview(previewedCharacter);
        }

        if (characterSelectButton != null)
        {
            characterSelectButton.interactable = previewedCharacter != null && previewedCharacter != selectedCharacter;
        }
    }

    private GameObject CreateLayout(string name, Transform parent, bool horizontal, float spacing)
    {
        GameObject group = new(name, typeof(RectTransform), horizontal ? typeof(HorizontalLayoutGroup) : typeof(VerticalLayoutGroup));
        group.transform.SetParent(parent, false);

        if (horizontal)
        {
            HorizontalLayoutGroup layout = group.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }
        else
        {
            VerticalLayoutGroup layout = group.GetComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        return group;
    }

    private void CreateTreeNode(Transform parent, string text)
    {
        Button node = CreateCardButton(parent, text, "Купить", () => { }, new Vector2(210f, 160f));
        node.interactable = false;
    }

    private void CreateSettingButton(string label, string value, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        CreateButton(contentRoot, $"{label}\n{value}", action, position, new Vector2(420f, 72f));
    }

    private void CycleSetting(string key, string[] values)
    {
        string current = PlayerPrefs.GetString(key, values[0]);
        int index = 0;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == current)
            {
                index = i;
                break;
            }
        }

        PlayerPrefs.SetString(key, values[(index + 1) % values.Length]);
        PlayerPrefs.Save();
        ShowScreen(MenuScreen.Settings);
    }

    private void ToggleFullscreen()
    {
        bool next = !Screen.fullScreen;
        Screen.fullScreen = next;
        PlayerPrefs.SetInt(FullscreenKey, next ? 1 : 0);
        PlayerPrefs.Save();
        ShowScreen(MenuScreen.Settings);
    }

    private Text CreateText(string name, Transform parent, string text, int size, FontStyle style)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text component = textObject.GetComponent<Text>();
        component.font = defaultFont;
        component.text = text;
        component.fontSize = size;
        component.fontStyle = style;
        component.color = Color.white;
        component.alignment = TextAnchor.MiddleLeft;
        component.horizontalOverflow = HorizontalWrapMode.Wrap;
        component.verticalOverflow = VerticalWrapMode.Overflow;
        return component;
    }

    private Text CreateText(string name, Transform parent, string text, int size, FontStyle style, Vector2 position, Vector2 rectSize)
    {
        Text component = CreateText(name, parent, text, size, style);
        RectTransform rect = component.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;
        return component;
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private void SetHeader(string title, string subtitle)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
        }
    }

    private void ClearContent()
    {
        if (contentRoot == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            DestroyObject(contentRoot.GetChild(i).gameObject);
        }
    }

    private void ClearGeneratedMenu()
    {
        if (canvas == null)
        {
            return;
        }

        ResetGeneratedReferences();

        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);

            if (child.name == GeneratedRootName)
            {
                DestroyObject(child.gameObject);
            }
        }

        GameObject worldRoot = GameObject.Find(WorldRootName);

        if (worldRoot != null)
        {
            DestroyObject(worldRoot);
        }
    }

    private void ResetGeneratedReferences()
    {
        contentRoot = null;
        leftMenuRoot = null;
        mapDetailsPanel = null;
        titleText = null;
        subtitleText = null;
        heroPreviewText = null;
        mapDetailsText = null;
        battleButton = null;
        masterSlider = null;
        musicSlider = null;
        sfxSlider = null;
        menuWorldRoot = null;
        heroWorldRoot = null;
        characterButtons.Clear();
        weaponButtons.Clear();
        mapButtons.Clear();
        characterCardViews.Clear();
        weaponCardViews.Clear();
        mapCardViews.Clear();
    }

    private bool TryBindSceneLayout()
    {
        if (canvas == null)
        {
            return false;
        }

        Transform root = canvas.transform.Find(GeneratedRootName);

        if (root == null)
        {
            return false;
        }

        Image rootImage = root.GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.color = new Color(0f, 0f, 0f, 0f);
            rootImage.raycastTarget = false;
        }

        Transform header = root.Find("Header");
        Transform heroPreview = root.Find("HeroPreview");
        Transform leftMenu = root.Find("LeftMenu");
        contentRoot = root.Find("Content");
        leftMenuRoot = leftMenu != null ? leftMenu.Find("MenuButtons") : null;
        titleText = GetText(header, "ScreenTitle");
        subtitleText = GetText(header, "ScreenSubtitle");
        heroPreviewText = GetText(heroPreview, "HeroPreviewText");

        return contentRoot != null
            && leftMenuRoot != null
            && titleText != null
            && subtitleText != null
            && heroPreviewText != null;
    }

    private void BindMenuButtons()
    {
        if (leftMenuRoot == null)
        {
            return;
        }

        BindMenuButton("PlayMenuButton", 0, () => ShowScreen(MenuScreen.Play));
        BindMenuButton("CharacterMenuButton", 1, () => ShowScreen(MenuScreen.Character));
        BindMenuButton("WeaponMenuButton", 2, () => ShowScreen(MenuScreen.Weapon));
        BindMenuButton("SkillTreeMenuButton", 3, () => ShowScreen(MenuScreen.SkillTree));
        BindMenuButton("SettingsMenuButton", 4, () => ShowScreen(MenuScreen.Settings));
        BindMenuButton("ExitMenuButton", 5, QuitGame);
    }

    private void BindMenuButton(string objectName, int fallbackIndex, UnityEngine.Events.UnityAction action)
    {
        Button button = FindChildComponent<Button>(leftMenuRoot, objectName);

        if (button == null && fallbackIndex >= 0 && fallbackIndex < leftMenuRoot.childCount)
        {
            button = leftMenuRoot.GetChild(fallbackIndex).GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static Text GetText(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private static T FindChildComponent<T>(Transform parent, string childName) where T : Component
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
        }

        return null;
    }

    private void CacheCharacters()
    {
        characters.Clear();

        if (characterRoster == null || characterRoster.Characters == null)
        {
            return;
        }

        for (int i = 0; i < characterRoster.Characters.Length; i++)
        {
            if (characterRoster.Characters[i] != null)
            {
                characters.Add(characterRoster.Characters[i]);
            }
        }

        if (selectedCharacter == null && characters.Count > 0)
        {
            SelectCharacter(characters[0]);
        }
    }

    private void LoadMapEntries()
    {
        MapDatabase database = SelectedLoadoutStore.LoadMapDatabase();

        if (database == null || database.Maps == null || database.Maps.Length == 0)
        {
            return;
        }

        maps.Clear();

        for (int i = 0; i < database.Maps.Length; i++)
        {
            MapData map = database.Maps[i];

            if (map == null)
            {
                continue;
            }

            maps.Add(new MapEntry(
                map.MapId,
                map.DisplayName,
                map.Difficulty,
                map.Biome,
                map.EnemyDescription,
                map.RewardsDescription));
        }
    }

    private MapEntry FindMap(string id)
    {
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i].Id == id)
            {
                return maps[i];
            }
        }

        return maps.Count > 0 ? maps[0] : default;
    }

    private static float GetStat(StatValue[] stats, StatType statType, float fallback = 0f)
    {
        if (stats == null)
        {
            return fallback;
        }

        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].statType == statType)
            {
                return stats[i].value;
            }
        }

        return fallback;
    }

    private void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(master);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(music);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfx);
        }

        AudioListener.volume = master;
    }

    private void EnsureCanvas()
    {
        canvas = FindFirstObjectByType<Canvas>();

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler existingScaler = canvas.GetComponent<CanvasScaler>();

            if (existingScaler != null)
            {
                existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                existingScaler.referenceResolution = new Vector2(1920f, 1080f);
                existingScaler.matchWidthOrHeight = 0.5f;
            }

            return;
        }

        GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void EnsureWorldBackdrop()
    {
        GameObject existing = GameObject.Find(WorldRootName);

        if (existing != null)
        {
            menuWorldRoot = existing.transform;
            heroWorldRoot = menuWorldRoot.Find(HeroWorldRootName);
            RefreshWorldHero();
            return;
        }

        GameObject root = new(WorldRootName);
        menuWorldRoot = root.transform;

        SetupMenuCamera();
        CreateDungeonRoom(menuWorldRoot);

        GameObject heroRootObject = new(HeroWorldRootName);
        heroRootObject.transform.SetParent(menuWorldRoot, false);
        heroWorldRoot = heroRootObject.transform;
        heroWorldRoot.position = new Vector3(2.65f, 0.03f, 0.55f);
        heroWorldRoot.rotation = Quaternion.Euler(0f, -18f, 0f);

        RefreshWorldHero();
    }

    private void SetupMenuCamera()
    {
        Camera camera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();

        if (camera == null)
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            camera = cameraObject.GetComponent<Camera>();
        }

        camera.transform.position = new Vector3(0f, 2.15f, -6.6f);
        camera.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        camera.fieldOfView = 34f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.028f, 0.026f, 0.034f, 1f);

        Light existingLight = FindFirstObjectByType<Light>();

        if (existingLight == null)
        {
            GameObject lightObject = new("MainMenu_KeyLight", typeof(Light));
            existingLight = lightObject.GetComponent<Light>();
        }

        existingLight.type = LightType.Directional;
        existingLight.color = new Color(1f, 0.78f, 0.52f, 1f);
        existingLight.intensity = 1.1f;
        existingLight.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
    }

    private void CreateDungeonRoom(Transform parent)
    {
        CreatePrimitive(parent, "RoomShadowBackdrop", PrimitiveType.Cube, new Vector3(0f, 1.7f, 4.18f), Vector3.zero, new Vector3(12.6f, 4.5f, 0.2f), new Color(0.055f, 0.05f, 0.065f, 1f));
        CreatePrimitive(parent, "FloorBase", PrimitiveType.Cube, new Vector3(0f, -0.09f, 1.05f), Vector3.zero, new Vector3(12.4f, 0.18f, 8.7f), new Color(0.13f, 0.105f, 0.09f, 1f));
        CreatePrimitive(parent, "LeftWallBase", PrimitiveType.Cube, new Vector3(-5.95f, 1.55f, 1.05f), new Vector3(0f, -12f, 0f), new Vector3(0.28f, 3.35f, 6.9f), new Color(0.075f, 0.07f, 0.085f, 1f));
        CreatePrimitive(parent, "RightWallBase", PrimitiveType.Cube, new Vector3(5.95f, 1.55f, 1.05f), new Vector3(0f, 12f, 0f), new Vector3(0.28f, 3.35f, 6.9f), new Color(0.075f, 0.07f, 0.085f, 1f));

        CreateFloorMosaic(parent);
        CreateBackWallMasonry(parent);
        CreateSideWallMasonry(parent, -5.78f, -12f, "Left");
        CreateSideWallMasonry(parent, 5.78f, 12f, "Right");

        CreatePrimitive(parent, "LeftPillar", PrimitiveType.Cube, new Vector3(-3.0f, 1.5f, 3.76f), Vector3.zero, new Vector3(0.46f, 3.0f, 0.42f), new Color(0.16f, 0.14f, 0.15f, 1f));
        CreatePrimitive(parent, "RightPillar", PrimitiveType.Cube, new Vector3(3.25f, 1.5f, 3.76f), Vector3.zero, new Vector3(0.46f, 3.0f, 0.42f), new Color(0.16f, 0.14f, 0.15f, 1f));
        CreateArch(parent, new Vector3(-3.95f, 1.38f, 3.98f));
        CreateBanner(parent, new Vector3(0.95f, 2.18f, 3.92f), "BannerCenter", new Color(0.015f, 0.06f, 0.14f, 1f));
        CreateBanner(parent, new Vector3(4.35f, 1.92f, 3.92f), "BannerRight", new Color(0.018f, 0.052f, 0.12f, 1f));
        CreateTorch(parent, new Vector3(-5.05f, 1.42f, 2.85f), "TorchLeft");
        CreateTorch(parent, new Vector3(3.35f, 1.42f, 3.05f), "TorchRight");
        CreateTable(parent);
        CreateSummonCircle(parent, new Vector3(2.65f, 0.04f, 0.55f));
    }

    private void CreateFloorMosaic(Transform parent)
    {
        for (int x = -5; x <= 5; x++)
        {
            for (int z = -3; z <= 4; z++)
            {
                float wave = Mathf.Abs(x) * 0.015f + Mathf.Abs(z) * 0.01f;
                Color tileColor = ((x + z) & 1) == 0
                    ? new Color(0.25f - wave, 0.205f - wave, 0.165f - wave, 1f)
                    : new Color(0.18f - wave, 0.145f - wave, 0.12f - wave, 1f);

                Vector3 position = new(x * 0.88f + (z % 2) * 0.08f, 0.012f, z * 0.78f + 0.75f);
                Vector3 scale = new(0.78f, 0.035f, 0.68f);
                CreatePrimitive(parent, $"FloorTile_{x}_{z}", PrimitiveType.Cube, position, Vector3.zero, scale, tileColor);
            }
        }
    }

    private void CreateBackWallMasonry(Transform parent)
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = -6; x <= 6; x++)
            {
                float offset = y % 2 == 0 ? 0f : 0.43f;
                float width = y % 3 == 0 ? 0.76f : 0.86f;
                Color stone = ((x + y) & 1) == 0 ? new Color(0.17f, 0.16f, 0.18f, 1f) : new Color(0.12f, 0.115f, 0.135f, 1f);
                CreatePrimitive(parent, $"BackStone_{x}_{y}", PrimitiveType.Cube, new Vector3(x * 0.86f + offset, y * 0.46f + 0.36f, 3.92f), Vector3.zero, new Vector3(width, 0.34f, 0.07f), stone);
            }
        }
    }

    private void CreateSideWallMasonry(Transform parent, float xPosition, float yRotation, string side)
    {
        for (int y = 0; y < 5; y++)
        {
            for (int z = -2; z <= 4; z++)
            {
                float offset = y % 2 == 0 ? 0f : 0.36f;
                Color stone = ((z + y) & 1) == 0 ? new Color(0.14f, 0.13f, 0.15f, 1f) : new Color(0.095f, 0.09f, 0.11f, 1f);
                CreatePrimitive(parent, $"{side}WallStone_{z}_{y}", PrimitiveType.Cube, new Vector3(xPosition, y * 0.5f + 0.34f, z * 0.74f + 0.8f + offset), new Vector3(0f, yRotation, 0f), new Vector3(0.08f, 0.36f, 0.66f), stone);
            }
        }
    }

    private void CreateArch(Transform parent, Vector3 center)
    {
        Color stone = new(0.18f, 0.16f, 0.17f, 1f);
        CreatePrimitive(parent, "Arch_DarkOpening", PrimitiveType.Cube, center + new Vector3(0f, -0.42f, -0.08f), Vector3.zero, new Vector3(1.18f, 1.45f, 0.05f), new Color(0.028f, 0.024f, 0.032f, 1f));
        CreatePrimitive(parent, "Arch_LeftColumn", PrimitiveType.Cube, center + new Vector3(-0.72f, -0.28f, -0.04f), Vector3.zero, new Vector3(0.26f, 1.72f, 0.14f), stone);
        CreatePrimitive(parent, "Arch_RightColumn", PrimitiveType.Cube, center + new Vector3(0.72f, -0.28f, -0.04f), Vector3.zero, new Vector3(0.26f, 1.72f, 0.14f), stone);

        for (int i = 0; i < 7; i++)
        {
            float t = i / 6f;
            float angle = Mathf.Lerp(205f, 335f, t);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 blockPosition = center + new Vector3(Mathf.Cos(radians) * 0.82f, Mathf.Sin(radians) * 0.64f + 0.2f, -0.035f);
            CreatePrimitive(parent, $"ArchStone_{i}", PrimitiveType.Cube, blockPosition, new Vector3(0f, 0f, angle + 90f), new Vector3(0.32f, 0.2f, 0.14f), stone);
        }
    }

    private void CreateBanner(Transform parent, Vector3 position, string name, Color color)
    {
        CreatePrimitive(parent, name + "_Rod", PrimitiveType.Cylinder, position + new Vector3(0f, 0.82f, -0.04f), new Vector3(0f, 0f, 90f), new Vector3(0.045f, 0.6f, 0.045f), new Color(0.47f, 0.27f, 0.08f, 1f));
        CreatePrimitive(parent, name + "_Cloth", PrimitiveType.Cube, position + new Vector3(0f, 0.08f, 0f), Vector3.zero, new Vector3(0.78f, 1.42f, 0.035f), color);
        CreatePrimitive(parent, name + "_BottomPoint", PrimitiveType.Cube, position + new Vector3(0f, -0.66f, -0.002f), new Vector3(0f, 0f, 45f), new Vector3(0.38f, 0.38f, 0.032f), color);
        CreatePrimitive(parent, name + "_MarkLeft", PrimitiveType.Cube, position + new Vector3(-0.12f, 0.12f, -0.05f), new Vector3(0f, 0f, 42f), new Vector3(0.08f, 0.58f, 0.028f), accentColor);
        CreatePrimitive(parent, name + "_MarkRight", PrimitiveType.Cube, position + new Vector3(0.12f, 0.12f, -0.055f), new Vector3(0f, 0f, -42f), new Vector3(0.08f, 0.58f, 0.028f), accentColor);
        CreatePrimitive(parent, name + "_MarkTip", PrimitiveType.Cube, position + new Vector3(0f, 0.43f, -0.06f), Vector3.zero, new Vector3(0.12f, 0.32f, 0.028f), accentColor);
    }

    private void CreateTorch(Transform parent, Vector3 position, string name)
    {
        CreatePrimitive(parent, name + "_Bracket", PrimitiveType.Cube, position + new Vector3(0f, -0.14f, 0.03f), new Vector3(0f, 0f, -28f), new Vector3(0.08f, 0.48f, 0.08f), new Color(0.2f, 0.11f, 0.04f, 1f));
        CreatePrimitive(parent, name + "_Cup", PrimitiveType.Cylinder, position + new Vector3(0f, 0.12f, -0.03f), Vector3.zero, new Vector3(0.16f, 0.16f, 0.16f), new Color(0.36f, 0.21f, 0.07f, 1f));
        CreatePrimitive(parent, name + "_FlameCore", PrimitiveType.Sphere, position + new Vector3(0f, 0.38f, -0.08f), Vector3.zero, new Vector3(0.24f, 0.4f, 0.24f), new Color(1f, 0.64f, 0.08f, 1f));
        CreatePrimitive(parent, name + "_FlameOuter", PrimitiveType.Sphere, position + new Vector3(0f, 0.32f, -0.09f), Vector3.zero, new Vector3(0.36f, 0.54f, 0.36f), new Color(1f, 0.2f, 0.02f, 1f));
        GameObject lightObject = new(name + "_Light", typeof(Light));
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = position + new Vector3(0f, 0.32f, -0.6f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.48f, 0.16f, 1f);
        light.intensity = 2.8f;
        light.range = 5.2f;
    }

    private void CreateTable(Transform parent)
    {
        CreatePrimitive(parent, "AlchemyTableTop", PrimitiveType.Cube, new Vector3(4.25f, 0.72f, 1.9f), new Vector3(0f, -8f, 0f), new Vector3(1.5f, 0.13f, 0.55f), new Color(0.24f, 0.13f, 0.055f, 1f));
        CreatePrimitive(parent, "AlchemyTableLegA", PrimitiveType.Cube, new Vector3(3.72f, 0.34f, 1.66f), Vector3.zero, new Vector3(0.11f, 0.7f, 0.11f), new Color(0.14f, 0.07f, 0.035f, 1f));
        CreatePrimitive(parent, "AlchemyTableLegB", PrimitiveType.Cube, new Vector3(4.78f, 0.34f, 2.08f), Vector3.zero, new Vector3(0.11f, 0.7f, 0.11f), new Color(0.14f, 0.07f, 0.035f, 1f));
        CreatePrimitive(parent, "PotionBlueBody", PrimitiveType.Sphere, new Vector3(4.0f, 0.95f, 1.78f), Vector3.zero, new Vector3(0.2f, 0.25f, 0.2f), new Color(0.05f, 0.25f, 0.65f, 0.95f));
        CreatePrimitive(parent, "PotionBlueNeck", PrimitiveType.Cylinder, new Vector3(4.0f, 1.16f, 1.78f), Vector3.zero, new Vector3(0.06f, 0.18f, 0.06f), new Color(0.08f, 0.28f, 0.7f, 0.95f));
        CreatePrimitive(parent, "PotionGreenBody", PrimitiveType.Sphere, new Vector3(4.45f, 0.95f, 1.82f), Vector3.zero, new Vector3(0.18f, 0.22f, 0.18f), new Color(0.08f, 0.48f, 0.24f, 0.95f));
        CreatePrimitive(parent, "PotionGreenNeck", PrimitiveType.Cylinder, new Vector3(4.45f, 1.12f, 1.82f), Vector3.zero, new Vector3(0.052f, 0.15f, 0.052f), new Color(0.08f, 0.5f, 0.26f, 0.95f));
    }

    private void CreateSummonCircle(Transform parent, Vector3 position)
    {
        CreatePrimitive(parent, "SummonCircleGlow", PrimitiveType.Cylinder, position + new Vector3(0f, -0.006f, 0f), Vector3.zero, new Vector3(1.55f, 0.012f, 1.55f), new Color(1f, 0.33f, 0.02f, 0.5f));
        CreatePrimitive(parent, "SummonCircleOuter", PrimitiveType.Cylinder, position, Vector3.zero, new Vector3(1.24f, 0.018f, 1.24f), new Color(1f, 0.52f, 0.04f, 0.92f));
        CreatePrimitive(parent, "SummonCircleInner", PrimitiveType.Cylinder, position + new Vector3(0f, 0.012f, 0f), Vector3.zero, new Vector3(0.88f, 0.018f, 0.88f), new Color(0.25f, 0.13f, 0.04f, 0.95f));
        CreatePrimitive(parent, "SummonRuneA", PrimitiveType.Cube, position + new Vector3(0f, 0.035f, 0f), new Vector3(0f, 30f, 0f), new Vector3(0.08f, 0.018f, 0.95f), new Color(1f, 0.62f, 0.08f, 1f));
        CreatePrimitive(parent, "SummonRuneB", PrimitiveType.Cube, position + new Vector3(0f, 0.04f, 0f), new Vector3(0f, -30f, 0f), new Vector3(0.08f, 0.018f, 0.95f), new Color(1f, 0.62f, 0.08f, 1f));
    }

    private void RefreshWorldHero()
    {
        if (heroWorldRoot == null)
        {
            return;
        }

        for (int i = heroWorldRoot.childCount - 1; i >= 0; i--)
        {
            DestroyObject(heroWorldRoot.GetChild(i).gameObject);
        }

        if (TryCreateSelectedCharacterPrefab())
        {
            return;
        }

        CreatePrimitive(heroWorldRoot, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.85f, 0f), Vector3.zero, new Vector3(0.55f, 0.85f, 0.55f), GetHeroColor(selectedCharacter));
        CreatePrimitive(heroWorldRoot, "HeadShadow", PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f), Vector3.zero, new Vector3(0.38f, 0.38f, 0.38f), new Color(0.035f, 0.035f, 0.04f, 1f));
        CreatePrimitive(heroWorldRoot, "Hood", PrimitiveType.Capsule, new Vector3(0f, 1.56f, -0.02f), Vector3.zero, new Vector3(0.56f, 0.42f, 0.48f), new Color(0.82f, 0.78f, 0.66f, 1f));
        CreatePrimitive(heroWorldRoot, "Belt", PrimitiveType.Cube, new Vector3(0f, 0.78f, -0.02f), Vector3.zero, new Vector3(0.68f, 0.12f, 0.48f), accentColor);
        CreatePrimitive(heroWorldRoot, "LeftArm", PrimitiveType.Capsule, new Vector3(-0.44f, 0.88f, 0f), new Vector3(0f, 0f, 18f), new Vector3(0.18f, 0.5f, 0.18f), new Color(0.22f, 0.13f, 0.07f, 1f));
        CreatePrimitive(heroWorldRoot, "RightArm", PrimitiveType.Capsule, new Vector3(0.44f, 0.88f, 0f), new Vector3(0f, 0f, -18f), new Vector3(0.18f, 0.5f, 0.18f), new Color(0.22f, 0.13f, 0.07f, 1f));
    }

    private bool TryCreateSelectedCharacterPrefab()
    {
        if (selectedCharacter == null || selectedCharacter.ModelPrefab == null)
        {
            return false;
        }

        GameObject model = Instantiate(selectedCharacter.ModelPrefab, heroWorldRoot);
        model.name = $"Model_{selectedCharacter.CharacterId}";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        if (HasVisibleRenderer(model))
        {
            return true;
        }

        DestroyObject(model);
        return false;
    }

    private static bool HasVisibleRenderer(GameObject root)
    {
        if (root == null)
        {
            return false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private Color GetHeroColor(CharacterData character)
    {
        if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
        {
            return new Color(0.32f, 0.18f, 0.09f, 1f);
        }

        return character.CharacterId switch
        {
            "viking" => new Color(0.38f, 0.22f, 0.12f, 1f),
            "knight" => new Color(0.42f, 0.42f, 0.46f, 1f),
            "pyromancer" => new Color(0.55f, 0.12f, 0.05f, 1f),
            "ranger" => new Color(0.17f, 0.36f, 0.16f, 1f),
            "necromancer" => new Color(0.18f, 0.12f, 0.27f, 1f),
            "paladin" => new Color(0.62f, 0.52f, 0.28f, 1f),
            "rune_smith" => new Color(0.29f, 0.29f, 0.32f, 1f),
            _ => new Color(0.25f, 0.16f, 0.1f, 1f)
        };
    }

    private GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 rotation, Vector3 scale, Color color)
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
            Material material = new(Shader.Find("Universal Render Pipeline/Lit") != null ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        return primitive;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static void DestroyObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
            return;
        }

        DestroyImmediate(target);
    }

    private void MarkSceneDirtyInEditor()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

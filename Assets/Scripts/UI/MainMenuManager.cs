using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        Runes,
        Achievements,
        Profile,
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
        public Sprite PreviewImage;
        public MapData Data;

        public MapEntry(string id, string name, string difficulty, string biome, string enemies, string rewards, Sprite previewImage = null, MapData data = null)
        {
            Id = id;
            Name = name;
            Difficulty = difficulty;
            Biome = biome;
            Enemies = enemies;
            Rewards = rewards;
            PreviewImage = previewImage;
            Data = data;
        }
    }

    private const string MasterVolumeKey = "settings.master_volume";
    private const string MusicVolumeKey = "settings.music_volume";
    private const string SfxVolumeKey = "settings.sfx_volume";
    private const string QualityKey = "settings.quality";
    private const string FullscreenKey = "settings.fullscreen";
    private const string LanguageKey = "settings.language";
    private const string RootName = "MainMenu_Backdrop";
    private const string WorldRootName = "MainMenu_3DRoom";
    private const string HeroWorldRootName = "SelectedHeroPreview";
    private const int CharacterSlotCount = 12;
    private const int WeaponSlotCount = 9;
    private const int MapSlotCount = 6;

    [SerializeField] private string gameplaySceneName = "main";
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private CharacterRoster characterRoster;
    [SerializeField] private Color panelColor = new(0.11f, 0.12f, 0.145f, 0.94f);
    [SerializeField] private Color cardColor = new(0.17f, 0.18f, 0.21f, 0.96f);
    [SerializeField] private Color accentColor = new(0.86f, 0.68f, 0.32f, 1f);

    private readonly List<CharacterData> characters = new();
    private readonly List<MapEntry> maps = new()
    {
        new("forgotten_plains", "Забытые равнины", "Низкая", "Равнины", "Стаи слабых врагов, быстрые преследователи", "Осколки души, базовое оружие"),
        new("ashen_quarry", "Пепельный карьер", "Средняя", "Камень и пепел", "Танки, огненные враги, мини-боссы", "Камни улучшений, редкие пассивки"),
        new("venom_marsh", "Ядовитые топи", "Средняя", "Болото", "Дальние ядовики, замедляющие враги", "Ядовитые навыки, ресурсы статусов"),
        new("storm_spire", "Грозовой шпиль", "Высокая", "Башня шторма", "Быстрые элиты, молниевые кастеры", "Редкие навыки молнии"),
        new("bone_citadel", "Костяная цитадель", "Очень высокая", "Некрополь", "Толпы, элиты, усиленные боссы", "Эпические награды")
    };

    private readonly MainMenuHeroPreviewBuilder heroPreviewBuilder = new();

    private Canvas canvas;
    private Transform menuWorldRoot;
    private Transform heroWorldRoot;

    private GameObject homePanel;
    private GameObject playPanel;
    private GameObject characterPanel;
    private GameObject weaponPanel;
    private GameObject skillTreePanel;
    private GameObject runesPanel;
    private GameObject achievementsPanel;
    private GameObject profilePanel;
    private GameObject settingsPanel;

    private Button playMenuButton;
    private Button characterMenuButton;
    private Button weaponMenuButton;
    private Button skillTreeMenuButton;
    private Button runesMenuButton;
    private Button achievementsMenuButton;
    private Button profileMenuButton;
    private Button settingsMenuButton;
    private Button exitMenuButton;

    private MapCardView[] mapSlots;
    private CharacterCardView[] characterSlots;
    private WeaponCardView[] weaponSlots;
    private SkillTreePanelView skillTreeView;

    private TMP_Text mapDetailsText;
    private Text legacyMapDetailsText;
    private Button mapSelectButton;
    private Button battleButton;
    private TMP_Text characterPreviewTitleText;
    private TMP_Text characterPreviewBodyText;
    private Text legacyCharacterPreviewTitleText;
    private Text legacyCharacterPreviewBodyText;
    private Button characterSelectButton;
    private TMP_Text weaponInfoText;
    private Text legacyWeaponInfoText;
    private Button weaponImproveButton;
    private Button qualityButton;
    private Button languageButton;
    private Slider masterSlider;
    private Coroutine activeFadeCoroutine;
    private Slider musicSlider;
    private Slider sfxSlider;

    private CharacterData selectedCharacter;
    private CharacterData previewedCharacter;
    private WeaponData selectedWeapon;
    private WeaponData previewedWeapon;
    private string selectedMapId;
    private MapEntry pendingMap;

    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeMenu();
        }
    }

    [ContextMenu("Rebuild Menu Preview")]
    public void RebuildSceneMenuLayout()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogWarning("Main menu layout can only be rebuilt in Edit Mode.", this);
            return;
        }

        EnsureSceneAuthoredLayout(createMissing: true);
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#else
        Debug.LogWarning("Main menu layout rebuild is editor-only.");
#endif
    }

    [ContextMenu("Refresh Selected Hero Preview")]
    public void RefreshSelectedHeroPreviewInScene()
    {
        characterRoster = characterRoster != null ? characterRoster : SelectedCharacterStore.LoadRoster();
        selectedCharacter = SelectedCharacterStore.ResolveSelectedCharacter(characterRoster, selectedCharacter);
        BindWorldPreview();
        RefreshWorldHero();
        MarkSceneDirtyInEditor();
    }

    [ContextMenu("Clear Menu Preview")]
    public void ClearSceneMenuLayout()
    {
        Debug.LogWarning("Main menu is scene-authored now. Delete or edit visual objects directly in Hierarchy instead of clearing them from runtime code.", this);
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

    private void InitializeMenu()
    {
        characterRoster = characterRoster != null ? characterRoster : SelectedCharacterStore.LoadRoster();
        LoadMapEntries();
        SaveSystem.EnsureContentDefaults(characterRoster, SelectedLoadoutStore.LoadMapDatabase());
        selectedCharacter = SelectedCharacterStore.ResolveSelectedCharacter(characterRoster, null);
        selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(selectedCharacter);
        selectedMapId = SelectedLoadoutStore.GetSelectedMapId();
        pendingMap = string.IsNullOrWhiteSpace(selectedMapId) ? FindMap(null) : FindMap(selectedMapId);

        CacheCharacters();
        EnsureSceneAuthoredLayout(createMissing: false);

        if (!HasRequiredLayout())
        {
            Debug.LogError("MainMenu scene is missing scene-authored UI. Open MainMenu and run Soulstone -> Bake Main Menu Scene.", this);
            return;
        }

        BindButtons();
        NormalizeRuntimeTextStyle();
        PopulateAllScreens();
        LoadSettings();
        ShowScreen(MenuScreen.Home);
        RefreshWorldHero();
    }

    private void EnsureSceneAuthoredLayout(bool createMissing)
    {
        canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            if (!createMissing)
            {
                return;
            }

            GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (createMissing)
        {
            ConfigureCanvas(canvas);
        }

        Transform root = canvas.transform.Find(RootName);

        if (root == null && createMissing)
        {
            root = CreatePanel(RootName, canvas.transform, new Color(0f, 0f, 0f, 0f)).transform;
            Stretch(root.GetComponent<RectTransform>());
        }

        if (root == null)
        {
            return;
        }

        BuildStaticLayoutIfNeeded(root, createMissing);
        BindSceneReferences(root);
        BindWorldPreview();
    }

    private bool HasRequiredLayout()
    {
        return canvas != null
            && playPanel != null
            && characterPanel != null
            && weaponPanel != null
            && skillTreePanel != null
            && settingsPanel != null
            && playMenuButton != null
            && characterMenuButton != null
            && weaponMenuButton != null
            && skillTreeMenuButton != null
            && settingsMenuButton != null
            && exitMenuButton != null;
    }

    private void BuildStaticLayoutIfNeeded(Transform root, bool createMissing)
    {
        if (!createMissing)
        {
            return;
        }

        EnsureLeftMenu(root);
        Transform pagesRoot = EnsurePagesRoot(root);

        homePanel = EnsurePage(pagesRoot, "HomePanel");
        playPanel = EnsurePage(pagesRoot, "PlayPanel");
        characterPanel = EnsurePage(pagesRoot, "CharacterPanel");
        weaponPanel = EnsurePage(pagesRoot, "WeaponPanel");
        skillTreePanel = EnsurePage(pagesRoot, "SkillTreePanel");
        runesPanel = EnsurePage(pagesRoot, "RunesPanel");
        achievementsPanel = EnsurePage(pagesRoot, "AchievementsPanel");
        profilePanel = EnsurePage(pagesRoot, "ProfilePanel");
        settingsPanel = EnsurePage(pagesRoot, "SettingsPanel");

        EnsurePlayPanel(playPanel.transform);
        EnsureCharacterPanel(characterPanel.transform);
        EnsureWeaponPanel(weaponPanel.transform);
        EnsureSkillTreePanel(skillTreePanel.transform);
        EnsureSettingsPanel(settingsPanel.transform);
    }

    private void BindSceneReferences(Transform root)
    {
        Transform leftMenu = root.Find("MenuPanel/ButtonsContainer") ?? root.Find("LeftMenu/MenuButtons");
        playMenuButton = FindChild<Button>(leftMenu, "PlayButton") ?? FindChild<Button>(leftMenu, "PlayMenuButton");
        characterMenuButton = FindChild<Button>(leftMenu, "CharacterButton") ?? FindChild<Button>(leftMenu, "CharacterMenuButton");
        weaponMenuButton = FindChild<Button>(leftMenu, "WeaponButton") ?? FindChild<Button>(leftMenu, "WeaponMenuButton");
        skillTreeMenuButton = FindChild<Button>(leftMenu, "SkillTreeButton") ?? FindChild<Button>(leftMenu, "SkillTreeMenuButton");
        runesMenuButton = FindChild<Button>(leftMenu, "RunesButton") ?? FindChild<Button>(leftMenu, "RunesMenuButton");
        achievementsMenuButton = FindChild<Button>(leftMenu, "AchievementsButton") ?? FindChild<Button>(leftMenu, "AchievementsMenuButton");
        profileMenuButton = FindChild<Button>(leftMenu, "ProfileButton") ?? FindChild<Button>(leftMenu, "ProfileMenuButton");
        settingsMenuButton = FindChild<Button>(leftMenu, "SettingsButton") ?? FindChild<Button>(leftMenu, "SettingsMenuButton");
        exitMenuButton = FindChild<Button>(leftMenu, "ExitButton") ?? FindChild<Button>(leftMenu, "ExitMenuButton");

        Transform pagesRoot = root.Find("Pages");
        homePanel = FindPage(pagesRoot, "HomePanel");
        playPanel = FindPage(pagesRoot, "PlayPanel");
        characterPanel = FindPage(pagesRoot, "CharacterPanel");
        weaponPanel = FindPage(pagesRoot, "WeaponPanel");
        skillTreePanel = FindPage(pagesRoot, "SkillTreePanel");
        runesPanel = FindPage(pagesRoot, "RunesPanel");
        achievementsPanel = FindPage(pagesRoot, "AchievementsPanel");
        profilePanel = FindPage(pagesRoot, "ProfilePanel");
        settingsPanel = FindPage(pagesRoot, "SettingsPanel");

        mapSlots = playPanel != null ? playPanel.GetComponentsInChildren<MapCardView>(true) : null;
        characterSlots = characterPanel != null ? characterPanel.GetComponentsInChildren<CharacterCardView>(true) : null;
        weaponSlots = weaponPanel != null ? weaponPanel.GetComponentsInChildren<WeaponCardView>(true) : null;
        skillTreeView = skillTreePanel != null ? skillTreePanel.GetComponentInChildren<SkillTreePanelView>(true) : null;

        mapDetailsText = FindChild<TMP_Text>(playPanel != null ? playPanel.transform : null, "MapDetailsText");
        legacyMapDetailsText = FindChild<Text>(playPanel != null ? playPanel.transform : null, "MapDetailsText");
        mapSelectButton = FindChild<Button>(playPanel != null ? playPanel.transform : null, "MapSelectButton");
        battleButton = FindChild<Button>(playPanel != null ? playPanel.transform : null, "BattleButton");
        characterPreviewTitleText = FindChild<TMP_Text>(characterPanel != null ? characterPanel.transform : null, "CharacterPreviewTitle");
        characterPreviewBodyText = FindChild<TMP_Text>(characterPanel != null ? characterPanel.transform : null, "CharacterPreviewBody");
        legacyCharacterPreviewTitleText = FindChild<Text>(characterPanel != null ? characterPanel.transform : null, "CharacterPreviewTitle");
        legacyCharacterPreviewBodyText = FindChild<Text>(characterPanel != null ? characterPanel.transform : null, "CharacterPreviewBody");
        characterSelectButton = FindChild<Button>(characterPanel != null ? characterPanel.transform : null, "CharacterSelectButton");
        weaponInfoText = FindChild<TMP_Text>(weaponPanel != null ? weaponPanel.transform : null, "WeaponInfoText");
        legacyWeaponInfoText = FindChild<Text>(weaponPanel != null ? weaponPanel.transform : null, "WeaponInfoText");
        weaponImproveButton = FindChild<Button>(weaponPanel != null ? weaponPanel.transform : null, "ImproveButton")
            ?? FindChild<Button>(weaponPanel != null ? weaponPanel.transform : null, "ForgeImproveButton")
            ?? FindChild<Button>(weaponPanel != null ? weaponPanel.transform : null, "WeaponImproveButton");
        qualityButton = FindChild<Button>(settingsPanel != null ? settingsPanel.transform : null, "QualityButton");
        languageButton = FindChild<Button>(settingsPanel != null ? settingsPanel.transform : null, "LanguageButton");
        masterSlider = FindChild<Slider>(settingsPanel != null ? settingsPanel.transform : null, "MasterVolumeSlider");
        musicSlider = FindChild<Slider>(settingsPanel != null ? settingsPanel.transform : null, "MusicVolumeSlider");
        sfxSlider = FindChild<Slider>(settingsPanel != null ? settingsPanel.transform : null, "SfxVolumeSlider");
    }

    private void BindWorldPreview()
    {
        GameObject worldRoot = GameObject.Find(WorldRootName);
        menuWorldRoot = worldRoot != null ? worldRoot.transform : null;
        heroWorldRoot = FindChildRecursive(menuWorldRoot, HeroWorldRootName);
    }

    private void BindButtons()
    {
        BindButton(playMenuButton, () => ShowScreen(MenuScreen.Play));
        BindButton(characterMenuButton, () => ShowScreen(MenuScreen.Character));
        BindButton(weaponMenuButton, () => ShowScreen(MenuScreen.Weapon));
        BindButton(skillTreeMenuButton, () => ShowScreen(MenuScreen.SkillTree));
        BindButton(runesMenuButton, () => ShowScreen(MenuScreen.Runes));
        BindButton(achievementsMenuButton, () => ShowScreen(MenuScreen.Achievements));
        BindButton(profileMenuButton, () => ShowScreen(MenuScreen.Profile));
        BindButton(settingsMenuButton, () => ShowScreen(MenuScreen.Settings));
        BindButton(exitMenuButton, QuitGame);
        BindButton(battleButton, PlayGame);
        BindButton(weaponImproveButton, ImprovePreviewedWeapon);
        BindButton(qualityButton, () => { CycleSetting(QualityKey, new[] { "Низкое", "Среднее", "Высокое" }); RefreshSettingsButtonLabels(); });
        BindButton(languageButton, () => { CycleSetting(LanguageKey, new[] { "Русский", "English" }); RefreshSettingsButtonLabels(); });
        BindButton(FindChild<Button>(settingsPanel != null ? settingsPanel.transform : null, "FullscreenButton"), ToggleFullscreen);

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        Button[] backButtons = canvas != null ? canvas.GetComponentsInChildren<Button>(true) : null;

        if (backButtons == null)
        {
            return;
        }

        for (int i = 0; i < backButtons.Length; i++)
        {
            if (backButtons[i] != null && backButtons[i].name == "BackButton")
            {
                BindButton(backButtons[i], () => ShowScreen(MenuScreen.Home));
            }
        }
    }

    private void PopulateAllScreens()
    {
        PopulateMapSlots();
        PopulateCharacterSlots();
        PopulateWeaponSlots();
        RefreshMapDetails();
        RefreshCharacterPreviewPanel();
        RefreshWeaponInfo();
        RefreshSkillTreePanel();
    }

    private void ShowScreen(MenuScreen screen)
    {
        GameObject activePanel = screen switch
        {
            MenuScreen.Home => homePanel,
            MenuScreen.Play => playPanel,
            MenuScreen.Character => characterPanel,
            MenuScreen.Weapon => weaponPanel,
            MenuScreen.SkillTree => skillTreePanel,
            MenuScreen.Runes => runesPanel,
            MenuScreen.Achievements => achievementsPanel,
            MenuScreen.Profile => profilePanel,
            MenuScreen.Settings => settingsPanel,
            _ => null
        };

        CanvasGroup activePanelGroup = activePanel != null ? activePanel.GetComponent<CanvasGroup>() : null;

        if (activePanel != null && activePanelGroup == null)
        {
            Debug.LogWarning($"Main menu panel '{activePanel.name}' is missing a scene-authored CanvasGroup. Fade-in will be skipped.", activePanel);
        }
        else if (activePanelGroup != null)
        {
            activePanelGroup.alpha = 0f;
        }

        SetActivePanel(homePanel, screen == MenuScreen.Home);
        SetActivePanel(playPanel, screen == MenuScreen.Play);
        SetActivePanel(characterPanel, screen == MenuScreen.Character);
        SetActivePanel(weaponPanel, screen == MenuScreen.Weapon);
        SetActivePanel(skillTreePanel, screen == MenuScreen.SkillTree);
        SetActivePanel(runesPanel, screen == MenuScreen.Runes);
        SetActivePanel(achievementsPanel, screen == MenuScreen.Achievements);
        SetActivePanel(profilePanel, screen == MenuScreen.Profile);
        SetActivePanel(settingsPanel, screen == MenuScreen.Settings);

        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        if (activePanel != null)
        {
            activeFadeCoroutine = StartCoroutine(FadeInPanel(activePanel));
        }

        switch (screen)
        {
            case MenuScreen.Play:
                RefreshMapDetails();
                break;
            case MenuScreen.Character:
                RefreshCharacterPreviewPanel();
                break;
            case MenuScreen.Weapon:
                PopulateWeaponSlots();
                break;
            case MenuScreen.SkillTree:
                RefreshSkillTreePanel();
                break;
            case MenuScreen.Runes:
                RefreshMetaPlaceholderPanel(runesPanel, "Руны", BuildRunesSummary());
                break;
            case MenuScreen.Achievements:
                RefreshMetaPlaceholderPanel(achievementsPanel, "Достижения", BuildAchievementsSummary());
                break;
            case MenuScreen.Profile:
                RefreshMetaPlaceholderPanel(profilePanel, "Профиль", BuildProfileSummary());
                break;
            case MenuScreen.Settings:
                LoadSettings();
                break;
        }

        RefreshWorldHero();
    }

    private void RefreshSkillTreePanel()
    {
        if (skillTreeView != null)
        {
            skillTreeView.Refresh();
            return;
        }

        if (skillTreePanel != null)
        {
            Debug.LogWarning("SkillTreePanel has no scene-authored SkillTreePanelView. Run Soulstone -> UI -> Build Scene Skill Tree Panel, or select MainMenuManager and press Build Scene Skill Tree Panel in Inspector.", skillTreePanel);
        }
    }

    private static void RefreshMetaPlaceholderPanel(GameObject panel, string title, string body)
    {
        if (panel == null)
        {
            return;
        }

        SetAnyText(panel.transform, title, "TitleText", "Title", "HeaderText", "PanelTitle");
        SetAnyText(panel.transform, body, "BodyText", "DescriptionText", "InfoText", "TooltipText", "PanelBody");
    }

    private static void SetAnyText(Transform root, string value, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            TMP_Text tmp = FindChild<TMP_Text>(root, names[i]);
            Text legacy = FindChild<Text>(root, names[i]);

            if (tmp != null || legacy != null)
            {
                SetText(tmp, legacy, value);
                return;
            }
        }
    }

    private static string BuildRunesSummary()
    {
        PlayerProfileData profile = SaveSystem.CurrentProfile;
        int owned = profile?.runeProgress?.Count ?? 0;
        int equipped = profile?.equippedRuneIds?.Count ?? 0;

        return $"Рун найдено: {owned}\nЭкипировано: {equipped}\n\nПанель уже scene-authored: добавь слоты рун в Hierarchy, а код будет только обновлять их данные.";
    }

    private static string BuildAchievementsSummary()
    {
        PlayerProfileData profile = SaveSystem.CurrentProfile;
        int count = profile?.achievements?.Count ?? 0;

        return $"Достижений в профиле: {count}\n\nПанель готова для scene-authored списка достижений, наград и locked/owned состояний.";
    }

    private static string BuildProfileSummary()
    {
        PlayerProfileData profile = SaveSystem.CurrentProfile;

        if (profile == null)
        {
            return "Профиль ещё не загружен.";
        }

        return $"Профиль: {profile.profileId}\nПерсонаж: {profile.selectedCharacterId}\nОружие: {profile.selectedWeaponId}\nКарта: {profile.selectedMapId}\n\nОткрыто персонажей: {profile.unlockedCharacterIds?.Count ?? 0}\nОткрыто оружия: {profile.unlockedWeaponIds?.Count ?? 0}\nОткрыто карт: {profile.unlockedMapIds?.Count ?? 0}";
    }

    private static IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        if (group == null)
        {
            yield break;
        }

        const float duration = 0.12f;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / duration;
            yield return null;
        }

        group.alpha = 1f;
    }

    private void PopulateMapSlots()
    {
        if (mapSlots == null)
        {
            return;
        }

        for (int i = 0; i < mapSlots.Length; i++)
        {
            bool hasMap = i < maps.Count;
            mapSlots[i].gameObject.SetActive(hasMap);

            if (!hasMap)
            {
                continue;
            }

            MapEntry map = maps[i];
            mapSlots[i].Configure(map.Name, map.Biome, map.Difficulty, () => OpenMapDetails(map));
            mapSlots[i].SetLocked(!IsMapUnlocked(map), GetMapRequirementText(map));
            mapSlots[i].SetSelected(map.Id == selectedMapId);
        }
    }

    private void PopulateCharacterSlots()
    {
        if (characterSlots == null)
        {
            return;
        }

        for (int i = 0; i < characterSlots.Length; i++)
        {
            bool hasCharacter = i < characters.Count;
            characterSlots[i].gameObject.SetActive(hasCharacter);

            if (!hasCharacter)
            {
                continue;
            }

            CharacterData character = characters[i];
            characterSlots[i].Configure(character, BuildCharacterStats(character), () => PreviewCharacter(character));
            characterSlots[i].SetLocked(!UnlockSystem.IsUnlocked(character), UnlockSystem.GetRequirementText(character));
            characterSlots[i].SetSelected(character == selectedCharacter);
        }
    }

    private void PopulateWeaponSlots()
    {
        if (weaponSlots == null)
        {
            return;
        }

        WeaponData[] weapons = selectedCharacter != null ? selectedCharacter.AvailableWeapons : null;
        int count = weapons != null ? weapons.Length : 0;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            bool hasWeapon = i < count && weapons[i] != null;
            weaponSlots[i].gameObject.SetActive(hasWeapon);

            if (!hasWeapon)
            {
                continue;
            }

            WeaponData weapon = weapons[i];
            weaponSlots[i].Configure(weapon, BuildWeaponDescription(weapon), () => PreviewOrSelectWeapon(weapon));
            weaponSlots[i].SetLocked(!UnlockSystem.IsUnlocked(weapon), UnlockSystem.GetRequirementText(weapon));
            weaponSlots[i].SetSelected(weapon == selectedWeapon);
        }

        RefreshWeaponInfo();
    }

    private void OpenMapDetails(MapEntry map)
    {
        pendingMap = map;
        RefreshMapDetails();
    }

    private void SelectMap(string mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return;
        }

        selectedMapId = mapId;
        SelectedLoadoutStore.SetSelectedMap(mapId);
        PopulateMapSlots();
        RefreshMapDetails();
    }

    private void SelectCharacter(CharacterData character)
    {
        if (character == null)
        {
            return;
        }

        if (!UnlockSystem.IsUnlocked(character))
        {
            PreviewCharacter(character);
            return;
        }

        selectedCharacter = character;
        previewedCharacter = character;
        SelectedCharacterStore.SetSelectedCharacter(character);
        selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(character);
        PopulateCharacterSlots();
        PopulateWeaponSlots();
        RefreshCharacterPreviewPanel();
        RefreshWorldHero();
    }

    private void PreviewCharacter(CharacterData character)
    {
        if (character == null)
        {
            return;
        }

        previewedCharacter = character;
        PopulateCharacterSlots();
        RefreshCharacterPreviewPanel();
    }

    private void SelectWeapon(WeaponData weapon)
    {
        if (selectedCharacter == null || weapon == null)
        {
            return;
        }

        if (!UnlockSystem.IsUnlocked(weapon))
        {
            previewedWeapon = weapon;
            RefreshWeaponInfo();
            return;
        }

        selectedWeapon = weapon;
        previewedWeapon = weapon;
        SelectedLoadoutStore.SetSelectedWeapon(selectedCharacter, weapon);
        PopulateWeaponSlots();
        RefreshWorldHero();
    }

    private void PreviewOrSelectWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            return;
        }

        if (UnlockSystem.IsUnlocked(weapon))
        {
            previewedWeapon = weapon;
            SelectWeapon(weapon);
            return;
        }

        if (previewedWeapon == weapon && UnlockSystem.TryUnlock(weapon))
        {
            SelectWeapon(weapon);
            return;
        }

        previewedWeapon = weapon;
        RefreshWeaponInfo();
    }

    private void ImprovePreviewedWeapon()
    {
        WeaponData weapon = previewedWeapon != null ? previewedWeapon : selectedWeapon;

        if (weapon == null)
        {
            return;
        }

        if (ForgeSystem.TryImprove(weapon))
        {
            if (weapon == selectedWeapon)
            {
                SelectedLoadoutStore.SetSelectedWeapon(selectedCharacter, weapon);
            }

            PopulateWeaponSlots();
            RefreshWeaponInfo();
        }
        else
        {
            RefreshWeaponInfo();
        }
    }

    private void ConfirmPreviewedCharacter()
    {
        if (previewedCharacter == null)
        {
            return;
        }

        if (!UnlockSystem.IsUnlocked(previewedCharacter))
        {
            if (UnlockSystem.TryUnlock(previewedCharacter))
            {
                SelectCharacter(previewedCharacter);
            }
            else
            {
                RefreshCharacterPreviewPanel();
            }

            return;
        }

        SelectCharacter(previewedCharacter);
    }

    private void RefreshMapDetails()
    {
        if (string.IsNullOrWhiteSpace(pendingMap.Id))
        {
            pendingMap = FindMap(selectedMapId);
        }

        bool mapUnlocked = IsMapUnlocked(pendingMap);
        string lockText = mapUnlocked ? string.Empty : $"\n\nНЕДОСТУПНО\n{GetMapRequirementText(pendingMap)}";

        SetText(mapDetailsText, legacyMapDetailsText,
            $"{pendingMap.Name}\n" +
            $"Сложность: {pendingMap.Difficulty}\n" +
            $"Биом: {pendingMap.Biome}\n\n" +
            $"Враги: {pendingMap.Enemies}\n" +
            $"Награды: {pendingMap.Rewards}" +
            lockText);

        RefreshMapPreviewImage(pendingMap.PreviewImage);
        BindButton(mapSelectButton, ConfirmPendingMap);
        SetButtonLabel(mapSelectButton, mapUnlocked ? "ВЫБРАТЬ" : "РАЗБЛОКИРОВАТЬ");

        if (mapSelectButton != null)
        {
            mapSelectButton.interactable = !string.IsNullOrWhiteSpace(pendingMap.Id);
        }

        if (battleButton != null)
        {
            battleButton.interactable = !string.IsNullOrWhiteSpace(selectedMapId) && IsMapUnlocked(FindMap(selectedMapId));
        }
    }

    private void ConfirmPendingMap()
    {
        if (string.IsNullOrWhiteSpace(pendingMap.Id))
        {
            return;
        }

        if (IsMapUnlocked(pendingMap))
        {
            SelectMap(pendingMap.Id);
            return;
        }

        if (pendingMap.Data != null && UnlockSystem.TryUnlock(pendingMap.Data))
        {
            SelectMap(pendingMap.Id);
            return;
        }

        RefreshMapDetails();
    }

    private void RefreshMapPreviewImage(Sprite sprite)
    {
        if (playPanel == null)
        {
            return;
        }

        Transform previewImg = FindChildRecursive(playPanel.transform, "MapPreviewImage");

        if (previewImg == null)
        {
            return;
        }

        Image img = previewImg.GetComponent<Image>();

        if (img != null)
        {
            img.sprite = sprite;
            img.enabled = sprite != null;
        }
    }

    private void RefreshCharacterPreviewPanel()
    {
        if (previewedCharacter == null)
        {
            previewedCharacter = selectedCharacter != null ? selectedCharacter : (characters.Count > 0 ? characters[0] : null);
        }

        bool unlocked = previewedCharacter != null && UnlockSystem.IsUnlocked(previewedCharacter);
        SetText(characterPreviewTitleText, legacyCharacterPreviewTitleText, previewedCharacter != null ? previewedCharacter.DisplayName : "Персонаж");
        SetText(characterPreviewBodyText, legacyCharacterPreviewBodyText, unlocked
            ? BuildCharacterPreview(previewedCharacter)
            : $"{BuildCharacterPreview(previewedCharacter)}\n\nНЕДОСТУПНО\n{UnlockSystem.GetRequirementText(previewedCharacter)}");

        BindButton(characterSelectButton, ConfirmPreviewedCharacter);
        SetButtonLabel(characterSelectButton, unlocked ? "ВЫБРАТЬ" : "РАЗБЛОКИРОВАТЬ");

        if (characterSelectButton != null)
        {
            characterSelectButton.interactable = previewedCharacter != null && (!unlocked || previewedCharacter != selectedCharacter);
        }
    }

    private void RefreshWeaponInfo()
    {
        if (weaponInfoText == null && legacyWeaponInfoText == null)
        {
            return;
        }

        WeaponData weaponToShow = previewedWeapon != null ? previewedWeapon : selectedWeapon;
        bool unlocked = weaponToShow == null || UnlockSystem.IsUnlocked(weaponToShow);
        WeaponProgressData progress = weaponToShow != null ? ForgeSystem.GetProgress(weaponToShow) : null;
        bool canImprove = weaponToShow != null && unlocked && ForgeSystem.CanImprove(weaponToShow);

        SetText(weaponInfoText, legacyWeaponInfoText, weaponToShow != null
            ? $"{(weaponToShow == selectedWeapon ? "Выбрано" : "Оружие")}: {weaponToShow.DisplayName}\n\n{BuildWeaponDescription(weaponToShow)}" +
              (unlocked ? $"\n\nКузница: уровень {progress?.level ?? 0}/{weaponToShow.MaxLevel}\n{ForgeSystem.BuildCostLine(weaponToShow)}" : $"\n\nНЕДОСТУПНО\n{UnlockSystem.GetRequirementText(weaponToShow)}\n\nНажми на карточку ещё раз, чтобы разблокировать.")
            : "Выбери оружие для текущего персонажа.");

        if (weaponImproveButton != null)
        {
            weaponImproveButton.interactable = canImprove;
            SetButtonLabel(weaponImproveButton, canImprove ? "УЛУЧШИТЬ" : "НЕЛЬЗЯ УЛУЧШИТЬ");
        }
    }

    private void RefreshWorldHero()
    {
        if (heroWorldRoot != null)
        {
            heroPreviewBuilder.Refresh(heroWorldRoot, selectedCharacter, accentColor);
        }
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
        RefreshSettingsButtonLabels();
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

            maps.Add(new MapEntry(map.MapId, map.DisplayName, map.Difficulty, map.Biome, map.EnemyDescription, map.RewardsDescription, map.PreviewImage, map));
        }
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
            selectedCharacter = characters[0];
        }
    }

    private MapEntry FindMap(string id)
    {
        for (int i = 0; i < maps.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(id) && maps[i].Id == id)
            {
                return maps[i];
            }
        }

        return maps.Count > 0 ? maps[0] : default;
    }

    private static bool IsMapUnlocked(MapEntry map)
    {
        if (string.IsNullOrWhiteSpace(map.Id))
        {
            return false;
        }

        return map.Data != null
            ? UnlockSystem.IsUnlocked(map.Data)
            : UnlockSystem.IsMapUnlocked(map.Id);
    }

    private static string GetMapRequirementText(MapEntry map)
    {
        if (map.Data != null)
        {
            return UnlockSystem.GetRequirementText(map.Data);
        }

        return "Карта пока заблокирована.";
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
        if (weapon == null)
        {
            return string.Empty;
        }

        string skill = weapon.StartingSkill != null ? weapon.StartingSkill.DisplayName : "нет";
        int poolSize = weapon.UniqueSkillPool != null ? weapon.UniqueSkillPool.Length : 0;
        string specialSkill = poolSize > 0 && weapon.UniqueSkillPool[0] != null ? weapon.UniqueSkillPool[0].DisplayName : "нет";
        string characterBonuses = BuildStatModifierLine("Бонусы персонажа/ур.", weapon.StatGrowthPerLevel);
        string skillBonuses = BuildStatModifierLine("Бонус стартового навыка/ур.", weapon.SkillGrowthPerLevel);

        return $"Стартовый навык: {skill}\nОсобый навык: {specialSkill}\nНавыков в пуле: {poolSize}{characterBonuses}{skillBonuses}";
    }

    private static string BuildStatModifierLine(string label, StatModifierData[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
        {
            return string.Empty;
        }

        List<string> parts = new();

        for (int i = 0; i < modifiers.Length; i++)
        {
            string value = modifiers[i].multiplier != 0f
                ? $"+{modifiers[i].multiplier:P0}"
                : $"+{modifiers[i].additive:0.##}";
            parts.Add($"{modifiers[i].statType} {value}");
        }

        return $"\n{label}: {string.Join(", ", parts)}";
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
    }

    private void RefreshSettingsButtonLabels()
    {
        string quality = PlayerPrefs.GetString(QualityKey, "Среднее");
        string language = PlayerPrefs.GetString(LanguageKey, "Русский");
        SetButtonLabel(qualityButton, $"Качество: {quality}");
        SetButtonLabel(languageButton, $"Язык: {language}");
    }

    private static void SetButtonLabel(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
        {
            label.text = text;
            ApplyTmpButtonLabelStyle(label);
        }

        Text legacyLabel = button.GetComponentInChildren<Text>(true);

        if (legacyLabel != null)
        {
            legacyLabel.text = text;
            ApplyLegacyButtonLabelStyle(legacyLabel);
        }
    }

    private static void SetText(TMP_Text tmpText, Text legacyText, string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
        }

        if (legacyText != null)
        {
            legacyText.text = value;
        }
    }

    private void ToggleFullscreen()
    {
        bool enabled = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 1;
        PlayerPrefs.SetInt(FullscreenKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        Screen.fullScreen = enabled;
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (action != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void SetActivePanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void NormalizeRuntimeTextStyle()
    {
        if (canvas == null)
        {
            return;
        }

        foreach (Text text in canvas.GetComponentsInChildren<Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            ApplyLegacyTextStyle(text);
        }

        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null)
            {
                continue;
            }

            ApplyTmpTextStyle(text);
        }
    }

    private static void ApplyLegacyTextStyle(Text text)
    {
        string lowerName = text.name.ToLowerInvariant();
        bool isButtonLabel = lowerName.Contains("label") || IsInsideButton(text.transform);
        bool isTitle = lowerName.Contains("title") || lowerName.Contains("slot");

        text.raycastTarget = false;

        if (isButtonLabel)
        {
            ApplyLegacyButtonLabelStyle(text);
            return;
        }

        if (isTitle)
        {
            text.color = new Color(0.98f, 0.92f, 0.72f, 1f);
            text.fontSize = Mathf.Max(text.fontSize, 18);
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.UpperCenter;
            return;
        }

        text.color = new Color(0.92f, 0.92f, 0.88f, 0.96f);
    }

    private static void ApplyTmpTextStyle(TMP_Text text)
    {
        string lowerName = text.name.ToLowerInvariant();
        bool isButtonLabel = lowerName.Contains("label") || IsInsideButton(text.transform);
        bool isTitle = lowerName.Contains("title") || lowerName.Contains("slot");

        text.raycastTarget = false;

        if (isButtonLabel)
        {
            ApplyTmpButtonLabelStyle(text);
            return;
        }

        if (isTitle)
        {
            text.color = new Color(0.98f, 0.92f, 0.72f, 1f);
            text.fontSize = Mathf.Max(text.fontSize, 18f);
            text.fontStyle = FontStyles.Bold;
            return;
        }

        text.color = new Color(0.92f, 0.92f, 0.88f, 0.96f);
    }

    private static void ApplyLegacyButtonLabelStyle(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.raycastTarget = false;
        text.color = new Color(1f, 0.92f, 0.62f, 1f);
        text.fontSize = Mathf.Max(text.fontSize, 22);
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.transform.SetAsLastSibling();
    }

    private static void ApplyTmpButtonLabelStyle(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.raycastTarget = false;
        text.color = new Color(1f, 0.92f, 0.62f, 1f);
        text.fontSize = Mathf.Max(text.fontSize, 22f);
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.transform.SetAsLastSibling();
    }

    private static bool IsInsideButton(Transform transform)
    {
        Transform current = transform != null ? transform.parent : null;

        while (current != null)
        {
            if (current.GetComponent<Button>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void ConfigureCanvas(Canvas targetCanvas)
    {
        if (targetCanvas == null)
        {
            return;
        }

        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = targetCanvas.GetComponent<CanvasScaler>();

        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static T FindChild<T>(Transform parent, string childName) where T : Component
    {
        Transform child = FindChildRecursive(parent, childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static GameObject FindPage(Transform parent, string childName)
    {
        Transform child = parent != null ? parent.Find(childName) : null;
        return child != null ? child.gameObject : null;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
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
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);

            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = color.a > 0.01f;
        return panel;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private GameObject EnsurePage(Transform pagesRoot, string name)
    {
        Transform existing = pagesRoot.Find(name);

        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject page = CreatePanel(name, pagesRoot, new Color(0f, 0f, 0f, 0f));
        RectTransform rect = page.GetComponent<RectTransform>();
        Stretch(rect);
        return page;
    }

    private Transform EnsurePagesRoot(Transform root)
    {
        Transform existing = root.Find("Pages");

        if (existing != null)
        {
            return existing;
        }

        GameObject pages = CreatePanel("Pages", root, new Color(0f, 0f, 0f, 0f));
        RectTransform rect = pages.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(340f, 80f);
        rect.offsetMax = new Vector2(-70f, -190f);
        return pages.transform;
    }

    private void EnsureLeftMenu(Transform root)
    {
        if (root.Find("MenuPanel") != null || root.Find("LeftMenu") != null)
        {
            return;
        }

        GameObject rail = CreatePanel("LeftMenu", root, new Color(0.015f, 0.02f, 0.035f, 0.70f));
        RectTransform rect = rail.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.offsetMin = new Vector2(42f, 190f);
        rect.offsetMax = new Vector2(360f, -170f);

        Outline outline = rail.AddComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(4f, -4f);

        TMP_Text logo = CreateText("Logo", rail.transform, "SOULSTONE\nPROTOTYPE", 30, FontStyles.Bold);
        logo.rectTransform.anchorMin = new Vector2(0f, 1f);
        logo.rectTransform.anchorMax = new Vector2(1f, 1f);
        logo.rectTransform.offsetMin = new Vector2(26f, -118f);
        logo.rectTransform.offsetMax = new Vector2(-26f, -24f);
        logo.alignment = TextAlignmentOptions.Center;
        logo.color = accentColor;

        GameObject menu = new("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        menu.transform.SetParent(rail.transform, false);
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

        CreateSceneButton("PlayMenuButton", menu.transform, "Играть", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
        CreateSceneButton("CharacterMenuButton", menu.transform, "Персонаж", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
        CreateSceneButton("WeaponMenuButton", menu.transform, "Оружие", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
        CreateSceneButton("SkillTreeMenuButton", menu.transform, "Дерево навыков", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
        CreateSceneButton("SettingsMenuButton", menu.transform, "Настройки", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
        CreateSceneButton("ExitMenuButton", menu.transform, "Выход", new Vector2(0f, 0f), new Vector2(250f, 48f), false);
    }

    private void EnsurePlayPanel(Transform panel)
    {
        // Migrate: remove old flat MapGrid if it exists directly under panel (pre-scroll layout)
        Transform oldGrid = panel.Find("MapGrid");
        if (oldGrid != null && panel.Find("MapScrollView") == null)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(oldGrid.gameObject);
#endif
        }

        GameObject scrollView = EnsureHorizontalScrollView(panel, "MapScrollView",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -250f), new Vector2(0f, -20f));

        Transform grid = EnsureScrollContent(scrollView.transform, "MapGrid");

        for (int i = 0; i < MapSlotCount; i++)
        {
            EnsureCard<MapCardView>(grid, $"MapSlot_{i + 1}", new Vector2(210f, 210f));
        }

        GameObject details = EnsureInfoPanel(panel, "MapDetails", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 120f), new Vector2(0f, 360f));
        TMP_Text detailsText = EnsureChildText(details.transform, "MapDetailsText", "Выбери карту в карусели выше.", 22, TextAlignmentOptions.TopLeft);
        detailsText.rectTransform.offsetMin = new Vector2(28f, 28f);
        detailsText.rectTransform.offsetMax = new Vector2(-280f, -24f);
        EnsureMapPreviewImage(details.transform);
        CreateSceneButton("MapSelectButton", details.transform, "ВЫБРАТЬ", new Vector2(-28f, 24f), new Vector2(220f, 54f), true);
        CreateSceneButton("BattleButton", panel, "В БОЙ", new Vector2(0f, 36f), new Vector2(260f, 64f), false);
        CreateSceneButton("BackButton", panel, "НАЗАД", new Vector2(-300f, 36f), new Vector2(220f, 56f), false);
    }

    private void EnsureCharacterPanel(Transform panel)
    {
        GameObject scrollView = EnsureInfoPanel(panel, "CharacterScrollView", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-430f, 0f));
        Image image = scrollView.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = false;

        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>() ?? scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 34f;

        Transform viewport = scrollView.transform.Find("Viewport");

        if (viewport == null)
        {
            GameObject viewportObject = new("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollView.transform, false);
            viewport = viewportObject.transform;
            Stretch(viewport.GetComponent<RectTransform>());
        }

        Transform grid = viewport.Find("CharacterGrid");

        if (grid == null)
        {
            GameObject gridObject = new("CharacterGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            gridObject.transform.SetParent(viewport, false);
            grid = gridObject.transform;
        }

        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

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
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = gridRect;

        for (int i = 0; i < CharacterSlotCount; i++)
        {
            EnsureCard<CharacterCardView>(grid, $"CharacterSlot_{i + 1}", new Vector2(220f, 230f));
        }

        GameObject preview = EnsureInfoPanel(panel, "CharacterPreviewPanel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-195f, 0f), new Vector2(390f, 520f));
        TMP_Text title = EnsureChildText(preview.transform, "CharacterPreviewTitle", "Персонаж", 26, TextAlignmentOptions.Top);
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.offsetMin = new Vector2(22f, -70f);
        title.rectTransform.offsetMax = new Vector2(-22f, -20f);
        TMP_Text body = EnsureChildText(preview.transform, "CharacterPreviewBody", "Выбери героя.", 20, TextAlignmentOptions.TopLeft);
        body.rectTransform.anchorMin = Vector2.zero;
        body.rectTransform.anchorMax = Vector2.one;
        body.rectTransform.offsetMin = new Vector2(24f, 96f);
        body.rectTransform.offsetMax = new Vector2(-24f, -90f);
        CreateSceneButton("CharacterSelectButton", preview.transform, "ВЫБРАТЬ", new Vector2(0f, 28f), new Vector2(250f, 56f), false);
        CreateSceneButton("BackButton", panel, "НАЗАД", new Vector2(-300f, 36f), new Vector2(220f, 56f), false);
    }

    private void EnsureWeaponPanel(Transform panel)
    {
        // Migrate: remove old flat WeaponGrid if it exists directly under panel (pre-scroll layout)
        Transform oldGrid = panel.Find("WeaponGrid");
        if (oldGrid != null && panel.Find("WeaponScrollView") == null)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(oldGrid.gameObject);
#endif
        }

        GameObject scrollView = EnsureHorizontalScrollView(panel, "WeaponScrollView",
            Vector2.zero, Vector2.one,
            Vector2.zero, new Vector2(-430f, 0f));

        Transform grid = EnsureScrollContent(scrollView.transform, "WeaponGrid");

        for (int i = 0; i < WeaponSlotCount; i++)
        {
            EnsureCard<WeaponCardView>(grid, $"WeaponSlot_{i + 1}", new Vector2(260f, 280f));
        }

        GameObject info = EnsureInfoPanel(panel, "WeaponInfoPanel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-195f, 0f), new Vector2(390f, 330f));
        TMP_Text infoText = EnsureChildText(info.transform, "WeaponInfoText", "Выбранное оружие привязывается к текущему персонажу.", 22, TextAlignmentOptions.TopLeft);
        infoText.rectTransform.offsetMin = new Vector2(24f, 24f);
        infoText.rectTransform.offsetMax = new Vector2(-24f, -24f);
        CreateSceneButton("BackButton", panel, "НАЗАД", new Vector2(-300f, 36f), new Vector2(220f, 56f), false);
    }

    private void EnsureSkillTreePanel(Transform panel)
    {
        if (panel.Find("SkillTreeText") != null)
        {
            return;
        }

        TMP_Text text = CreateText("SkillTreeText", panel, "Дерево навыков\n\nУровень 1 -> +HP\nУровень 2 -> +Шанс редкости\nУровень 3 -> +Стартовый уровень\nУровень 4 -> +Пул навыков\n\nПока это визуальная заготовка для будущей мета-прогрессии.", 26, FontStyles.Bold);
        Stretch(text.rectTransform);
        text.alignment = TextAlignmentOptions.Center;
        CreateSceneButton("BackButton", panel, "НАЗАД", new Vector2(-300f, 36f), new Vector2(220f, 56f), false);
    }

    private void EnsureSettingsPanel(Transform panel)
    {
        EnsureSlider(panel, "MasterVolumeSlider", "Общая громкость", new Vector2(0f, 160f));
        EnsureSlider(panel, "MusicVolumeSlider", "Музыка", new Vector2(0f, 80f));
        EnsureSlider(panel, "SfxVolumeSlider", "Эффекты", new Vector2(0f, 0f));
        CreateSceneButton("QualityButton", panel, "Качество: Среднее", new Vector2(-260f, -110f), new Vector2(420f, 56f), false);
        CreateSceneButton("LanguageButton", panel, "Язык: Русский", new Vector2(260f, -110f), new Vector2(420f, 56f), false);
        CreateSceneButton("FullscreenButton", panel, "Полный экран", new Vector2(-260f, -210f), new Vector2(420f, 56f), false);
        CreateSceneButton("BackButton", panel, "НАЗАД", new Vector2(-300f, 36f), new Vector2(220f, 56f), false);

    }

    private GameObject EnsureInfoPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeOrOffsetMax)
    {
        Transform existing = parent.Find(name);
        GameObject panel = existing != null ? existing.gameObject : CreatePanel(name, parent, panelColor);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);

        if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
        {
            rect.offsetMin = anchoredPosition;
            rect.offsetMax = sizeOrOffsetMax;
        }
        else
        {
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeOrOffsetMax;
        }

        return panel;
    }

    private static void EnsureMapPreviewImage(Transform detailsPanel)
    {
        const string previewName = "MapPreviewImage";

        if (detailsPanel.Find(previewName) != null)
        {
            return;
        }

        GameObject previewObject = new(previewName, typeof(RectTransform), typeof(Image));
        previewObject.transform.SetParent(detailsPanel, false);

        RectTransform rect = previewObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-28f, 0f);
        rect.sizeDelta = new Vector2(220f, 220f);

        Image img = previewObject.GetComponent<Image>();
        img.color = new Color(0.17f, 0.18f, 0.21f, 1f);
        img.enabled = false;
    }

    private static GameObject EnsureHorizontalScrollView(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        Transform existing = parent.Find(name);
        GameObject scrollObject = existing != null ? existing.gameObject : new(name, typeof(RectTransform), typeof(Image));
        scrollObject.transform.SetParent(parent, false);

        Image img = scrollObject.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.raycastTarget = false;

        RectTransform rect = scrollObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);

        if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
        {
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
        else
        {
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>() ?? scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.scrollSensitivity = 40f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;

        Transform viewport = scrollObject.transform.Find("Viewport");

        if (viewport == null)
        {
            GameObject viewportObject = new("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            viewport = viewportObject.transform;
        }

        Stretch(viewport.GetComponent<RectTransform>());
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        return scrollObject;
    }

    private static Transform EnsureScrollContent(Transform scrollView, string gridName)
    {
        Transform viewport = scrollView.Find("Viewport");
        Transform grid = viewport != null ? viewport.Find(gridName) : null;

        if (grid == null)
        {
            GameObject gridObject = new(gridName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            gridObject.transform.SetParent(viewport ?? scrollView, false);
            grid = gridObject.transform;
        }

        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(0f, 1f);
        gridRect.pivot = new Vector2(0f, 0.5f);
        gridRect.offsetMin = new Vector2(8f, 0f);
        gridRect.offsetMax = new Vector2(8f, 0f);

        HorizontalLayoutGroup layout = grid.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            scrollRect.content = gridRect;
        }

        return grid;
    }

    private void EnsureGrid(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool horizontal, float spacing)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
        {
            return;
        }

        GameObject grid = new(name, typeof(RectTransform));
        grid.transform.SetParent(parent, false);
        RectTransform rect = grid.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        HorizontalOrVerticalLayoutGroup layout = horizontal
            ? grid.AddComponent<HorizontalLayoutGroup>()
            : grid.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private T EnsureCard<T>(Transform parent, string name, Vector2 size) where T : MenuCardViewBase
    {
        Transform existing = parent.Find(name);
        GameObject card = existing != null ? existing.gameObject : CreatePanel(name, parent, cardColor);
        T view = card.GetComponent<T>() ?? card.AddComponent<T>();
        Button button = card.GetComponent<Button>() ?? card.AddComponent<Button>();
        button.targetGraphic = card.GetComponent<Image>();

        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        LayoutElement layout = card.GetComponent<LayoutElement>() ?? card.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;

        TMP_Text title = EnsureChildText(card.transform, "Title", name, 22, TextAlignmentOptions.Top);
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.offsetMin = new Vector2(12f, -60f);
        title.rectTransform.offsetMax = new Vector2(-12f, -12f);

        TMP_Text body = EnsureChildText(card.transform, "Body", string.Empty, 16, TextAlignmentOptions.Center);
        body.rectTransform.anchorMin = Vector2.zero;
        body.rectTransform.anchorMax = Vector2.one;
        body.rectTransform.offsetMin = new Vector2(12f, 16f);
        body.rectTransform.offsetMax = new Vector2(-12f, -70f);
        return view;
    }

    private TMP_Text EnsureChildText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        TMP_Text label = existing != null ? existing.GetComponent<TMP_Text>() : null;

        if (label == null)
        {
            label = CreateText(name, parent, text, fontSize, FontStyles.Normal);
            Stretch(label.rectTransform);
        }

        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        return label;
    }

    private Button CreateSceneButton(string name, Transform parent, string label, Vector2 position, Vector2 size, bool bottomRight)
    {
        Transform existing = parent.Find(name);
        GameObject buttonObject = existing != null ? existing.gameObject : CreatePanel(name, parent, accentColor);
        Button button = buttonObject.GetComponent<Button>() ?? buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = bottomRight ? new Vector2(1f, 0f) : new Vector2(0.5f, 0f);
        rect.anchorMax = bottomRight ? new Vector2(1f, 0f) : new Vector2(0.5f, 0f);
        rect.pivot = bottomRight ? new Vector2(1f, 0f) : new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text text = EnsureChildText(buttonObject.transform, "Label", label, 22, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.color = new Color(0.96f, 0.86f, 0.55f, 1f);
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private void EnsureSlider(Transform parent, string name, string label, Vector2 position)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
        {
            return;
        }

        TMP_Text labelText = CreateText(name + "_Label", parent, label, 24, FontStyles.Bold);
        labelText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        labelText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        labelText.rectTransform.anchoredPosition = position + new Vector2(-340f, 0f);
        labelText.rectTransform.sizeDelta = new Vector2(260f, 40f);
        labelText.alignment = TextAlignmentOptions.Right;

        GameObject sliderObject = new(name, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(440f, 30f);
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
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
}


using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DDRunManager : MonoBehaviour
{
    [SerializeField] private CharacterSystem characterSystem;
    [SerializeField] private SpawnSystem spawnSystem;
    [SerializeField] private RunAffixData[] affixPool;
    [SerializeField] private int affixesPerRun = 2;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverSummaryText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victorySummaryText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private bool beginRunOnStart = true;
    [SerializeField] private bool enableDebugKillHotkey = true;
    [SerializeField] private KeyCode debugKillHotkey = KeyCode.K;
    [Header("Map Spawn")]
    [SerializeField] private float fallbackPlayerSpawnY = 1.08f;
    [SerializeField] private int spawnSearchRings = 5;
    [SerializeField] private int spawnSearchSamplesPerRing = 12;
    [Header("Map Ownership")]
    [SerializeField] private bool hideLegacySceneMapPlaceholders = true;
    [Header("Meta Progression")]
    [SerializeField] private SkillTreeData skillTreeData;

    private ProgressionSystem progressionSystem;
    private MapData currentMap;
    private GameObject currentMapInstance;
    private DDRunState currentState = DDRunState.Initializing;
    private float elapsedRunTime;
    private readonly List<RunAffixData> activeAffixes = new();
    private bool hasGrantedRunRewards;
    private RunRewardBreakdown lastRewardBreakdown;

    public event Action<DDRunState> StateChanged;
    public event Action<float> ElapsedTimeChanged;
    public event Action ActiveAffixesChanged;
    public event Action<RunRewardBreakdown> RunRewardsGranted;

    public DDRunState CurrentState => currentState;
    public float ElapsedRunTime => elapsedRunTime;
    public bool IsGameplayRunning => currentState == DDRunState.Playing;
    public bool IsTerminalState => currentState == DDRunState.GameOver || currentState == DDRunState.Victory;
    public IReadOnlyList<RunAffixData> ActiveAffixes => activeAffixes;
    public MapData CurrentMap => currentMap;
    public RunRewardBreakdown LastRewardBreakdown => lastRewardBreakdown;
    public float CurrentSpawnRateMultiplier => GetAggregateMultiplier(affix => affix.SpawnRateMultiplier);
    public int CurrentPackSizeBonus => GetAggregateBonus(affix => affix.PackSizeBonus);
    public int CurrentMaxAliveBonus => GetAggregateBonus(affix => affix.MaxAliveBonus);
    public float CurrentEnemyHealthMultiplier => GetAggregateMultiplier(affix => affix.EnemyHealthMultiplier);
    public float CurrentEnemyDamageMultiplier => GetAggregateMultiplier(affix => affix.EnemyDamageMultiplier);
    public float CurrentExperienceMultiplier => GetAggregateMultiplier(affix => affix.ExperienceMultiplier);

    private void Start()
    {
        ResolveReferences();
        SkillTreeSystem.SetActiveTree(skillTreeData);
        SkillTreeSystem.ApplyGlobalBonuses(characterSystem != null ? characterSystem.RuntimeStats : null, skillTreeData);
        progressionSystem?.ApplyAccountStartingLevelBonus(skillTreeData);
        ApplySelectedMap();
        Subscribe();
        HideGameOverPanel();

        if (beginRunOnStart)
        {
            BeginRun();
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        HandleDebugInput();

        if (currentState != DDRunState.Playing)
        {
            return;
        }

        elapsedRunTime += Time.deltaTime;
        ElapsedTimeChanged?.Invoke(elapsedRunTime);

        if (spawnSystem != null && spawnSystem.NormalizedTime >= 1f)
        {
            TriggerVictory();
        }
    }

    public void BeginRun()
    {
        elapsedRunTime = 0f;
        hasGrantedRunRewards = false;
        lastRewardBreakdown = null;
        RollRunAffixes();
        ElapsedTimeChanged?.Invoke(elapsedRunTime);
        SetHudVisible(true);
        HideGameOverPanel();
        HideVictoryPanel();
        HidePausePanel();
        SetState(DDRunState.Playing);
    }

    public void EnterLevelUpSelection()
    {
        if (IsTerminalState)
        {
            return;
        }

        SetState(DDRunState.LevelUpSelection);
    }

    public void ResumeGameplay()
    {
        if (IsTerminalState)
        {
            return;
        }

        SetState(DDRunState.Playing);
    }

    public void TriggerVictory()
    {
        if (IsTerminalState)
        {
            return;
        }

        GrantRunRewards(isVictory: true);
        UpdateVictorySummary();
        SetHudVisible(false);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        SetState(DDRunState.Victory);
    }

    public void PauseRun()
    {
        if (currentState != DDRunState.Playing)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        SetState(DDRunState.Paused);
    }

    public void UnpauseRun()
    {
        if (currentState != DDRunState.Paused)
        {
            return;
        }

        HidePausePanel();
        SetState(DDRunState.Playing);
    }

    public void TriggerGameOver()
    {
        if (currentState == DDRunState.GameOver)
        {
            return;
        }

        GrantRunRewards(isVictory: false);
        UpdateGameOverSummary();
        SetHudVisible(false);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        SetState(DDRunState.GameOver);
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReloadCurrentScene()
    {
        RestartRun();
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void DebugKillPlayer()
    {
        TriggerGameOver();
    }

    private void ResolveReferences()
    {
        if (characterSystem == null)
        {
            characterSystem = FindFirstObjectByType<CharacterSystem>();
        }

        if (spawnSystem == null)
        {
            spawnSystem = FindFirstObjectByType<SpawnSystem>();
        }

        if (characterSystem != null)
        {
            progressionSystem = characterSystem.ProgressionSystem;
        }
    }

    private void ApplySelectedMap()
    {
        currentMap = SelectedLoadoutStore.ResolveSelectedMap();

        if (currentMap == null)
        {
            return;
        }

        if (currentMap.MapPrefab != null && currentMapInstance == null)
        {
            HideLegacySceneMapPlaceholders();
            currentMapInstance = Instantiate(currentMap.MapPrefab);
            currentMapInstance.name = currentMap.MapPrefab.name;
        }
        else if (currentMap.MapPrefab == null)
        {
            Debug.LogWarning($"Map '{currentMap.DisplayName}' has no map prefab. Gameplay will use whatever environment already exists in the scene.", this);
        }

        PlacePlayerAtMapSpawn();

        if (spawnSystem != null)
        {
            spawnSystem.ApplyMapData(currentMap);
        }
    }

    private void HideLegacySceneMapPlaceholders()
    {
        if (!hideLegacySceneMapPlaceholders)
        {
            return;
        }

        GameObject[] roots = gameObject.scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];

            if (root == null || root == gameObject || root == currentMapInstance || root.name == currentMap.MapPrefab.name)
            {
                continue;
            }

            if (root.name == "Ground" || root.name.StartsWith("Cube", StringComparison.OrdinalIgnoreCase))
            {
                root.SetActive(false);
            }
        }
    }

    private void PlacePlayerAtMapSpawn()
    {
        if (characterSystem == null || currentMap == null)
        {
            return;
        }

        Vector3 spawnPosition = currentMap.PlayerSpawnPosition;

        if (spawnPosition.y <= 0.01f)
        {
            spawnPosition.y = fallbackPlayerSpawnY;
        }

        spawnPosition = ResolveSafePlayerSpawnPosition(spawnPosition, currentMap.PlayerSpawnClearRadius);

        CharacterController controller = characterSystem.GetComponent<CharacterController>();
        bool wasControllerEnabled = controller != null && controller.enabled;

        if (controller != null)
        {
            controller.enabled = false;
        }

        characterSystem.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

        if (controller != null)
        {
            controller.enabled = wasControllerEnabled;
        }
    }

    private Vector3 ResolveSafePlayerSpawnPosition(Vector3 preferredPosition, float clearRadius)
    {
        if (IsPlayerSpawnClear(preferredPosition, clearRadius))
        {
            return preferredPosition;
        }

        int rings = Mathf.Max(1, spawnSearchRings);
        int samplesPerRing = Mathf.Max(6, spawnSearchSamplesPerRing);

        for (int ring = 1; ring <= rings; ring++)
        {
            float searchRadius = clearRadius * ring;

            for (int sample = 0; sample < samplesPerRing; sample++)
            {
                float angle = sample * Mathf.PI * 2f / samplesPerRing;
                Vector3 candidate = preferredPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * searchRadius;

                if (IsPlayerSpawnClear(candidate, clearRadius))
                {
                    return candidate;
                }
            }
        }

        Debug.LogWarning($"No clear player spawn found for map '{currentMap.DisplayName}'. Using configured spawn position.");
        return preferredPosition;
    }

    private bool IsPlayerSpawnClear(Vector3 position, float clearRadius)
    {
        CharacterController controller = characterSystem != null ? characterSystem.GetComponent<CharacterController>() : null;
        float capsuleRadius = controller != null ? Mathf.Max(controller.radius + 0.1f, clearRadius * 0.5f) : clearRadius;
        float capsuleHeight = controller != null ? Mathf.Max(controller.height, capsuleRadius * 2f) : 2f;
        float capsuleCenterY = controller != null ? controller.center.y : capsuleHeight * 0.5f;
        Vector3 bottom = position + Vector3.up * (capsuleCenterY - capsuleHeight * 0.5f + capsuleRadius);
        Vector3 top = position + Vector3.up * (capsuleCenterY + capsuleHeight * 0.5f - capsuleRadius);
        Collider[] overlaps = Physics.OverlapCapsule(bottom, top, capsuleRadius, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider hit = overlaps[i];

            if (hit == null || IsOwnPlayerCollider(hit) || IsGroundCollider(hit))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private bool IsOwnPlayerCollider(Collider hit)
    {
        return characterSystem != null && hit.transform.IsChildOf(characterSystem.transform);
    }

    private static bool IsGroundCollider(Collider hit)
    {
        Transform current = hit.transform;

        while (current != null)
        {
            if (current.name == "Ground")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void Subscribe()
    {
        if (characterSystem != null)
        {
            characterSystem.Died -= HandlePlayerDied;
            characterSystem.Died += HandlePlayerDied;
        }

        if (spawnSystem != null)
        {
            spawnSystem.BossEnemyKilled -= HandleBossKilled;
            spawnSystem.BossEnemyKilled += HandleBossKilled;
        }
    }

    private void Unsubscribe()
    {
        if (characterSystem != null)
        {
            characterSystem.Died -= HandlePlayerDied;
        }

        if (spawnSystem != null)
        {
            spawnSystem.BossEnemyKilled -= HandleBossKilled;
        }
    }

    private void HandlePlayerDied()
    {
        TriggerGameOver();
    }

    private void HandleBossKilled(EnemyAgent enemy)
    {
        if (!IsTerminalState && spawnSystem != null && spawnSystem.HadBossesSpawned && spawnSystem.ActiveBossCount <= 0)
        {
            TriggerVictory();
        }
    }

    private void HandleDebugInput()
    {
        if (!enableDebugKillHotkey || IsTerminalState)
        {
            return;
        }

        if (Input.GetKeyDown(debugKillHotkey))
        {
            DebugKillPlayer();
        }
    }

    private void SetState(DDRunState nextState)
    {
        currentState = nextState;
        Time.timeScale = currentState == DDRunState.Playing ? 1f : 0f;
        StateChanged?.Invoke(currentState);
    }

    private void UpdateGameOverSummary()
    {
        if (gameOverSummaryText == null)
        {
            return;
        }

        int level = progressionSystem != null ? progressionSystem.CurrentLevel : 1;
        string affixLine = activeAffixes.Count > 0
            ? $"\nАффиксы: {string.Join(", ", activeAffixes.ConvertAll(static affix => affix.DisplayName))}"
            : string.Empty;

        gameOverSummaryText.text = $"Поражение\nВремя: {FormatTime(elapsedRunTime)}\nУровень: {level}{affixLine}";
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void HidePausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void UpdateVictorySummary()
    {
        if (victorySummaryText == null)
        {
            return;
        }

        int level = progressionSystem != null ? progressionSystem.CurrentLevel : 1;
        int kills = spawnSystem != null ? spawnSystem.KillCount : 0;
        string affixLine = activeAffixes.Count > 0
            ? $"\nАффиксы: {string.Join(", ", activeAffixes.ConvertAll(static affix => affix.DisplayName))}"
            : string.Empty;

        victorySummaryText.text = $"Победа!\nВремя: {FormatTime(elapsedRunTime)}\nУровень: {level}\nУбийств: {kills}{affixLine}";
    }

    private void GrantRunRewards(bool isVictory)
    {
        if (hasGrantedRunRewards)
        {
            return;
        }

        hasGrantedRunRewards = true;

        int level = progressionSystem != null ? progressionSystem.CurrentLevel : 1;
        int kills = spawnSystem != null ? spawnSystem.KillCount : 0;
        int eliteKills = spawnSystem != null ? spawnSystem.EliteKillCount : 0;
        int miniBossKills = spawnSystem != null ? spawnSystem.MiniBossKillCount : 0;
        int bossKills = spawnSystem != null ? spawnSystem.BossKillCount : 0;

        lastRewardBreakdown = RunRewardSystem.Calculate(
            isVictory,
            elapsedRunTime,
            level,
            kills,
            eliteKills,
            miniBossKills,
            bossKills,
            currentMap,
            GetRunRewardMultiplier());

        RunRewardSystem.AwardToProfile(lastRewardBreakdown);
        RunRewardsGranted?.Invoke(lastRewardBreakdown);
    }

    private float GetRunRewardMultiplier()
    {
        float multiplier = currentMap != null ? currentMap.RewardMultiplier : 1f;

        if (activeAffixes.Count > 0)
        {
            multiplier *= 1f + activeAffixes.Count * 0.05f;
        }

        return Mathf.Max(0.1f, multiplier);
    }

    private static string FormatTime(float seconds)
    {
        TimeSpan span = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return span.ToString(@"mm\:ss");
    }

    private void SetHudVisible(bool visible)
    {
        if (hudRoot != null)
        {
            hudRoot.SetActive(visible);
        }
    }

    private void RollRunAffixes()
    {
        activeAffixes.Clear();

        if (affixPool == null || affixPool.Length == 0 || affixesPerRun <= 0)
        {
            ActiveAffixesChanged?.Invoke();
            return;
        }

        List<RunAffixData> candidates = new();

        for (int i = 0; i < affixPool.Length; i++)
        {
            if (affixPool[i] != null)
            {
                candidates.Add(affixPool[i]);
            }
        }

        int picks = Mathf.Min(affixesPerRun, candidates.Count);

        for (int i = 0; i < picks; i++)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            activeAffixes.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        ActiveAffixesChanged?.Invoke();
    }

    private float GetAggregateMultiplier(Func<RunAffixData, float> selector)
    {
        float value = 1f;

        for (int i = 0; i < activeAffixes.Count; i++)
        {
            value *= selector(activeAffixes[i]);
        }

        return value;
    }

    private int GetAggregateBonus(Func<RunAffixData, int> selector)
    {
        int value = 0;

        for (int i = 0; i < activeAffixes.Count; i++)
        {
            value += selector(activeAffixes[i]);
        }

        return value;
    }
}

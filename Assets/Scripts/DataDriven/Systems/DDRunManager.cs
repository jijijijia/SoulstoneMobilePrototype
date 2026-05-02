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
    [SerializeField] private bool beginRunOnStart = true;
    [SerializeField] private bool enableDebugKillHotkey = true;
    [SerializeField] private KeyCode debugKillHotkey = KeyCode.K;

    private ProgressionSystem progressionSystem;
    private MapData currentMap;
    private GameObject currentMapInstance;
    private DDRunState currentState = DDRunState.Initializing;
    private float elapsedRunTime;
    private readonly List<RunAffixData> activeAffixes = new();

    public event Action<DDRunState> StateChanged;
    public event Action<float> ElapsedTimeChanged;
    public event Action ActiveAffixesChanged;

    public DDRunState CurrentState => currentState;
    public float ElapsedRunTime => elapsedRunTime;
    public bool IsGameplayRunning => currentState == DDRunState.Playing;
    public IReadOnlyList<RunAffixData> ActiveAffixes => activeAffixes;
    public MapData CurrentMap => currentMap;
    public float CurrentSpawnRateMultiplier => GetAggregateMultiplier(affix => affix.SpawnRateMultiplier);
    public int CurrentPackSizeBonus => GetAggregateBonus(affix => affix.PackSizeBonus);
    public int CurrentMaxAliveBonus => GetAggregateBonus(affix => affix.MaxAliveBonus);
    public float CurrentEnemyHealthMultiplier => GetAggregateMultiplier(affix => affix.EnemyHealthMultiplier);
    public float CurrentEnemyDamageMultiplier => GetAggregateMultiplier(affix => affix.EnemyDamageMultiplier);
    public float CurrentExperienceMultiplier => GetAggregateMultiplier(affix => affix.ExperienceMultiplier);

    private void Start()
    {
        ResolveReferences();
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
    }

    public void BeginRun()
    {
        elapsedRunTime = 0f;
        RollRunAffixes();
        ElapsedTimeChanged?.Invoke(elapsedRunTime);
        SetHudVisible(true);
        HideGameOverPanel();
        SetState(DDRunState.Playing);
    }

    public void EnterLevelUpSelection()
    {
        if (currentState == DDRunState.GameOver)
        {
            return;
        }

        SetState(DDRunState.LevelUpSelection);
    }

    public void ResumeGameplay()
    {
        if (currentState == DDRunState.GameOver)
        {
            return;
        }

        SetState(DDRunState.Playing);
    }

    public void TriggerGameOver()
    {
        if (currentState == DDRunState.GameOver)
        {
            return;
        }

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
            currentMapInstance = Instantiate(currentMap.MapPrefab);
            currentMapInstance.name = currentMap.MapPrefab.name;
        }

        if (spawnSystem != null)
        {
            spawnSystem.ApplyMapData(currentMap);
        }
    }

    private void Subscribe()
    {
        if (characterSystem != null)
        {
            characterSystem.Died -= HandlePlayerDied;
            characterSystem.Died += HandlePlayerDied;
        }
    }

    private void Unsubscribe()
    {
        if (characterSystem != null)
        {
            characterSystem.Died -= HandlePlayerDied;
        }
    }

    private void HandlePlayerDied()
    {
        TriggerGameOver();
    }

    private void HandleDebugInput()
    {
        if (!enableDebugKillHotkey || currentState == DDRunState.GameOver)
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
            ? $"\nAffixes: {string.Join(", ", activeAffixes.ConvertAll(static affix => affix.DisplayName))}"
            : string.Empty;

        gameOverSummaryText.text = $"Game Over\nTime: {FormatTime(elapsedRunTime)}\nLevel: {level}{affixLine}";
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DDHUDManager : MonoBehaviour
{
    [SerializeField] private CharacterSystem characterSystem;
    [SerializeField] private DDRunManager runManager;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Image[] activeSkillIcons;
    [SerializeField] private TMP_Text[] activeSkillRankTexts;
    [SerializeField] private TMP_Text[] activeSkillNameTexts;
    [Header("Cooldown Panel")]
    [SerializeField] private Transform activeSkillCooldownRoot;
    [SerializeField] private SkillCooldownSlotView[] activeSkillCooldownSlots;
    [Header("Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private TMP_Text killsText;

    private ProgressionSystem progressionSystem;
    private SkillSystem skillSystem;
    private SpawnSystem spawnSystem;
    private bool hasBoundSkillSlots;

    private void Start()
    {
        ResolveReferences();
        ResolveCooldownSlots();
        Subscribe();
        RefreshAll();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ResolveCooldownSlots();
        Subscribe();
    }

    private void OnDisable()
    {
        if (characterSystem != null)
        {
            characterSystem.HealthChanged -= HandleHealthChanged;
        }

        if (progressionSystem != null)
        {
            progressionSystem.LevelChanged -= HandleLevelChanged;
            progressionSystem.ExperienceChanged -= HandleExperienceChanged;
        }

        if (skillSystem != null)
        {
            skillSystem.ActiveSkillsChanged -= HandleActiveSkillsChanged;
        }

        if (runManager != null)
        {
            runManager.ElapsedTimeChanged -= HandleElapsedTimeChanged;
            runManager.StateChanged -= HandleStateChanged;
        }

        if (spawnSystem != null)
        {
            spawnSystem.KillCountChanged -= HandleKillCountChanged;
        }
    }

    private void Update()
    {
        RefreshSkillCooldowns();
    }

    private void ResolveReferences()
    {
        if (characterSystem == null)
        {
            characterSystem = FindFirstObjectByType<CharacterSystem>();
        }

        if (characterSystem != null)
        {
            progressionSystem = characterSystem.ProgressionSystem;
            skillSystem = characterSystem.SkillSystem;
        }

        if (runManager == null)
        {
            runManager = FindFirstObjectByType<DDRunManager>();
        }

        if (spawnSystem == null)
        {
            spawnSystem = FindFirstObjectByType<SpawnSystem>();
        }
    }

    private void ResolveCooldownSlots()
    {
        if (activeSkillCooldownRoot == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            canvas = canvas != null ? canvas : FindFirstObjectByType<Canvas>();
            activeSkillCooldownRoot = FindChildRecursive(canvas != null ? canvas.transform : null, "ActiveSkillCooldownPanel");
        }

        if (activeSkillCooldownRoot == null)
        {
            Debug.LogWarning("DDHUDManager could not find ActiveSkillCooldownPanel in the scene. Add it under Canvas/HUDPanel to show skill cooldowns.");
            return;
        }

        activeSkillCooldownRoot.gameObject.SetActive(true);

        if (!HasCooldownSlots())
        {
            activeSkillCooldownSlots = activeSkillCooldownRoot.GetComponentsInChildren<SkillCooldownSlotView>(true);
        }
    }

    private bool HasCooldownSlots()
    {
        if (activeSkillCooldownSlots == null || activeSkillCooldownSlots.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < activeSkillCooldownSlots.Length; i++)
        {
            if (activeSkillCooldownSlots[i] != null)
            {
                return true;
            }
        }

        return false;
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

    private void Subscribe()
    {
        if (characterSystem != null)
        {
            characterSystem.HealthChanged -= HandleHealthChanged;
            characterSystem.HealthChanged += HandleHealthChanged;
        }

        if (progressionSystem != null)
        {
            progressionSystem.LevelChanged -= HandleLevelChanged;
            progressionSystem.ExperienceChanged -= HandleExperienceChanged;
            progressionSystem.LevelChanged += HandleLevelChanged;
            progressionSystem.ExperienceChanged += HandleExperienceChanged;
        }

        if (skillSystem != null)
        {
            skillSystem.ActiveSkillsChanged -= HandleActiveSkillsChanged;
            skillSystem.ActiveSkillsChanged += HandleActiveSkillsChanged;
        }

        if (runManager != null)
        {
            runManager.ElapsedTimeChanged -= HandleElapsedTimeChanged;
            runManager.StateChanged -= HandleStateChanged;
            runManager.ElapsedTimeChanged += HandleElapsedTimeChanged;
            runManager.StateChanged += HandleStateChanged;
        }

        if (spawnSystem != null)
        {
            spawnSystem.KillCountChanged -= HandleKillCountChanged;
            spawnSystem.KillCountChanged += HandleKillCountChanged;
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(HandlePauseClicked);
        }
    }

    private void RefreshAll()
    {
        if (characterSystem != null)
        {
            HandleHealthChanged(characterSystem.CurrentHealth, characterSystem.MaxHealth);
        }

        if (progressionSystem != null)
        {
            HandleLevelChanged(progressionSystem.CurrentLevel);
            HandleExperienceChanged(progressionSystem.CurrentExperience, progressionSystem.ExperienceToNextLevel);
        }

        if (runManager != null)
        {
            HandleElapsedTimeChanged(runManager.ElapsedRunTime);
            HandleStateChanged(runManager.CurrentState);
        }

        HandleActiveSkillsChanged();
    }

    private static readonly Color HpNormalColor = Color.white;
    private static readonly Color HpDangerColor = new(1f, 0.27f, 0.27f, 1f);

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
            healthText.color = maxHealth > 0f && currentHealth / maxHealth < 0.3f ? HpDangerColor : HpNormalColor;
        }
    }

    private void HandleLevelChanged(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    private void HandleExperienceChanged(int currentExperience, int experienceToNextLevel)
    {
        if (experienceText != null)
        {
            experienceText.text = $"XP: {currentExperience}/{experienceToNextLevel}";
        }

        if (experienceSlider != null)
        {
            experienceSlider.maxValue = Mathf.Max(1, experienceToNextLevel);
            experienceSlider.value = currentExperience;
        }
    }

    private void HandleElapsedTimeChanged(float elapsedTime)
    {
        if (timeText != null)
        {
            System.TimeSpan span = System.TimeSpan.FromSeconds(Mathf.Max(0f, elapsedTime));
            timeText.text = span.ToString(@"mm\:ss");
        }
    }

    private void HandleStateChanged(DDRunState state)
    {
        if (stateText != null)
        {
            stateText.text = state switch
            {
                DDRunState.Playing => "Бой",
                DDRunState.LevelUpSelection => "Новый уровень",
                DDRunState.GameOver => "Поражение",
                DDRunState.Victory => "Победа!",
                DDRunState.Paused => "Пауза",
                _ => "Готово"
            };
        }
    }

    private void HandleActiveSkillsChanged()
    {
        hasBoundSkillSlots = false;
        int count = skillSystem != null ? skillSystem.ActiveSkills.Count : 0;
        int maxSlots = Mathf.Max(
            activeSkillIcons != null ? activeSkillIcons.Length : 0,
            activeSkillRankTexts != null ? activeSkillRankTexts.Length : 0,
            activeSkillNameTexts != null ? activeSkillNameTexts.Length : 0,
            activeSkillCooldownSlots != null ? activeSkillCooldownSlots.Length : 0);

        for (int i = 0; i < maxSlots; i++)
        {
            SkillInstance skillInstance = i < count ? skillSystem.ActiveSkills[i] : null;
            SkillData skillData = skillInstance?.Data;

            if (activeSkillCooldownSlots != null && i < activeSkillCooldownSlots.Length && activeSkillCooldownSlots[i] != null)
            {
                activeSkillCooldownSlots[i].Bind(skillInstance);
            }

            if (activeSkillIcons != null && i < activeSkillIcons.Length && activeSkillIcons[i] != null)
            {
                activeSkillIcons[i].sprite = skillData != null ? skillData.Icon : null;
                activeSkillIcons[i].enabled = skillData != null && skillData.Icon != null;
            }

            if (activeSkillRankTexts != null && i < activeSkillRankTexts.Length && activeSkillRankTexts[i] != null)
            {
                activeSkillRankTexts[i].text = skillInstance != null ? $"R{skillInstance.Rank}" : string.Empty;
            }

            if (activeSkillNameTexts != null && i < activeSkillNameTexts.Length && activeSkillNameTexts[i] != null)
            {
                activeSkillNameTexts[i].text = skillData != null ? skillData.DisplayName : string.Empty;
            }
        }

        hasBoundSkillSlots = true;
        RefreshSkillCooldowns();
    }

    private void HandleKillCountChanged(int kills)
    {
        if (killsText != null)
        {
            killsText.text = $"Убийств: {kills}";
        }
    }

    private void HandlePauseClicked()
    {
        if (runManager == null)
        {
            return;
        }

        if (runManager.CurrentState == DDRunState.Paused)
        {
            runManager.UnpauseRun();
        }
        else if (runManager.CurrentState == DDRunState.Playing)
        {
            runManager.PauseRun();
        }
    }

    private void RefreshSkillCooldowns()
    {
        if (!hasBoundSkillSlots || activeSkillCooldownSlots == null)
        {
            return;
        }

        for (int i = 0; i < activeSkillCooldownSlots.Length; i++)
        {
            if (activeSkillCooldownSlots[i] != null)
            {
                activeSkillCooldownSlots[i].Refresh();
            }
        }
    }
}

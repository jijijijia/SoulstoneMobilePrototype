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

    private ProgressionSystem progressionSystem;
    private SkillSystem skillSystem;

    private void Start()
    {
        ResolveReferences();
        Subscribe();
        RefreshAll();
    }

    private void OnEnable()
    {
        ResolveReferences();
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

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
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
                DDRunState.Playing => "Fight",
                DDRunState.LevelUpSelection => "Level Up",
                DDRunState.GameOver => "Game Over",
                _ => "Ready"
            };
        }
    }

    private void HandleActiveSkillsChanged()
    {
        int count = skillSystem != null ? skillSystem.ActiveSkills.Count : 0;
        int maxSlots = Mathf.Max(
            activeSkillIcons != null ? activeSkillIcons.Length : 0,
            activeSkillRankTexts != null ? activeSkillRankTexts.Length : 0,
            activeSkillNameTexts != null ? activeSkillNameTexts.Length : 0);

        for (int i = 0; i < maxSlots; i++)
        {
            SkillInstance skillInstance = i < count ? skillSystem.ActiveSkills[i] : null;
            SkillData skillData = skillInstance?.Data;

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
    }
}

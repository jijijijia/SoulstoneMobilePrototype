using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunResultScreen : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text skillsText;
    [SerializeField] private TMP_Text affixesText;
    [SerializeField] private TMP_Text rewardsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private DDRunManager runManager;
    private CharacterSystem characterSystem;
    private SpawnSystem spawnSystem;

    private void Start()
    {
        ResolveReferences();
        HideContainer();
        BindButtons();
        Subscribe();
    }

    private void OnEnable()
    {
        ResolveReferences();
        BindButtons();
        Subscribe();
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.StateChanged -= HandleStateChanged;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
        }
    }

    private void ResolveReferences()
    {
        if (runManager == null)
        {
            runManager = FindFirstObjectByType<DDRunManager>();
        }

        if (characterSystem == null)
        {
            characterSystem = FindFirstObjectByType<CharacterSystem>();
        }

        if (spawnSystem == null)
        {
            spawnSystem = FindFirstObjectByType<SpawnSystem>();
        }
    }

    private void Subscribe()
    {
        if (runManager != null)
        {
            runManager.StateChanged -= HandleStateChanged;
            runManager.StateChanged += HandleStateChanged;
        }
    }

    private void HandleStateChanged(DDRunState state)
    {
        if (state == DDRunState.GameOver || state == DDRunState.Victory)
        {
            ShowResult(state == DDRunState.Victory);
            return;
        }

        if (state == DDRunState.Playing)
        {
            HideContainer();
        }
    }

    private void ShowResult(bool victory)
    {
        if (container != null)
        {
            container.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = victory ? "Победа!" : "Поражение";
        }

        float runTime = runManager != null ? runManager.ElapsedRunTime : 0f;

        if (timeText != null)
        {
            System.TimeSpan span = System.TimeSpan.FromSeconds(Mathf.Max(0f, runTime));
            timeText.text = $"Время: {span.ToString(@"mm\:ss")}";
        }

        ProgressionSystem progressionSystem = characterSystem != null ? characterSystem.ProgressionSystem : null;
        int level = progressionSystem != null ? progressionSystem.CurrentLevel : 1;

        if (levelText != null)
        {
            levelText.text = $"Уровень: {level}";
        }

        int kills = spawnSystem != null ? spawnSystem.KillCount : 0;

        if (killsText != null)
        {
            killsText.text = $"Убийств: {kills}";
        }

        if (skillsText != null)
        {
            SkillSystem skillSystem = characterSystem != null ? characterSystem.SkillSystem : null;
            skillsText.text = BuildSkillsLine(skillSystem);
        }

        if (affixesText != null && runManager != null)
        {
            string affixesLine = BuildAffixesLine(runManager);
            RunRewardBreakdown rewards = runManager.LastRewardBreakdown;

            if (rewardsText != null)
            {
                affixesText.text = affixesLine;
                rewardsText.text = BuildRewardsLine(rewards);
            }
            else
            {
                string rewardsLine = BuildRewardsLine(rewards);
                affixesText.text = string.IsNullOrWhiteSpace(rewardsLine)
                    ? affixesLine
                    : $"{affixesLine}\n{rewardsLine}";
            }
        }
    }

    private void BindButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartRun);
            restartButton.onClick.AddListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    private static string BuildSkillsLine(SkillSystem skillSystem)
    {
        if (skillSystem == null || skillSystem.ActiveSkills.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();
        sb.Append("Навыки: ");

        for (int i = 0; i < skillSystem.ActiveSkills.Count; i++)
        {
            SkillInstance skill = skillSystem.ActiveSkills[i];

            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(skill.Data != null ? skill.Data.DisplayName : "?");
            sb.Append(" R");
            sb.Append(skill.Rank);
        }

        return sb.ToString();
    }

    private static string BuildRewardsLine(RunRewardBreakdown rewards)
    {
        if (rewards == null || !rewards.HasRewards)
        {
            return "Награды: нет";
        }

        StringBuilder sb = new();
        sb.Append("Награды: ");

        for (int i = 0; i < rewards.rewards.Count; i++)
        {
            RunRewardAmount reward = rewards.rewards[i];

            if (reward == null || reward.amount <= 0)
            {
                continue;
            }

            if (sb.Length > "Награды: ".Length)
            {
                sb.Append(", ");
            }

            sb.Append(GetCurrencyDisplayName(reward.currencyType));
            sb.Append(" +");
            sb.Append(reward.amount);
        }

        return sb.ToString();
    }

    private static string GetCurrencyDisplayName(CurrencyType currencyType)
    {
        return currencyType switch
        {
            CurrencyType.SoulShards => "осколки душ",
            CurrencyType.RedCrystal => "редкие кристаллы",
            CurrencyType.WeaponOre => "материалы оружия",
            CurrencyType.RuneDust => "руническая пыль",
            CurrencyType.BlueCrystal => "синие кристаллы",
            CurrencyType.AmberCrystal => "янтарные кристаллы",
            CurrencyType.GreenCrystal => "зелёные кристаллы",
            CurrencyType.VoidCrystal => "кристаллы бездны",
            _ => currencyType.ToString()
        };
    }

    private void RestartRun()
    {
        if (runManager != null)
        {
            runManager.RestartRun();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static string BuildAffixesLine(DDRunManager runManager)
    {
        if (runManager.ActiveAffixes.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();
        sb.Append("Аффиксы: ");

        for (int i = 0; i < runManager.ActiveAffixes.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            sb.Append(runManager.ActiveAffixes[i].DisplayName);
        }

        return sb.ToString();
    }

    private void HideContainer()
    {
        if (container != null)
        {
            container.SetActive(false);
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DDLevelUpManager : MonoBehaviour
{
    [SerializeField] private CharacterSystem characterSystem;
    [SerializeField] private DDRunManager runManager;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Image[] optionIcons;
    [SerializeField] private TMP_Text[] optionTypeTexts;
    [SerializeField] private TMP_Text[] optionTitleTexts;
    [SerializeField] private TMP_Text[] optionDescriptionTexts;
    [SerializeField] private TMP_Text[] optionRankTexts;
    [SerializeField] private Button swapToUpgradesButton;

    private ProgressionSystem progressionSystem;
    private readonly List<ProgressionChoice> currentChoices = new();
    private bool subscribed;

    private void Start()
    {
        ResolveReferences();
        SubscribeIfReady();

        if (!subscribed)
        {
            Invoke(nameof(RetrySubscription), 0.1f);
        }

        HidePanel();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeIfReady();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void RetrySubscription()
    {
        ResolveReferences();
        SubscribeIfReady();
    }

    public void SelectChoice(int index)
    {
        if (index < 0 || index >= currentChoices.Count)
        {
            return;
        }

        currentChoices[index].Apply();
        HidePanel();
        if (runManager != null)
        {
            runManager.ResumeGameplay();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void SwapSkillChoicesToUpgrades()
    {
        if (progressionSystem == null || !progressionSystem.CanSwapSkillsToUpgrades)
        {
            return;
        }

        ApplyChoices(progressionSystem.GenerateUpgradeChoices());
    }

    private void HandleLevelChanged(int level)
    {
        if (level <= 1)
        {
            return;
        }

        ShowChoices();
    }

    private void ShowChoices()
    {
        if (levelUpPanel == null || progressionSystem == null)
        {
            return;
        }

        ApplyChoices(progressionSystem.GenerateChoices());

        if (currentChoices.Count == 0)
        {
            return;
        }

        if (runManager != null)
        {
            runManager.EnterLevelUpSelection();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    private void HidePanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        if (swapToUpgradesButton != null)
        {
            swapToUpgradesButton.gameObject.SetActive(false);
            swapToUpgradesButton.onClick.RemoveAllListeners();
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
        }

        if (runManager == null)
        {
            runManager = FindFirstObjectByType<DDRunManager>();
        }
    }

    private void SubscribeIfReady()
    {
        if (subscribed || progressionSystem == null)
        {
            return;
        }

        progressionSystem.LevelChanged += HandleLevelChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || progressionSystem == null)
        {
            return;
        }

        progressionSystem.LevelChanged -= HandleLevelChanged;
        subscribed = false;
    }

    private void ApplyChoices(List<ProgressionChoice> choices)
    {
        currentChoices.Clear();

        if (choices != null)
        {
            currentChoices.AddRange(choices);
        }

        if (currentChoices.Count == 0)
        {
            HidePanel();
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool active = i < currentChoices.Count;
            optionButtons[i].gameObject.SetActive(active);
            optionButtons[i].onClick.RemoveAllListeners();

            if (!active)
            {
                continue;
            }

            ConfigureLevelUpButtonStyle(optionButtons[i]);

            if (optionIcons != null && i < optionIcons.Length && optionIcons[i] != null)
            {
                optionIcons[i].sprite = currentChoices[i].Icon;
                optionIcons[i].enabled = currentChoices[i].Icon != null;
            }

            if (optionTypeTexts != null && i < optionTypeTexts.Length && optionTypeTexts[i] != null)
            {
                optionTypeTexts[i].text = currentChoices[i].TypeLabel ?? string.Empty;
            }

            if (optionTitleTexts != null && i < optionTitleTexts.Length && optionTitleTexts[i] != null)
            {
                optionTitleTexts[i].text = currentChoices[i].Title;
            }

            if (optionDescriptionTexts != null && i < optionDescriptionTexts.Length && optionDescriptionTexts[i] != null)
            {
                optionDescriptionTexts[i].text = currentChoices[i].Description;
            }

            if (optionRankTexts != null && i < optionRankTexts.Length && optionRankTexts[i] != null)
            {
                optionRankTexts[i].text = currentChoices[i].RankText ?? string.Empty;
            }

            int capturedIndex = i;
            optionButtons[i].onClick.AddListener(() => SelectChoice(capturedIndex));
        }

        levelUpPanel.SetActive(true);
        RefreshSwapButton();
    }

    private static void ConfigureLevelUpButtonStyle(Button button)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = new Color(0.17f, 0.18f, 0.21f, 0.96f);
        cb.highlightedColor = new Color(0.28f, 0.24f, 0.12f, 1f);
        cb.pressedColor = new Color(0.60f, 0.46f, 0.18f, 1f);
        cb.selectedColor = cb.highlightedColor;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.08f;
        button.colors = cb;
        button.transition = Selectable.Transition.ColorTint;

        Outline outline = button.GetComponent<Outline>();

        if (outline != null)
        {
            outline.effectColor = new Color(0.86f, 0.68f, 0.32f, 0f);
            outline.effectDistance = new Vector2(3f, -3f);
        }
        else
        {
            Debug.LogWarning($"Level-up option '{button.name}' is missing a scene-authored Outline component.", button);
        }

        if (button.GetComponent<ButtonPunchAnimation>() == null)
        {
            Debug.LogWarning($"Level-up option '{button.name}' is missing a scene-authored ButtonPunchAnimation component.", button);
        }

        LevelUpButtonHighlight highlight = button.GetComponent<LevelUpButtonHighlight>();

        if (highlight != null)
        {
            highlight.SetOutline(outline);
        }
        else
        {
            Debug.LogWarning($"Level-up option '{button.name}' is missing a scene-authored LevelUpButtonHighlight component.", button);
        }
    }

    private void RefreshSwapButton()
    {
        if (swapToUpgradesButton == null)
        {
            return;
        }

        bool active = progressionSystem != null && progressionSystem.CanSwapSkillsToUpgrades;
        swapToUpgradesButton.gameObject.SetActive(active);
        swapToUpgradesButton.onClick.RemoveAllListeners();

        if (active)
        {
            swapToUpgradesButton.onClick.AddListener(SwapSkillChoicesToUpgrades);
        }
    }
}

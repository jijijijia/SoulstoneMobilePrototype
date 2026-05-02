using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionSystem : MonoBehaviour
{
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int baseExperienceToNextLevel = 5;
    [SerializeField] private int experienceStepPerLevel = 3;

    private CharacterSystem owner;
    private SkillSystem skillSystem;
    private UpgradeSystem upgradeSystem;
    private ProgressionChoiceGenerator choiceGenerator;
    private int currentLevel;
    private int currentExperience;
    private int experienceToNextLevel;
    private ProgressionChoiceSetType currentChoiceSetType;

    public event Action<int> LevelChanged;
    public event Action<int, int> ExperienceChanged;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public ProgressionChoiceSetType CurrentChoiceSetType => currentChoiceSetType;
    public bool CanSwapSkillsToUpgrades => currentChoiceSetType == ProgressionChoiceSetType.Skills &&
                                           choiceGenerator != null &&
                                           choiceGenerator.HasUpgradeChoices(upgradeSystem);

    public void Initialize(CharacterSystem characterSystem, SkillSystem ownerSkillSystem, UpgradeSystem ownerUpgradeSystem, ProgressionChoiceGenerator progressionChoiceGenerator)
    {
        owner = characterSystem;
        skillSystem = ownerSkillSystem;
        upgradeSystem = ownerUpgradeSystem;
        choiceGenerator = progressionChoiceGenerator;
        ResetState();
        currentLevel = Mathf.Max(1, startingLevel);
        experienceToNextLevel = GetRequiredExperienceForLevel(currentLevel);
        ExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        LevelChanged?.Invoke(currentLevel);
    }

    public void ResetState()
    {
        currentExperience = 0;
        currentLevel = 0;
        experienceToNextLevel = 0;
        currentChoiceSetType = ProgressionChoiceSetType.None;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        float experienceMultiplier = owner != null && owner.RuntimeStats != null
            ? Mathf.Max(0f, owner.RuntimeStats.GetValue(StatType.ExperienceMultiplier))
            : 1f;
        int adjustedAmount = experienceMultiplier <= 0f
            ? 0
            : Mathf.Max(1, Mathf.CeilToInt(amount * experienceMultiplier));

        if (adjustedAmount <= 0)
        {
            return;
        }

        currentExperience += adjustedAmount;
        ExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;
            experienceToNextLevel = GetRequiredExperienceForLevel(currentLevel);
            LevelChanged?.Invoke(currentLevel);
            ExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        }
    }

    public List<ProgressionChoice> GenerateChoices()
    {
        if (choiceGenerator == null)
        {
            currentChoiceSetType = ProgressionChoiceSetType.None;
            return new List<ProgressionChoice>();
        }

        bool hasSkillChoices = choiceGenerator.HasSkillChoices(skillSystem);
        bool hasUpgradeChoices = choiceGenerator.HasUpgradeChoices(upgradeSystem);

        if (!hasSkillChoices && !hasUpgradeChoices)
        {
            currentChoiceSetType = ProgressionChoiceSetType.None;
            return new List<ProgressionChoice>();
        }

        if (hasSkillChoices && hasUpgradeChoices)
        {
            bool rollSkills = UnityEngine.Random.value < 0.5f;
            currentChoiceSetType = rollSkills ? ProgressionChoiceSetType.Skills : ProgressionChoiceSetType.Upgrades;
            return rollSkills
                ? choiceGenerator.GenerateSkillChoices(skillSystem)
                : choiceGenerator.GenerateUpgradeChoices(upgradeSystem);
        }

        if (hasSkillChoices)
        {
            currentChoiceSetType = ProgressionChoiceSetType.Skills;
            return choiceGenerator.GenerateSkillChoices(skillSystem);
        }

        currentChoiceSetType = ProgressionChoiceSetType.Upgrades;
        return choiceGenerator.GenerateUpgradeChoices(upgradeSystem);
    }

    public List<ProgressionChoice> GenerateUpgradeChoices()
    {
        if (choiceGenerator == null)
        {
            currentChoiceSetType = ProgressionChoiceSetType.None;
            return new List<ProgressionChoice>();
        }

        currentChoiceSetType = ProgressionChoiceSetType.Upgrades;
        return choiceGenerator.GenerateUpgradeChoices(upgradeSystem);
    }

    private int GetRequiredExperienceForLevel(int level)
    {
        return baseExperienceToNextLevel + Mathf.Max(0, level - 1) * experienceStepPerLevel;
    }
}

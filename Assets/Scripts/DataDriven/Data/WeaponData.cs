using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Weapon", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject visualPrefab;
    [SerializeField] private SkillData startingSkill;
    [SerializeField] private SkillData[] uniqueSkillPool;
    [Header("Forge")]
    [SerializeField] private int maxLevel = 10;
    [SerializeField] private CurrencyAmount[] forgeUpgradeCostByLevel;
    [SerializeField] private StatModifierData[] statGrowthPerLevel;
    [SerializeField] private StatModifierData[] skillGrowthPerLevel;
    [SerializeField] private CurrencyAmount[] requiredMaterials;
    [SerializeField] private GameObject forgeVisualPrefab;
    [Header("Unlock")]
    [SerializeField] private string unlockId;
    [SerializeField] private bool unlockedByDefault;
    [SerializeField] private int unlockCost;
    [SerializeField] private CurrencyType unlockCurrency = CurrencyType.SoulShards;
    [TextArea(2, 4)]
    [SerializeField] private string unlockRequirementText;
    [SerializeField] private string requiredAchievementId;
    [SerializeField] private int requiredPlayerLevel = 1;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject VisualPrefab => visualPrefab;
    public SkillData StartingSkill => startingSkill;
    public SkillData[] UniqueSkillPool => uniqueSkillPool;
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public CurrencyAmount[] ForgeUpgradeCostByLevel => forgeUpgradeCostByLevel;
    public StatModifierData[] StatGrowthPerLevel => statGrowthPerLevel;
    public StatModifierData[] SkillGrowthPerLevel => skillGrowthPerLevel;
    public CurrencyAmount[] RequiredMaterials => requiredMaterials;
    public GameObject ForgeVisualPrefab => forgeVisualPrefab != null ? forgeVisualPrefab : visualPrefab;
    public string UnlockId => string.IsNullOrWhiteSpace(unlockId) ? weaponId : unlockId;
    public bool UnlockedByDefault => unlockedByDefault;
    public int UnlockCost => unlockedByDefault ? 0 : Mathf.Max(75, unlockCost);
    public CurrencyType UnlockCurrency => unlockCurrency;
    public string UnlockRequirementText => string.IsNullOrWhiteSpace(unlockRequirementText)
        ? $"Разблокировать за {UnlockCost} {UnlockCurrency}"
        : unlockRequirementText;
    public string RequiredAchievementId => requiredAchievementId;
    public int RequiredPlayerLevel => Mathf.Max(1, requiredPlayerLevel);

    public CurrencyAmount[] GetForgeUpgradeCost(int currentLevel)
    {
        int safeCurrentLevel = Mathf.Clamp(currentLevel, 1, MaxLevel);

        if (forgeUpgradeCostByLevel != null && forgeUpgradeCostByLevel.Length > 0)
        {
            int index = Mathf.Clamp(safeCurrentLevel - 1, 0, forgeUpgradeCostByLevel.Length - 1);
            CurrencyAmount configuredCost = forgeUpgradeCostByLevel[index];

            if (configuredCost != null && configuredCost.amount > 0)
            {
                return new[] { configuredCost };
            }
        }

        int nextLevel = Mathf.Min(MaxLevel, safeCurrentLevel + 1);

        return new[]
        {
            new CurrencyAmount
            {
                currencyType = CurrencyType.WeaponOre,
                amount = 25 * nextLevel
            },
            new CurrencyAmount
            {
                currencyType = CurrencyType.SoulShards,
                amount = 50 * nextLevel
            }
        };
    }
}

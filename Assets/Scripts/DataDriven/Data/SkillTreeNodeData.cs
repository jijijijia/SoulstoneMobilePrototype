using UnityEngine;

[CreateAssetMenu(fileName = "SkillTreeNode_New", menuName = "Soulstone/Skill Tree/Node")]
public class SkillTreeNodeData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Progression")]
    [Min(1)]
    [SerializeField] private int maxLevel = 1;
    [SerializeField] private CurrencyAmount[] costPerLevel;
    [SerializeField] private CurrencyType currencyType = CurrencyType.SoulShards;
    [SerializeField] private string[] requiredNodeIds;

    [Header("Bonuses")]
    [SerializeField] private StatModifierData[] statModifiers;
    [SerializeField] private SkillTreeUnlockEffectData[] unlockEffects;

    public string NodeId => string.IsNullOrWhiteSpace(nodeId) ? name : nodeId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public CurrencyAmount[] CostPerLevel => costPerLevel;
    public CurrencyType CurrencyType => currencyType;
    public string[] RequiredNodeIds => requiredNodeIds;
    public StatModifierData[] StatModifiers => statModifiers;
    public SkillTreeUnlockEffectData[] UnlockEffects => unlockEffects;

    public CurrencyAmount GetCostForLevel(int currentLevel)
    {
        int nextLevel = Mathf.Clamp(currentLevel + 1, 1, MaxLevel);

        if (costPerLevel != null && costPerLevel.Length >= nextLevel && costPerLevel[nextLevel - 1] != null)
        {
            return costPerLevel[nextLevel - 1];
        }

        return new CurrencyAmount
        {
            currencyType = currencyType,
            amount = Mathf.Max(1, nextLevel) * 100
        };
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            nodeId = name;
        }

        maxLevel = Mathf.Max(1, maxLevel);
    }
}

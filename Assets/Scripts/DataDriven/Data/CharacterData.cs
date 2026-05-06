using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Character", fileName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private StatValue[] baseStats;
    [SerializeField] private WeaponData startingWeapon;
    [SerializeField] private WeaponData[] availableWeapons;
    [SerializeField] private UpgradeData[] globalUpgradePool;
    [Header("Unlock")]
    [SerializeField] private string unlockId;
    [SerializeField] private bool unlockedByDefault;
    [SerializeField] private int unlockCost;
    [SerializeField] private CurrencyType unlockCurrency = CurrencyType.SoulShards;
    [TextArea(2, 4)]
    [SerializeField] private string unlockRequirementText;
    [SerializeField] private string requiredAchievementId;
    [SerializeField] private int requiredPlayerLevel = 1;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public GameObject ModelPrefab => modelPrefab;
    public StatValue[] BaseStats => baseStats;
    public WeaponData StartingWeapon => startingWeapon;
    public WeaponData[] AvailableWeapons => availableWeapons;
    public UpgradeData[] GlobalUpgradePool => globalUpgradePool;
    public string UnlockId => string.IsNullOrWhiteSpace(unlockId) ? characterId : unlockId;
    public bool UnlockedByDefault => unlockedByDefault;
    public int UnlockCost => unlockedByDefault ? 0 : Mathf.Max(100, unlockCost);
    public CurrencyType UnlockCurrency => unlockCurrency;
    public string UnlockRequirementText => string.IsNullOrWhiteSpace(unlockRequirementText)
        ? $"Разблокировать за {UnlockCost} {UnlockCurrency}"
        : unlockRequirementText;
    public string RequiredAchievementId => requiredAchievementId;
    public int RequiredPlayerLevel => Mathf.Max(1, requiredPlayerLevel);
}

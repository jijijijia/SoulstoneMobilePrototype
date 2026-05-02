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

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public GameObject ModelPrefab => modelPrefab;
    public StatValue[] BaseStats => baseStats;
    public WeaponData StartingWeapon => startingWeapon;
    public WeaponData[] AvailableWeapons => availableWeapons;
    public UpgradeData[] GlobalUpgradePool => globalUpgradePool;
}

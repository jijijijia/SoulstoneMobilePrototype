using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Weapon", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private SkillData startingSkill;
    [SerializeField] private SkillData[] uniqueSkillPool;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public SkillData StartingSkill => startingSkill;
    public SkillData[] UniqueSkillPool => uniqueSkillPool;
}

using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    private CharacterSystem owner;
    private SkillSystem skillSystem;
    private WeaponData currentWeapon;

    public WeaponData CurrentWeapon => currentWeapon;

    public void Initialize(CharacterSystem characterSystem, SkillSystem ownerSkillSystem)
    {
        owner = characterSystem;
        skillSystem = ownerSkillSystem;
        ResetState();
    }

    public void Equip(WeaponData weaponData)
    {
        if (weaponData == null || owner == null || skillSystem == null || owner.RuntimeStats == null)
        {
            return;
        }

        currentWeapon = weaponData;
        skillSystem.RegisterWeapon(weaponData);

        if (weaponData.StartingSkill != null)
        {
            skillSystem.AcquireSkill(weaponData.StartingSkill, true);
        }
    }

    public void ResetState()
    {
        currentWeapon = null;
    }
}

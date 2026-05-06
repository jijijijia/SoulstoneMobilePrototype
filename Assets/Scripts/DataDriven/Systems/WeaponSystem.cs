using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    private CharacterSystem owner;
    private SkillSystem skillSystem;
    private WeaponData currentWeapon;
    private string activeModifierSourceId;

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
        ApplyForgeStatBonuses(weaponData);
        skillSystem.RegisterWeapon(weaponData);

        if (weaponData.StartingSkill != null)
        {
            skillSystem.AcquireSkill(weaponData.StartingSkill, true);
        }
    }

    public void ResetState()
    {
        RemoveForgeStatBonuses();
        currentWeapon = null;
    }

    public void GetSkillModifierTotals(SkillData skillData, StatType statType, out float additive, out float multiplier)
    {
        ForgeSystem.GetSkillModifierTotals(currentWeapon, skillData, statType, out additive, out multiplier);
    }

    private void ApplyForgeStatBonuses(WeaponData weaponData)
    {
        RemoveForgeStatBonuses();

        if (owner?.RuntimeStats == null || weaponData == null)
        {
            return;
        }

        StatModifierData[] modifiers = ForgeSystem.GetCharacterStatModifiers(weaponData);

        if (modifiers == null || modifiers.Length == 0)
        {
            return;
        }

        activeModifierSourceId = $"forge:weapon:{weaponData.WeaponId}";
        owner.RuntimeStats.AddModifiers(activeModifierSourceId, modifiers);
    }

    private void RemoveForgeStatBonuses()
    {
        if (owner?.RuntimeStats != null && !string.IsNullOrWhiteSpace(activeModifierSourceId))
        {
            owner.RuntimeStats.RemoveModifiers(activeModifierSourceId);
        }

        activeModifierSourceId = null;
    }
}

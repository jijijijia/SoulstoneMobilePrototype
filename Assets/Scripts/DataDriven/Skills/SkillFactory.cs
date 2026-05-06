using UnityEngine;

public static class SkillFactory
{
    public static SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        if (owner == null || skillData == null)
        {
            return null;
        }

        if (skillData.AttackDefinitions != null && skillData.AttackDefinitions.Length > 0)
        {
            ModularAttackSkill modularSkill = owner.gameObject.AddComponent<ModularAttackSkill>();
            modularSkill.Initialize(new SkillRuntimeContext
            {
                Owner = owner,
                SkillData = skillData,
                OwnerStats = owner.RuntimeStats,
                UpgradeSystem = owner.UpgradeSystem
            }, rank);
            return modularSkill;
        }

        if (skillData.RuntimeDefinition == null)
        {
            Debug.LogError($"Skill '{skillData.name}' does not have a Runtime Definition assigned.");
            return null;
        }

        return skillData.RuntimeDefinition.CreateRuntime(owner, skillData, rank);
    }
}

using UnityEngine;

public static class SkillFactory
{
    public static SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        if (owner == null || skillData == null)
        {
            return null;
        }

        if (skillData.RuntimeDefinition == null)
        {
            Debug.LogError($"Skill '{skillData.name}' does not have a Runtime Definition assigned.");
            return null;
        }

        return skillData.RuntimeDefinition.CreateRuntime(owner, skillData, rank);
    }
}

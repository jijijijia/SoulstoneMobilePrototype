using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill Runtime/Modular Attack", fileName = "ModularAttackSkillRuntimeDefinition")]
public class ModularAttackSkillRuntimeDefinition : SkillRuntimeDefinition
{
    public override SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        ModularAttackSkill behaviour = owner.gameObject.AddComponent<ModularAttackSkill>();
        behaviour.Initialize(new SkillRuntimeContext
        {
            Owner = owner,
            SkillData = skillData,
            OwnerStats = owner.RuntimeStats,
            UpgradeSystem = owner.UpgradeSystem
        }, rank);
        return behaviour;
    }
}

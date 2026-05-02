using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill Runtime/Shockwave", fileName = "ShockwaveSkillRuntimeDefinition")]
public class ShockwaveSkillRuntimeDefinition : SkillRuntimeDefinition
{
    public override SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        DataDrivenShockwaveSkill behaviour = owner.gameObject.AddComponent<DataDrivenShockwaveSkill>();
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

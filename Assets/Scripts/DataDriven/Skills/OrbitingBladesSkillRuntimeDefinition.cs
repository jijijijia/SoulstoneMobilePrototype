using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill Runtime/Orbiting Blades", fileName = "OrbitingBladesSkillRuntimeDefinition")]
public class OrbitingBladesSkillRuntimeDefinition : SkillRuntimeDefinition
{
    public override SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        DataDrivenOrbitingBlade behaviour = owner.gameObject.AddComponent<DataDrivenOrbitingBlade>();
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

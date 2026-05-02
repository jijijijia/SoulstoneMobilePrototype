using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill Runtime/Configurable Skill", fileName = "ConfigurableSkillRuntimeDefinition")]
public class ConfigurableSkillRuntimeDefinition : SkillRuntimeDefinition
{
    public override SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        ConfigurableSkillBehaviour behaviour = owner.gameObject.AddComponent<ConfigurableSkillBehaviour>();
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

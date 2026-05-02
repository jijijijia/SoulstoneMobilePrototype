using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Skill Runtime/Projectile", fileName = "ProjectileSkillRuntimeDefinition")]
public class ProjectileSkillRuntimeDefinition : SkillRuntimeDefinition
{
    public override SkillBehaviourBase CreateRuntime(CharacterSystem owner, SkillData skillData, int rank)
    {
        DataDrivenProjectileSkill behaviour = owner.gameObject.AddComponent<DataDrivenProjectileSkill>();
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

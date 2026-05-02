using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Impact/Status", fileName = "StatusAttackImpact")]
public class StatusAttackImpactDefinition : AttackImpactDefinition
{
    [SerializeField] private bool includeSkillStatuses = true;
    [SerializeField] private StatusEffectData[] additionalStatuses;

    public override void BuildPayload(AttackRuntimeContext context, AttackResolvedPayload payload)
    {
        if (context?.SkillContext == null || payload == null)
        {
            return;
        }

        if (includeSkillStatuses)
        {
            payload.AddStatuses(context.SkillContext.SkillData.AppliedStatuses);
        }

        payload.AddStatuses(additionalStatuses);
    }
}

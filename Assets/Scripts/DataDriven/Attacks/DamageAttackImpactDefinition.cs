using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Impact/Damage", fileName = "DamageAttackImpact")]
public class DamageAttackImpactDefinition : AttackImpactDefinition
{
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float damagePerRank = 5f;
    [SerializeField] private float ownerDamageMultiplier = 1f;

    public override void BuildPayload(AttackRuntimeContext context, AttackResolvedPayload payload)
    {
        if (context?.SkillContext == null || payload == null)
        {
            return;
        }

        payload.AddDamage(context.SkillContext.ResolveDamage(baseDamage, damagePerRank, context.Rank, ownerDamageMultiplier));
    }
}

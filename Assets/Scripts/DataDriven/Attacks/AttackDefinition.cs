using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Definition", fileName = "AttackDefinition")]
public class AttackDefinition : ScriptableObject
{
    [SerializeField] private string attackId;
    [SerializeField] private string displayName;
    [SerializeField] private float baseCooldown = 1f;
    [SerializeField] private float cooldownReductionPerRank = 0.05f;
    [SerializeField] private AttackTargetingDefinition targeting;
    [SerializeField] private AttackDeliveryDefinition delivery;
    [SerializeField] private AttackImpactDefinition[] impacts;

    public string AttackId => attackId;
    public string DisplayName => displayName;
    public AttackTargetingDefinition Targeting => targeting;
    public AttackDeliveryDefinition Delivery => delivery;
    public AttackImpactDefinition[] Impacts => impacts;

    public float ResolveCooldown(SkillRuntimeContext context, int rank)
    {
        return context.ResolveCooldown(baseCooldown, cooldownReductionPerRank, rank);
    }
}

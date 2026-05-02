using UnityEngine;

public abstract class AttackImpactDefinition : ScriptableObject
{
    public abstract void BuildPayload(AttackRuntimeContext context, AttackResolvedPayload payload);
}

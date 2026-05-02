using System.Collections.Generic;
using UnityEngine;

public abstract class AttackDeliveryDefinition : ScriptableObject
{
    public abstract void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload);
}

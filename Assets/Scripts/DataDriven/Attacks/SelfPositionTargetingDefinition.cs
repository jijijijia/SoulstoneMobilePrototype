using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Targeting/Self Position", fileName = "SelfPositionTargeting")]
public class SelfPositionTargetingDefinition : AttackTargetingDefinition
{
    public override void ResolveTargets(AttackRuntimeContext context, List<AttackTargetData> results)
    {
        results.Clear();

        if (context?.Owner == null)
        {
            return;
        }

        results.Add(new AttackTargetData
        {
            Enemy = null,
            Position = context.Owner.transform.position
        });
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Circular Sweep", fileName = "CircularSweepAttackDelivery")]
public class CircularSweepAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 2.8f;
    [SerializeField] private float sweepDuration = 0.22f;
    [SerializeField] private float arcWidth = 70f;
    [SerializeField] private bool clockwise = true;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        GameObject hitboxObject = new("CircularSweepHitbox");
        RuntimeCircularSweepHitbox hitbox = hitboxObject.AddComponent<RuntimeCircularSweepHitbox>();
        hitbox.Initialize(
            context.Owner.transform,
            context.SkillContext.ResolveAreaRadius(radius),
            sweepDuration,
            arcWidth,
            clockwise,
            payload.Damage,
            payload.ToStatusArray());
    }
}

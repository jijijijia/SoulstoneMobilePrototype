using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Circular Sweep", fileName = "CircularSweepAttackDelivery")]
public class CircularSweepAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 2.8f;
    [SerializeField] private float sweepDuration = 0.22f;
    [Tooltip("Total angle covered by the swing. Use 360 for a full spin, 90-180 for a partial weapon arc.")]
    [SerializeField] private float sweepAngle = 360f;
    [Tooltip("Width of the active damage slice while the swing moves along the total angle.")]
    [SerializeField] private float arcWidth = 70f;
    [SerializeField] private bool clockwise = true;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        GameObject hitboxObject = PoolManager.Spawn(DefaultRuntimePrefabFactory.GetCircularSweepHitboxPrefab(), context.Owner.transform.position, Quaternion.identity);
        RuntimeCircularSweepHitbox hitbox = hitboxObject.GetComponent<RuntimeCircularSweepHitbox>();
        hitbox.Initialize(
            context.Owner.transform,
            context.SkillContext.ResolveAreaRadius(radius),
            sweepDuration,
            sweepAngle,
            arcWidth,
            clockwise,
            payload.Damage,
            payload.ToStatusArray());
    }
}

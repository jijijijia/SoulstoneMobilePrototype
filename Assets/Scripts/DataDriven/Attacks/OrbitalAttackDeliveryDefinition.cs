using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Orbital", fileName = "OrbitalAttackDelivery")]
public class OrbitalAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private int orbitalCount = 3;
    [SerializeField] private float orbitRadius = 2.2f;
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float hitCooldown = 0.35f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        GameObject orbitalObject = PoolManager.Spawn(DefaultRuntimePrefabFactory.GetOrbitalAttackPrefab(), context.Owner.transform.position, Quaternion.identity);
        RuntimeOrbitalAttack orbital = orbitalObject.GetComponent<RuntimeOrbitalAttack>();
        orbital.Initialize(
            context.Owner.transform,
            Mathf.Max(1, orbitalCount),
            context.SkillContext.ResolveAreaRadius(orbitRadius),
            Mathf.Max(0.05f, context.SkillContext.ResolveAreaRadius(hitRadius)),
            Mathf.Max(0.05f, duration),
            rotationSpeed,
            Mathf.Max(0.02f, hitCooldown),
            payload.Damage,
            payload.ToStatusArray(),
            context.SkillContext.SkillData.VisualPrefab);
    }
}

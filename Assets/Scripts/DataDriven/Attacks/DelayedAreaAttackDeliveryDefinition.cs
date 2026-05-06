using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Delayed Area", fileName = "DelayedAreaAttackDelivery")]
public class DelayedAreaAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private float warningHeight = 0.06f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.SkillContext?.SkillData == null || targets == null || targets.Count == 0 || payload == null)
        {
            return;
        }

        float resolvedRadius = context.SkillContext.ResolveAreaRadius(radius);
        StatusEffectData[] statuses = payload.ToStatusArray();

        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 position = targets[i].Position;
            position.y = warningHeight;

            GameObject strikeObject = PoolManager.Spawn(DefaultRuntimePrefabFactory.GetDelayedAreaStrikePrefab(), position, Quaternion.identity);
            RuntimeDelayedAreaStrike strike = strikeObject.GetComponent<RuntimeDelayedAreaStrike>();
            strike.Initialize(position, resolvedRadius, delay, payload.Damage, statuses, context.SkillContext.SkillData.VisualPrefab);
        }
    }
}

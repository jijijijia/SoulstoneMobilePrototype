using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Lasting Area", fileName = "LastingAreaAttackDelivery")]
public class LastingAreaAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float visualHeight = 0.08f;

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
            position.y = visualHeight;

            GameObject prefab = context.SkillContext.SkillData.VisualPrefab != null
                ? context.SkillContext.SkillData.VisualPrefab
                : DefaultRuntimePrefabFactory.GetShockwavePrefab();

            GameObject zoneObject = PoolManager.Spawn(prefab, position, Quaternion.identity);
            RuntimeDamageZone zone = zoneObject.GetComponent<RuntimeDamageZone>();

            if (zone == null)
            {
                zone = zoneObject.AddComponent<RuntimeDamageZone>();
                PoolManager.MarkPoolableCacheDirty(zoneObject);
            }

            zone.Initialize(resolvedRadius, duration, tickInterval, payload.Damage, statuses);
        }
    }
}

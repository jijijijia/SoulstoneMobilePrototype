using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Summon", fileName = "SummonAttackDelivery")]
public class SummonAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private int summonCount = 1;
    [SerializeField] private float summonDuration = 10f;
    [SerializeField] private float summonMoveSpeed = 5f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float spawnRadius = 1.2f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        GameObject summonPrefab = context.SkillContext.SkillData.VisualPrefab;
        int count = Mathf.Max(1, summonCount);
        StatusEffectData[] statuses = payload.ToStatusArray();

        for (int i = 0; i < count; i++)
        {
            float angle = 360f * i / count;
            Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * spawnRadius;
            Vector3 position = context.Owner.transform.position + offset;
            position.y = context.Owner.transform.position.y;

            GameObject summonObject = summonPrefab != null
                ? Object.Instantiate(summonPrefab, position, Quaternion.identity)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            summonObject.name = "RuntimeSummon";
            RuntimeSummonedMinion minion = summonObject.GetComponent<RuntimeSummonedMinion>();

            if (minion == null)
            {
                minion = summonObject.AddComponent<RuntimeSummonedMinion>();
            }

            minion.Initialize(
                context.Owner.transform,
                summonDuration,
                summonMoveSpeed,
                attackRange,
                attackInterval,
                payload.Damage,
                statuses);
        }
    }
}

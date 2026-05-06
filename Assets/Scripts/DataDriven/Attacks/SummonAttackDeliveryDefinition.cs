using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Summon", fileName = "SummonAttackDelivery")]
public class SummonAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private int summonCount = 5;
    [SerializeField] private int maxActiveSummons = 10;
    [SerializeField] private float summonDuration = 10f;
    [SerializeField] private float summonHealth = 90f;
    [SerializeField] private int summonLives = 1;
    [SerializeField] private float summonMoveSpeed = 5f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float spawnRadius = 1.2f;
    [SerializeField] private float enemyAggroRadius = 8f;

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context?.Owner == null || payload == null)
        {
            return;
        }

        GameObject summonPrefab = context.SkillContext.SkillData.VisualPrefab;
        int count = Mathf.Max(1, summonCount);
        int summonLimit = Mathf.Max(1, maxActiveSummons);
        int activeSummons = RuntimeSummonRegistry.GetActiveCount(context.Owner.transform);
        StatusEffectData[] statuses = payload.ToStatusArray();

        float angleOffset = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            if (activeSummons >= summonLimit)
            {
                return;
            }

            float angle = angleOffset + 360f * i / count;
            Vector3 offset = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * spawnRadius;
            Vector3 position = context.Owner.transform.position + offset;
            position.y = context.Owner.transform.position.y;

            GameObject prefab = summonPrefab != null ? summonPrefab : DefaultRuntimePrefabFactory.GetSummonPrefab();
            GameObject summonObject = PoolManager.Spawn(prefab, position, Quaternion.identity);

            summonObject.name = "RuntimeSummon";
            ForcePlaceSummon(summonObject, position);
            RuntimeSummonedMinion minion = summonObject.GetComponent<RuntimeSummonedMinion>();

            if (minion == null)
            {
                minion = summonObject.AddComponent<RuntimeSummonedMinion>();
                PoolManager.MarkPoolableCacheDirty(summonObject);
            }

            minion.Initialize(
                context.Owner.transform,
                summonDuration,
                summonHealth,
                summonLives,
                summonMoveSpeed,
                attackRange,
                attackInterval,
                enemyAggroRadius,
                payload.Damage,
                statuses);

            activeSummons++;
        }
    }

    private static void ForcePlaceSummon(GameObject summonObject, Vector3 position)
    {
        if (summonObject == null)
        {
            return;
        }

        CharacterController controller = summonObject.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        summonObject.transform.SetPositionAndRotation(position, Quaternion.identity);

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}

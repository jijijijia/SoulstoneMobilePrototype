using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Delivery/Chain", fileName = "ChainAttackDelivery")]
public class ChainAttackDeliveryDefinition : AttackDeliveryDefinition
{
    [SerializeField] private int maxJumps = 4;
    [SerializeField] private float jumpRadius = 5f;
    [SerializeField] private float damageFalloffPerJump = 0.15f;

    private readonly List<EnemyAgent> candidates = new();
    private readonly List<EnemyAgent> hitEnemies = new();

    public override void Deliver(AttackRuntimeContext context, IReadOnlyList<AttackTargetData> targets, AttackResolvedPayload payload)
    {
        if (context == null || payload == null || targets == null || targets.Count == 0)
        {
            return;
        }

        EnemyAgent current = targets[0].Enemy;

        if (current == null)
        {
            return;
        }

        StatusEffectData[] statuses = payload.ToStatusArray();
        hitEnemies.Clear();

        int jumps = Mathf.Max(1, maxJumps);

        for (int i = 0; i < jumps && current != null; i++)
        {
            float damageMultiplier = Mathf.Max(0.1f, 1f - damageFalloffPerJump * i);
            current.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(payload.Damage * damageMultiplier)));
            current.ApplyStatuses(statuses);
            hitEnemies.Add(current);

            current = FindNextTarget(current.transform.position);
        }
    }

    private EnemyAgent FindNextTarget(Vector3 origin)
    {
        EnemyRegistry.GetEnemiesInRadius(origin, jumpRadius, candidates);

        EnemyAgent closest = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            EnemyAgent candidate = candidates[i];

            if (candidate == null || hitEnemies.Contains(candidate))
            {
                continue;
            }

            float distanceSqr = (candidate.transform.position - origin).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = candidate;
            }
        }

        return closest;
    }
}

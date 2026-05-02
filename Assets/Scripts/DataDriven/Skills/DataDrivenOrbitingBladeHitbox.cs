using System.Collections.Generic;
using UnityEngine;

public class DataDrivenOrbitingBladeHitbox : MonoBehaviour, IPoolable
{
    private readonly Dictionary<EnemyAgent, float> lastHitTimes = new();

    private float hitCooldown;
    private StatusEffectData[] statusPayload;
    private SkillRuntimeContext context;
    private int rank;
    private float baseDamage;
    private float damagePerRank;

    public void Configure(SkillRuntimeContext skillContext, int currentRank, float attackBaseDamage, float attackDamagePerRank, float cooldown, StatusEffectData[] statuses)
    {
        context = skillContext;
        rank = currentRank;
        baseDamage = attackBaseDamage;
        damagePerRank = attackDamagePerRank;
        hitCooldown = cooldown;
        statusPayload = statuses;
    }

    private void OnDisable()
    {
        lastHitTimes.Clear();
    }

    public void OnTakenFromPool()
    {
        lastHitTimes.Clear();
    }

    public void OnReturnedToPool()
    {
        lastHitTimes.Clear();
        context = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        EnemyAgent enemy = other.GetComponent<EnemyAgent>();

        if (enemy == null)
        {
            return;
        }

        if (lastHitTimes.TryGetValue(enemy, out float lastHitTime) && Time.time - lastHitTime < hitCooldown)
        {
            return;
        }

        lastHitTimes[enemy] = Time.time;
        int damage = context != null
            ? context.ResolveDamage(baseDamage, damagePerRank, rank, 1f)
            : Mathf.RoundToInt(baseDamage);
        enemy.TakeDamage(damage);
        enemy.ApplyStatuses(statusPayload);
    }
}

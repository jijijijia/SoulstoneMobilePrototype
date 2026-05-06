using UnityEngine;

public class RuntimeSummonedMinion : MonoBehaviour, IPoolable
{
    private Transform owner;
    private StatusEffectData[] statuses;
    private float duration;
    private float maxHealth;
    private float currentHealth;
    private float moveSpeed;
    private float attackRange;
    private float attackInterval;
    private float enemyAggroRadius;
    private int damage;
    private int remainingLives;
    private float lifeTimer;
    private float attackTimer;
    private bool isDespawning;

    public Transform Owner => owner;
    public float EnemyAggroRadius => enemyAggroRadius;
    public bool IsAlive => !isDespawning && currentHealth > 0f && gameObject.activeInHierarchy;

    public void Initialize(Transform ownerTransform, float activeDuration, float health, int lives, float speed, float range, float interval, float aggroRadius, int summonDamage, StatusEffectData[] summonStatuses)
    {
        owner = ownerTransform;
        duration = Mathf.Max(0.1f, activeDuration);
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
        moveSpeed = Mathf.Max(0f, speed);
        attackRange = Mathf.Max(0.1f, range);
        attackInterval = Mathf.Max(0.05f, interval);
        enemyAggroRadius = Mathf.Max(0.1f, aggroRadius);
        damage = Mathf.Max(0, summonDamage);
        remainingLives = Mathf.Max(1, lives);
        statuses = summonStatuses;
        lifeTimer = 0f;
        attackTimer = attackInterval;
        isDespawning = false;

        Collider collider = GetComponent<Collider>();

        if (collider != null)
        {
            collider.isTrigger = false;
        }

        RuntimeSummonRegistry.Register(this);
    }

    private void OnDisable()
    {
        RuntimeSummonRegistry.Unregister(this);
    }

    public void TakeDamage(float incomingDamage)
    {
        if (isDespawning)
        {
            return;
        }

        currentHealth -= Mathf.Max(0f, incomingDamage);

        if (currentHealth > 0f)
        {
            return;
        }

        remainingLives--;

        if (remainingLives > 0)
        {
            currentHealth = maxHealth;
            return;
        }

        isDespawning = true;
        PoolManager.Release(gameObject);
    }

    private void Update()
    {
        if (isDespawning)
        {
            return;
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= duration)
        {
            isDespawning = true;
            PoolManager.Release(gameObject);
            return;
        }

        EnemyAgent target = EnemyRegistry.GetClosestEnemy(transform.position, 24f);

        if (target == null)
        {
            FollowOwnerLoosely();
            return;
        }

        MoveToward(target.transform.position);
        TryAttack(target);
    }

    private void FollowOwnerLoosely()
    {
        if (owner == null)
        {
            return;
        }

        Vector3 toOwner = owner.position - transform.position;
        toOwner.y = 0f;

        if (toOwner.sqrMagnitude <= 4f)
        {
            return;
        }

        transform.position += toOwner.normalized * moveSpeed * Time.deltaTime;
    }

    private void MoveToward(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= attackRange * attackRange)
        {
            return;
        }

        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        transform.position += movement;
        transform.forward = direction.normalized;
    }

    private void TryAttack(EnemyAgent target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 delta = target.transform.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude > attackRange * attackRange)
        {
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;

        if (damage > 0)
        {
            target.TakeDamageFromSummon(damage);
        }

        target.ApplyStatuses(statuses);
    }

    public void OnTakenFromPool()
    {
        lifeTimer = 0f;
        attackTimer = 0f;
        isDespawning = false;
    }

    public void OnReturnedToPool()
    {
        RuntimeSummonRegistry.Unregister(this);
        owner = null;
        statuses = null;
        currentHealth = 0f;
        remainingLives = 0;
        lifeTimer = 0f;
        attackTimer = 0f;
        isDespawning = true;
    }
}

public static class RuntimeSummonRegistry
{
    private static readonly System.Collections.Generic.HashSet<RuntimeSummonedMinion> activeSummons = new();

    public static void Register(RuntimeSummonedMinion summon)
    {
        if (summon != null)
        {
            activeSummons.Add(summon);
        }
    }

    public static void Unregister(RuntimeSummonedMinion summon)
    {
        if (summon != null)
        {
            activeSummons.Remove(summon);
        }
    }

    public static int GetActiveCount(Transform owner)
    {
        int count = 0;

        foreach (RuntimeSummonedMinion summon in activeSummons)
        {
            if (summon == null || !summon.IsAlive || summon.Owner != owner)
            {
                continue;
            }

            count++;
        }

        return count;
    }

    public static RuntimeSummonedMinion GetClosestSummon(Vector3 origin, float maxDistance)
    {
        RuntimeSummonedMinion closest = null;
        float closestDistanceSqr = maxDistance * maxDistance;

        foreach (RuntimeSummonedMinion summon in activeSummons)
        {
            if (summon == null || !summon.IsAlive)
            {
                continue;
            }

            Vector3 delta = summon.transform.position - origin;
            delta.y = 0f;
            float distanceSqr = delta.sqrMagnitude;

            if (distanceSqr > closestDistanceSqr || distanceSqr > summon.EnemyAggroRadius * summon.EnemyAggroRadius)
            {
                continue;
            }

            closestDistanceSqr = distanceSqr;
            closest = summon;
        }

        return closest;
    }
}

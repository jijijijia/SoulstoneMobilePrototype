using UnityEngine;

public class ModularProjectile : MonoBehaviour, IPoolable
{
    private readonly System.Collections.Generic.HashSet<EnemyAgent> hitEnemies = new();

    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifeTime;
    private int damage;
    private StatusEffectData[] statuses;
    private int remainingPierces;
    private int remainingRicochets;
    private float ricochetSearchRadius;
    private float lifeTimer;
    private bool isActive;

    public void Initialize(Vector3 direction, float speed, float duration, int projectileDamage, StatusEffectData[] appliedStatuses)
    {
        Initialize(direction, speed, duration, projectileDamage, appliedStatuses, 0, 0, 6f);
    }

    public void Initialize(Vector3 direction, float speed, float duration, int projectileDamage, StatusEffectData[] appliedStatuses, int pierceCount, int ricochetCount, float searchRadius)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifeTime = duration;
        damage = projectileDamage;
        statuses = appliedStatuses;
        remainingPierces = Mathf.Max(0, pierceCount);
        remainingRicochets = Mathf.Max(0, ricochetCount);
        ricochetSearchRadius = Mathf.Max(0.1f, searchRadius);
        lifeTimer = 0f;
        hitEnemies.Clear();
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            PoolManager.Release(gameObject);
            return;
        }

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyAgent enemy = other.GetComponent<EnemyAgent>();

        if (enemy == null)
        {
            return;
        }

        enemy.TakeDamage(damage);
        enemy.ApplyStatuses(statuses);
        hitEnemies.Add(enemy);

        if (TryRicochet(enemy))
        {
            return;
        }

        if (remainingPierces > 0)
        {
            remainingPierces--;
            return;
        }

        PoolManager.Release(gameObject);
    }

    private bool TryRicochet(EnemyAgent sourceEnemy)
    {
        if (remainingRicochets <= 0 || sourceEnemy == null)
        {
            return false;
        }

        EnemyAgent nextTarget = FindRicochetTarget(sourceEnemy.transform.position);

        if (nextTarget == null)
        {
            return false;
        }

        Vector3 targetPosition = nextTarget.transform.position;
        targetPosition.y = transform.position.y;
        Vector3 nextDirection = targetPosition - transform.position;

        if (nextDirection.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        remainingRicochets--;
        moveDirection = nextDirection.normalized;
        return true;
    }

    private EnemyAgent FindRicochetTarget(Vector3 origin)
    {
        EnemyRegistry.GetEnemiesInRadius(origin, ricochetSearchRadius, EnemySearchBuffer.Results);

        EnemyAgent closest = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < EnemySearchBuffer.Results.Count; i++)
        {
            EnemyAgent candidate = EnemySearchBuffer.Results[i];

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

    public void OnTakenFromPool()
    {
        lifeTimer = 0f;
        isActive = false;
        moveDirection = Vector3.zero;
        statuses = null;
        hitEnemies.Clear();
    }

    public void OnReturnedToPool()
    {
        lifeTimer = 0f;
        isActive = false;
        moveDirection = Vector3.zero;
        statuses = null;
        hitEnemies.Clear();
    }

    private static class EnemySearchBuffer
    {
        public static readonly System.Collections.Generic.List<EnemyAgent> Results = new();
    }
}

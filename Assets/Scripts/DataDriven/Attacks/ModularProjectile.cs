using UnityEngine;

public class ModularProjectile : MonoBehaviour, IPoolable
{
    private readonly System.Collections.Generic.HashSet<EnemyAgent> hitEnemies = new();
    private readonly System.Collections.Generic.List<EnemyAgent> inheritedHitsBuffer = new();

    private GameObject prefabSource;
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifeTime;
    private int damage;
    private StatusEffectData[] statuses;
    private int remainingPierces;
    private int remainingRicochets;
    private float ricochetSearchRadius;
    private int remainingForks;
    private int forkProjectileCount;
    private float forkSpreadAngle;
    private float lifeTimer;
    private bool isActive;

    public void Initialize(Vector3 direction, float speed, float duration, int projectileDamage, StatusEffectData[] appliedStatuses)
    {
        Initialize(direction, speed, duration, projectileDamage, appliedStatuses, 0, 0, 6f, 0, 0, 35f, null);
    }

    public void Initialize(Vector3 direction, float speed, float duration, int projectileDamage, StatusEffectData[] appliedStatuses, int pierceCount, int ricochetCount, float searchRadius)
    {
        Initialize(direction, speed, duration, projectileDamage, appliedStatuses, pierceCount, ricochetCount, searchRadius, 0, 0, 35f, null);
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float duration,
        int projectileDamage,
        StatusEffectData[] appliedStatuses,
        int pierceCount,
        int ricochetCount,
        float searchRadius,
        int forkGenerations,
        int forkCount,
        float forkAngle,
        GameObject sourcePrefab)
    {
        InitializeInternal(
            direction,
            speed,
            duration,
            projectileDamage,
            appliedStatuses,
            pierceCount,
            ricochetCount,
            searchRadius,
            forkGenerations,
            forkCount,
            forkAngle,
            sourcePrefab,
            null);
    }

    private void InitializeInternal(
        Vector3 direction,
        float speed,
        float duration,
        int projectileDamage,
        StatusEffectData[] appliedStatuses,
        int pierceCount,
        int ricochetCount,
        float searchRadius,
        int forkGenerations,
        int forkCount,
        float forkAngle,
        GameObject sourcePrefab,
        System.Collections.Generic.IEnumerable<EnemyAgent> inheritedHits)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifeTime = duration;
        damage = projectileDamage;
        statuses = appliedStatuses;
        remainingPierces = Mathf.Max(0, pierceCount);
        remainingRicochets = Mathf.Max(0, ricochetCount);
        ricochetSearchRadius = Mathf.Max(0.1f, searchRadius);
        remainingForks = Mathf.Max(0, forkGenerations);
        forkProjectileCount = Mathf.Max(0, forkCount);
        forkSpreadAngle = Mathf.Max(0f, forkAngle);
        prefabSource = sourcePrefab;
        lifeTimer = 0f;
        hitEnemies.Clear();

        if (inheritedHits != null)
        {
            foreach (EnemyAgent enemy in inheritedHits)
            {
                if (enemy != null)
                {
                    hitEnemies.Add(enemy);
                }
            }
        }

        isActive = true;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        }
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

        if (enemy == null || hitEnemies.Contains(enemy))
        {
            return;
        }

        enemy.TakeDamage(damage);
        enemy.ApplyStatuses(statuses);
        hitEnemies.Add(enemy);

        if (TryFork())
        {
            PoolManager.Release(gameObject);
            return;
        }

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

    private bool TryFork()
    {
        if (remainingForks <= 0 || forkProjectileCount <= 0)
        {
            return false;
        }

        GameObject projectilePrefab = prefabSource != null ? prefabSource : DefaultRuntimePrefabFactory.GetModularProjectilePrefab();
        int count = Mathf.Max(2, forkProjectileCount);
        float totalSpread = Mathf.Max(0f, forkSpreadAngle);
        float startAngle = count == 1 ? 0f : -totalSpread * 0.5f;
        float angleStep = count == 1 ? 0f : totalSpread / (count - 1);

        inheritedHitsBuffer.Clear();
        inheritedHitsBuffer.AddRange(hitEnemies);

        for (int i = 0; i < count; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(startAngle + angleStep * i, Vector3.up);
            Vector3 forkDirection = rotation * moveDirection;
            GameObject forkObject = PoolManager.Spawn(projectilePrefab, transform.position, Quaternion.identity);
            ModularProjectile forkProjectile = forkObject.GetComponent<ModularProjectile>();

            if (forkProjectile == null)
            {
                forkProjectile = forkObject.AddComponent<ModularProjectile>();
                PoolManager.MarkPoolableCacheDirty(forkObject);
            }

            forkProjectile.InitializeInternal(
                forkDirection,
                moveSpeed,
                Mathf.Max(0.1f, lifeTime - lifeTimer),
                damage,
                statuses,
                remainingPierces,
                remainingRicochets,
                ricochetSearchRadius,
                remainingForks - 1,
                forkProjectileCount,
                forkSpreadAngle,
                projectilePrefab,
                inheritedHitsBuffer);
        }

        inheritedHitsBuffer.Clear();
        return true;
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
        prefabSource = null;
        hitEnemies.Clear();
        inheritedHitsBuffer.Clear();
    }

    public void OnReturnedToPool()
    {
        lifeTimer = 0f;
        isActive = false;
        moveDirection = Vector3.zero;
        statuses = null;
        prefabSource = null;
        hitEnemies.Clear();
        inheritedHitsBuffer.Clear();
    }

    private static class EnemySearchBuffer
    {
        public static readonly System.Collections.Generic.List<EnemyAgent> Results = new();
    }
}

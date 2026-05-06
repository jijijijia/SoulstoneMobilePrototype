using UnityEngine;

public sealed class EnemyProjectileShooter
{
    public void Fire(Vector3 sourcePosition, Vector3 targetPosition, EnemyData enemyData, float damage)
    {
        GameObject projectilePrefab = enemyData != null && enemyData.ProjectilePrefab != null
            ? enemyData.ProjectilePrefab
            : DefaultRuntimePrefabFactory.GetEnemyProjectilePrefab();
        Vector3 spawnPosition = sourcePosition + Vector3.up * 0.8f;
        GameObject projectileObject = PoolManager.Spawn(projectilePrefab, spawnPosition, Quaternion.identity);

        if (projectileObject == null)
        {
            return;
        }

        projectileObject.transform.localScale = Vector3.one * (enemyData != null ? enemyData.ProjectileScale : 0.4f);
        ApplyProjectileVisual(projectileObject, enemyData);

        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<EnemyProjectile>();
            PoolManager.MarkPoolableCacheDirty(projectileObject);
        }

        Vector3 direction = (targetPosition - spawnPosition).normalized;
        projectile.Initialize(
            direction,
            enemyData != null ? enemyData.ProjectileSpeed : 10f,
            enemyData != null ? enemyData.ProjectileLifetime : 4f,
            damage,
            enemyData != null ? enemyData.AttackStatuses : null);
    }

    private static void ApplyProjectileVisual(GameObject projectileObject, EnemyData enemyData)
    {
        Renderer projectileRenderer = projectileObject.GetComponent<Renderer>();

        if (projectileRenderer == null || projectileRenderer.material == null || !projectileRenderer.material.HasProperty("_Color"))
        {
            return;
        }

        projectileRenderer.material.color = enemyData != null ? enemyData.ProjectileColor : Color.green;
    }
}

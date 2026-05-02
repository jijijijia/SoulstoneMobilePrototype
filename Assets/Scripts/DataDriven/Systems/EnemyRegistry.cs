using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistry
{
    private static readonly HashSet<EnemyAgent> activeEnemies = new();
    private static readonly List<EnemyAgent> randomCandidatesBuffer = new();

    public static void Register(EnemyAgent enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
        }
    }

    public static void Unregister(EnemyAgent enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
        }
    }

    public static EnemyAgent GetClosestEnemy(Vector3 origin, float maxDistance)
    {
        EnemyAgent closestEnemy = null;
        float closestDistanceSqr = maxDistance * maxDistance;

        foreach (EnemyAgent enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public static void GetClosestEnemies(Vector3 origin, float maxDistance, List<EnemyAgent> results, int maxCount)
    {
        results.Clear();
        float maxDistanceSqr = maxDistance * maxDistance;

        foreach (EnemyAgent enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;

            if (distanceSqr > maxDistanceSqr)
            {
                continue;
            }

            results.Add(enemy);
        }

        results.Sort((left, right) =>
            (left.transform.position - origin).sqrMagnitude.CompareTo((right.transform.position - origin).sqrMagnitude));

        if (results.Count > maxCount)
        {
            results.RemoveRange(maxCount, results.Count - maxCount);
        }
    }

    public static void GetRandomEnemies(Vector3 origin, float maxDistance, List<EnemyAgent> results, int maxCount)
    {
        results.Clear();
        randomCandidatesBuffer.Clear();
        float maxDistanceSqr = maxDistance * maxDistance;

        foreach (EnemyAgent enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            if ((enemy.transform.position - origin).sqrMagnitude <= maxDistanceSqr)
            {
                randomCandidatesBuffer.Add(enemy);
            }
        }

        int count = Mathf.Min(maxCount, randomCandidatesBuffer.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, randomCandidatesBuffer.Count);
            results.Add(randomCandidatesBuffer[randomIndex]);
            randomCandidatesBuffer.RemoveAt(randomIndex);
        }
    }

    public static void GetEnemiesInRadius(Vector3 origin, float radius, List<EnemyAgent> results)
    {
        results.Clear();
        float radiusSqr = radius * radius;

        foreach (EnemyAgent enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            if ((enemy.transform.position - origin).sqrMagnitude <= radiusSqr)
            {
                results.Add(enemy);
            }
        }
    }

    public static bool HasEnemyWithinDistance(Vector3 origin, float radius)
    {
        float radiusSqr = radius * radius;

        foreach (EnemyAgent enemy in activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 delta = enemy.transform.position - origin;
            delta.y = 0f;

            if (delta.sqrMagnitude <= radiusSqr)
            {
                return true;
            }
        }

        return false;
    }
}

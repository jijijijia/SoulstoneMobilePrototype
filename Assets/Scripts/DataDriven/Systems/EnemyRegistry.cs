using System.Collections.Generic;
using UnityEngine;

public static class EnemyRegistry
{
    private const float CellSize = 6f;

    private static readonly HashSet<EnemyAgent> activeEnemies = new();
    private static readonly List<EnemyAgent> randomCandidatesBuffer = new();
    private static readonly Dictionary<Vector2Int, List<EnemyAgent>> enemiesByCell = new();
    private static readonly Dictionary<EnemyAgent, Vector2Int> enemyCells = new();

    private static Vector3 sortOrigin;

    private static int CompareByDistanceToSortOrigin(EnemyAgent left, EnemyAgent right)
        => GetFlatDistanceSqr(left.transform.position, sortOrigin)
            .CompareTo(GetFlatDistanceSqr(right.transform.position, sortOrigin));

    public static void Register(EnemyAgent enemy)
    {
        if (enemy == null || !activeEnemies.Add(enemy))
        {
            return;
        }

        AddToCell(enemy, GetCell(enemy.transform.position));
    }

    public static void Unregister(EnemyAgent enemy)
    {
        if (enemy == null)
        {
            return;
        }

        activeEnemies.Remove(enemy);
        RemoveFromCurrentCell(enemy);
    }

    public static void UpdatePosition(EnemyAgent enemy)
    {
        if (enemy == null || !activeEnemies.Contains(enemy))
        {
            return;
        }

        Vector2Int newCell = GetCell(enemy.transform.position);

        if (enemyCells.TryGetValue(enemy, out Vector2Int oldCell) && oldCell == newCell)
        {
            return;
        }

        RemoveFromCurrentCell(enemy);
        AddToCell(enemy, newCell);
    }

    public static EnemyAgent GetClosestEnemy(Vector3 origin, float maxDistance)
    {
        EnemyAgent closestEnemy = null;
        float closestDistanceSqr = maxDistance * maxDistance;
        Vector2Int centerCell = GetCell(origin);
        int cellRadius = GetCellRadius(maxDistance);

        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                if (!enemiesByCell.TryGetValue(new Vector2Int(x, y), out List<EnemyAgent> enemies))
                {
                    continue;
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    EnemyAgent enemy = enemies[i];

                    if (!IsSearchable(enemy))
                    {
                        RemoveDeadCellEntry(enemies, i, enemy);
                        continue;
                    }

                    float distanceSqr = GetFlatDistanceSqr(enemy.transform.position, origin);

                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestEnemy = enemy;
                    }
                }
            }
        }

        return closestEnemy;
    }

    public static void GetClosestEnemies(Vector3 origin, float maxDistance, List<EnemyAgent> results, int maxCount)
    {
        results.Clear();
        float maxDistanceSqr = maxDistance * maxDistance;
        Vector2Int centerCell = GetCell(origin);
        int cellRadius = GetCellRadius(maxDistance);

        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                if (!enemiesByCell.TryGetValue(new Vector2Int(x, y), out List<EnemyAgent> enemies))
                {
                    continue;
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    EnemyAgent enemy = enemies[i];

                    if (!IsSearchable(enemy))
                    {
                        RemoveDeadCellEntry(enemies, i, enemy);
                        continue;
                    }

                    if (GetFlatDistanceSqr(enemy.transform.position, origin) <= maxDistanceSqr)
                    {
                        results.Add(enemy);
                    }
                }
            }
        }

        sortOrigin = origin;
        results.Sort(CompareByDistanceToSortOrigin);

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
        Vector2Int centerCell = GetCell(origin);
        int cellRadius = GetCellRadius(maxDistance);

        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                if (!enemiesByCell.TryGetValue(new Vector2Int(x, y), out List<EnemyAgent> enemies))
                {
                    continue;
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    EnemyAgent enemy = enemies[i];

                    if (!IsSearchable(enemy))
                    {
                        RemoveDeadCellEntry(enemies, i, enemy);
                        continue;
                    }

                    if (GetFlatDistanceSqr(enemy.transform.position, origin) <= maxDistanceSqr)
                    {
                        randomCandidatesBuffer.Add(enemy);
                    }
                }
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
        Vector2Int centerCell = GetCell(origin);
        int cellRadius = GetCellRadius(radius);

        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                if (!enemiesByCell.TryGetValue(new Vector2Int(x, y), out List<EnemyAgent> enemies))
                {
                    continue;
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    EnemyAgent enemy = enemies[i];

                    if (!IsSearchable(enemy))
                    {
                        RemoveDeadCellEntry(enemies, i, enemy);
                        continue;
                    }

                    if (GetFlatDistanceSqr(enemy.transform.position, origin) <= radiusSqr)
                    {
                        results.Add(enemy);
                    }
                }
            }
        }
    }

    public static bool HasEnemyWithinDistance(Vector3 origin, float radius)
    {
        float radiusSqr = radius * radius;
        Vector2Int centerCell = GetCell(origin);
        int cellRadius = GetCellRadius(radius);

        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                if (!enemiesByCell.TryGetValue(new Vector2Int(x, y), out List<EnemyAgent> enemies))
                {
                    continue;
                }

                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    EnemyAgent enemy = enemies[i];

                    if (!IsSearchable(enemy))
                    {
                        RemoveDeadCellEntry(enemies, i, enemy);
                        continue;
                    }

                    if (GetFlatDistanceSqr(enemy.transform.position, origin) <= radiusSqr)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static Vector2Int GetCell(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / CellSize),
            Mathf.FloorToInt(position.z / CellSize));
    }

    private static int GetCellRadius(float radius)
    {
        return Mathf.Max(0, Mathf.CeilToInt(radius / CellSize));
    }

    private static void AddToCell(EnemyAgent enemy, Vector2Int cell)
    {
        if (!enemiesByCell.TryGetValue(cell, out List<EnemyAgent> enemies))
        {
            enemies = new List<EnemyAgent>();
            enemiesByCell[cell] = enemies;
        }

        enemies.Add(enemy);
        enemyCells[enemy] = cell;
    }

    private static void RemoveFromCurrentCell(EnemyAgent enemy)
    {
        if (!enemyCells.TryGetValue(enemy, out Vector2Int cell))
        {
            return;
        }

        enemyCells.Remove(enemy);

        if (!enemiesByCell.TryGetValue(cell, out List<EnemyAgent> enemies))
        {
            return;
        }

        enemies.Remove(enemy);

        if (enemies.Count == 0)
        {
            enemiesByCell.Remove(cell);
        }
    }

    private static void RemoveDeadCellEntry(List<EnemyAgent> enemies, int index, EnemyAgent enemy)
    {
        enemies.RemoveAt(index);

        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
            enemyCells.Remove(enemy);
        }
    }

    private static bool IsSearchable(EnemyAgent enemy)
    {
        return enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDead;
    }

    private static float GetFlatDistanceSqr(Vector3 left, Vector3 right)
    {
        float dx = left.x - right.x;
        float dz = left.z - right.z;
        return dx * dx + dz * dz;
    }
}

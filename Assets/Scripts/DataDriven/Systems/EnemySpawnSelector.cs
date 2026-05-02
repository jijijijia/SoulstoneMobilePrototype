using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnSelector
{
    private readonly List<float> weightsBuffer = new();

    public void FillEligibleEnemies(List<EnemyData> buffer, EnemyData[] enemyPool, SpawnTimelineData spawnTimeline, float normalizedTime, int killCount)
    {
        buffer.Clear();

        if (enemyPool == null)
        {
            return;
        }

        foreach (EnemyData enemy in enemyPool)
        {
            if (enemy == null)
            {
                continue;
            }

            if (enemy.Prefab == null || killCount < enemy.KillRequirement)
            {
                continue;
            }

            if (spawnTimeline != null &&
                (!spawnTimeline.TryGetEnemyWeight(enemy, normalizedTime, out float weight) || weight <= 0f))
            {
                continue;
            }

            buffer.Add(enemy);
        }
    }

    public EnemyData PickEnemyByWeight(List<EnemyData> candidates, SpawnTimelineData spawnTimeline, float normalizedTime)
    {
        weightsBuffer.Clear();
        float totalWeight = 0f;

        foreach (EnemyData candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            float weight = GetWeight(candidate, spawnTimeline, normalizedTime);
            weightsBuffer.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            EnemyData candidate = candidates[i];

            if (candidate == null)
            {
                continue;
            }

            cumulative += weightsBuffer[i];

            if (randomValue <= cumulative)
            {
                return candidate;
            }
        }

        return null;
    }

    private static float GetWeight(EnemyData candidate, SpawnTimelineData spawnTimeline, float normalizedTime)
    {
        if (spawnTimeline != null && spawnTimeline.TryGetEnemyWeight(candidate, normalizedTime, out float weight))
        {
            return Mathf.Max(0f, weight);
        }

        return candidate.SpawnWeight;
    }
}

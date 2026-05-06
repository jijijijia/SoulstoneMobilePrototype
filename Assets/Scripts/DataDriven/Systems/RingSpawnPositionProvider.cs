using System.Collections.Generic;
using UnityEngine;

public sealed class RingSpawnPositionProvider
{
    public Vector3 FindSpawnPosition(
        Transform playerTransform,
        EnemyData enemyData,
        SpawnTimelineData spawnTimeline,
        float fallbackSpawnRadius,
        Vector3 playerMoveDirection,
        List<Vector3> reservedPositions,
        bool safePadding,
        MapBoundsResolver mapBounds,
        float mapPadding,
        float minimumSpawnSeparation,
        float activeEnemySpawnSeparation,
        int ringSampleCount,
        float ringRadiusJitter)
    {
        if (playerTransform == null)
        {
            return Vector3.zero;
        }

        float radius = spawnTimeline != null ? spawnTimeline.SpawnDistance : fallbackSpawnRadius;
        float directionBias = spawnTimeline != null ? spawnTimeline.PlayerDirectionBias : 0f;
        float baseAngle = GetBaseAngle(playerMoveDirection, directionBias);
        int samples = Mathf.Max(8, ringSampleCount);
        float padding = safePadding
            ? Mathf.Max(mapPadding, enemyData != null ? enemyData.VisualScaleMultiplier * 1.6f : mapPadding)
            : mapPadding;

        int startIndex = Random.Range(0, samples);

        for (int i = 0; i < samples; i++)
        {
            int sampleIndex = (startIndex + i) % samples;
            float angle = baseAngle + (sampleIndex / (float)samples) * Mathf.PI * 2f;
            float jitterMultiplier = 1f + Random.Range(-ringRadiusJitter, ringRadiusJitter);
            Vector3 direction = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 candidate = playerTransform.position + direction * radius * jitterMultiplier;
            candidate.y = 0.5f;

            if (IsValidSpawnPoint(candidate, reservedPositions, padding, mapBounds, minimumSpawnSeparation, activeEnemySpawnSeparation))
            {
                return candidate;
            }
        }

        return GetFallbackSpawnPosition(playerTransform, reservedPositions, padding, mapBounds, minimumSpawnSeparation, activeEnemySpawnSeparation);
    }

    private static bool IsValidSpawnPoint(
        Vector3 candidate,
        List<Vector3> reservedPositions,
        float padding,
        MapBoundsResolver mapBounds,
        float minimumSpawnSeparation,
        float activeEnemySpawnSeparation)
    {
        if (mapBounds == null || !mapBounds.Contains(candidate, padding))
        {
            return false;
        }

        return HasEnoughSeparation(candidate, reservedPositions, minimumSpawnSeparation, activeEnemySpawnSeparation);
    }

    private static Vector3 GetFallbackSpawnPosition(
        Transform playerTransform,
        List<Vector3> reservedPositions,
        float padding,
        MapBoundsResolver mapBounds,
        float minimumSpawnSeparation,
        float activeEnemySpawnSeparation)
    {
        if (mapBounds == null)
        {
            return playerTransform.position;
        }

        Vector2 min = mapBounds.Min;
        Vector2 max = mapBounds.Max;

        for (int i = 0; i < 48; i++)
        {
            Vector3 candidate = new(
                Random.Range(min.x + padding, max.x - padding),
                0.5f,
                Random.Range(min.y + padding, max.y - padding));

            if (HasEnoughSeparation(candidate, reservedPositions, minimumSpawnSeparation, activeEnemySpawnSeparation))
            {
                return candidate;
            }
        }

        return new Vector3(
            Mathf.Clamp(playerTransform.position.x, min.x + padding, max.x - padding),
            0.5f,
            Mathf.Clamp(playerTransform.position.z, min.y + padding, max.y - padding));
    }

    private static float GetBaseAngle(Vector3 moveDirection, float directionBias)
    {
        if (moveDirection.sqrMagnitude <= 0.001f || directionBias <= 0f)
        {
            return Random.Range(0f, Mathf.PI * 2f);
        }

        Vector3 biasedDirection = Vector3.Slerp(Vector3.forward, moveDirection.normalized, directionBias).normalized;
        return Mathf.Atan2(biasedDirection.z, biasedDirection.x);
    }

    private static bool HasEnoughSeparation(
        Vector3 candidate,
        List<Vector3> reservedPositions,
        float minimumSpawnSeparation,
        float activeEnemySpawnSeparation)
    {
        if (reservedPositions != null && reservedPositions.Count > 0 && minimumSpawnSeparation > 0.01f)
        {
            float minimumDistanceSqr = minimumSpawnSeparation * minimumSpawnSeparation;

            for (int i = 0; i < reservedPositions.Count; i++)
            {
                Vector3 delta = reservedPositions[i] - candidate;
                delta.y = 0f;

                if (delta.sqrMagnitude < minimumDistanceSqr)
                {
                    return false;
                }
            }
        }

        if (activeEnemySpawnSeparation > 0.01f && EnemyRegistry.HasEnemyWithinDistance(candidate, activeEnemySpawnSeparation))
        {
            return false;
        }

        return true;
    }
}

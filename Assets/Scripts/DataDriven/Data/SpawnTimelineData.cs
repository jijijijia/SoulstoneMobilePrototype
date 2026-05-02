using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Spawn Timeline", fileName = "SpawnTimelineData")]
public class SpawnTimelineData : ScriptableObject
{
    [SerializeField] private float runDuration = 600f;
    [SerializeField] private float spawnDistance = 18f;
    [SerializeField, Range(0f, 1f)] private float playerDirectionBias = 0.45f;
    [SerializeField, Range(0f, 1f)] private float lateralJitter = 0.35f;
    [SerializeField] private TimelineFloatKeyframe[] spawnRateKeyframes;
    [SerializeField] private TimelineIntKeyframe[] packSizeKeyframes;
    [SerializeField] private TimelineIntKeyframe[] maxAliveKeyframes;
    [SerializeField] private TimelineFloatKeyframe[] healthMultiplierKeyframes;
    [SerializeField] private TimelineFloatKeyframe[] damageMultiplierKeyframes;
    [SerializeField] private EnemyWeightTrack[] enemyWeightTracks;
    [SerializeField] private SpawnBurstEvent[] burstEvents;

    public float RunDuration => Mathf.Max(1f, runDuration);
    public float SpawnDistance => Mathf.Max(1f, spawnDistance);
    public float PlayerDirectionBias => playerDirectionBias;
    public float LateralJitter => lateralJitter;
    public SpawnBurstEvent[] BurstEvents => burstEvents;

    public float GetNormalizedTime(float elapsedTime)
    {
        return Mathf.Clamp01(elapsedTime / RunDuration);
    }

    public float GetSpawnRate(float normalizedTime, float fallback = 0.5f)
    {
        return Mathf.Max(0f, EvaluateFloat(spawnRateKeyframes, normalizedTime, fallback));
    }

    public int GetPackSize(float normalizedTime, int fallback = 1)
    {
        return Mathf.Max(1, EvaluateInt(packSizeKeyframes, normalizedTime, fallback));
    }

    public int GetMaxAlive(float normalizedTime, int fallback = 20)
    {
        return Mathf.Max(1, EvaluateInt(maxAliveKeyframes, normalizedTime, fallback));
    }

    public float GetHealthMultiplier(float normalizedTime, float fallback = 1f)
    {
        return Mathf.Max(0.1f, EvaluateFloat(healthMultiplierKeyframes, normalizedTime, fallback));
    }

    public float GetDamageMultiplier(float normalizedTime, float fallback = 1f)
    {
        return Mathf.Max(0.1f, EvaluateFloat(damageMultiplierKeyframes, normalizedTime, fallback));
    }

    public bool TryGetEnemyWeight(EnemyData enemy, float normalizedTime, out float weight)
    {
        if (enemyWeightTracks != null)
        {
            foreach (EnemyWeightTrack track in enemyWeightTracks)
            {
                if (track == null || track.Enemy != enemy)
                {
                    continue;
                }

                weight = Mathf.Max(0f, EvaluateFloat(track.Keyframes, normalizedTime, 0f));
                return true;
            }
        }

        weight = 0f;
        return false;
    }

    private static float EvaluateFloat(TimelineFloatKeyframe[] keyframes, float normalizedTime, float fallback)
    {
        if (keyframes == null || keyframes.Length == 0)
        {
            return fallback;
        }

        normalizedTime = Mathf.Clamp01(normalizedTime);

        if (normalizedTime <= keyframes[0].t)
        {
            return keyframes[0].value;
        }

        for (int i = 1; i < keyframes.Length; i++)
        {
            if (normalizedTime > keyframes[i].t)
            {
                continue;
            }

            float segmentT = Mathf.InverseLerp(keyframes[i - 1].t, keyframes[i].t, normalizedTime);
            return Mathf.Lerp(keyframes[i - 1].value, keyframes[i].value, segmentT);
        }

        return keyframes[keyframes.Length - 1].value;
    }

    private static int EvaluateInt(TimelineIntKeyframe[] keyframes, float normalizedTime, int fallback)
    {
        if (keyframes == null || keyframes.Length == 0)
        {
            return fallback;
        }

        normalizedTime = Mathf.Clamp01(normalizedTime);

        if (normalizedTime <= keyframes[0].t)
        {
            return keyframes[0].value;
        }

        for (int i = 1; i < keyframes.Length; i++)
        {
            if (normalizedTime > keyframes[i].t)
            {
                continue;
            }

            float segmentT = Mathf.InverseLerp(keyframes[i - 1].t, keyframes[i].t, normalizedTime);
            return Mathf.RoundToInt(Mathf.Lerp(keyframes[i - 1].value, keyframes[i].value, segmentT));
        }

        return keyframes[keyframes.Length - 1].value;
    }
}

[System.Serializable]
public class EnemyWeightTrack
{
    [SerializeField] private EnemyData enemy;
    [SerializeField] private TimelineFloatKeyframe[] keyframes;

    public EnemyData Enemy => enemy;
    public TimelineFloatKeyframe[] Keyframes => keyframes;
}

[System.Serializable]
public class TimelineFloatKeyframe
{
    [Range(0f, 1f)] public float t;
    public float value = 1f;
}

[System.Serializable]
public class TimelineIntKeyframe
{
    [Range(0f, 1f)] public float t;
    public int value = 1;
}

[System.Serializable]
public class SpawnBurstEvent
{
    public string eventId = "burst";
    [Range(0f, 1f)] public float t;
    public EnemyData enemy;
    public int count = 1;
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;
}

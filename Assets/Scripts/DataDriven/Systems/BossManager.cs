using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private SpawnSystem spawnSystem;
    [SerializeField] private EnemyData[] bosses;
    [SerializeField] private int killsPerBoss = 50;
    [SerializeField] private TimedBossEvent[] timedBossEvents;

    private int spawnedBosses;
    private int spawnedTimedBosses;

    private void OnEnable()
    {
        if (spawnSystem == null)
        {
            spawnSystem = FindFirstObjectByType<SpawnSystem>();
        }

        spawnedBosses = 0;
        spawnedTimedBosses = 0;

        if (spawnSystem != null)
        {
            spawnSystem.KillCountChanged += HandleKillCountChanged;
        }
    }

    private void OnDisable()
    {
        if (spawnSystem != null)
        {
            spawnSystem.KillCountChanged -= HandleKillCountChanged;
        }
    }

    private void Update()
    {
        if (spawnSystem == null)
        {
            spawnSystem = FindFirstObjectByType<SpawnSystem>();
        }

        if (spawnSystem == null || timedBossEvents == null || timedBossEvents.Length == 0)
        {
            return;
        }

        while (spawnedTimedBosses < timedBossEvents.Length)
        {
            TimedBossEvent bossEvent = timedBossEvents[spawnedTimedBosses];

            if (bossEvent == null || bossEvent.Boss == null)
            {
                spawnedTimedBosses++;
                continue;
            }

            if (spawnSystem.NormalizedTime < bossEvent.TriggerTime)
            {
                break;
            }

            spawnSystem.SpawnSpecificEnemy(bossEvent.Boss);
            spawnedTimedBosses++;
        }
    }

    private void HandleKillCountChanged(int kills)
    {
        if (bosses == null || bosses.Length == 0)
        {
            return;
        }

        int expectedBossCount = kills / Mathf.Max(1, killsPerBoss);

        if (expectedBossCount <= spawnedBosses)
        {
            return;
        }

        EnemyData boss = bosses[Mathf.Min(spawnedBosses, bosses.Length - 1)];
        spawnSystem.SpawnSpecificEnemy(boss);
        spawnedBosses++;
    }
}

[System.Serializable]
public class TimedBossEvent
{
    [SerializeField, Range(0f, 1f)] private float triggerTime = 0.8f;
    [SerializeField] private EnemyData boss;

    public float TriggerTime => triggerTime;
    public EnemyData Boss => boss;
}

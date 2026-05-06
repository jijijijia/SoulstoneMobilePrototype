using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarView : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text healthValueText;

    private SpawnSystem spawnSystem;
    private EnemyAgent trackedBoss;

    private void Start()
    {
        ResolveSpawnSystem();
        HideContainer();
    }

    private void OnEnable()
    {
        ResolveSpawnSystem();
    }

    private void OnDisable()
    {
        UnsubscribeFromSpawnSystem();
    }

    private void Update()
    {
        if (trackedBoss == null || trackedBoss.IsDead)
        {
            if (trackedBoss != null)
            {
                trackedBoss = null;
                HideContainer();
            }

            return;
        }

        float currentHealth = trackedBoss.CurrentHealth;
        float maxHealth = trackedBoss.MaxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = Mathf.Max(1f, maxHealth);
            healthSlider.value = Mathf.Max(0f, currentHealth);
        }

        if (healthValueText != null)
        {
            healthValueText.text = $"{Mathf.CeilToInt(Mathf.Max(0f, currentHealth))}/{Mathf.RoundToInt(maxHealth)}";
        }
    }

    private void HandleBossSpawned(EnemyAgent boss)
    {
        trackedBoss = boss;

        if (nameText != null)
        {
            nameText.text = boss.Data != null ? boss.Data.DisplayName : string.Empty;
        }

        float maxHealth = boss.MaxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = Mathf.Max(1f, maxHealth);
            healthSlider.value = maxHealth;
        }

        ShowContainer();
    }

    private void HandleBossKilled(EnemyAgent boss)
    {
        if (trackedBoss == boss)
        {
            trackedBoss = null;
            HideContainer();
        }
    }

    private void ResolveSpawnSystem()
    {
        if (spawnSystem != null)
        {
            return;
        }

        spawnSystem = FindFirstObjectByType<SpawnSystem>();

        if (spawnSystem != null)
        {
            spawnSystem.BossEnemySpawned += HandleBossSpawned;
            spawnSystem.BossEnemyKilled += HandleBossKilled;
        }
    }

    private void UnsubscribeFromSpawnSystem()
    {
        if (spawnSystem != null)
        {
            spawnSystem.BossEnemySpawned -= HandleBossSpawned;
            spawnSystem.BossEnemyKilled -= HandleBossKilled;
        }
    }

    private void ShowContainer()
    {
        if (container != null)
        {
            container.SetActive(true);
        }
    }

    private void HideContainer()
    {
        trackedBoss = null;

        if (container != null)
        {
            container.SetActive(false);
        }
    }
}

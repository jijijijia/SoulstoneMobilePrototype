using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Run Affix", fileName = "RunAffixData")]
public class RunAffixData : ScriptableObject
{
    [SerializeField] private string affixId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private float spawnRateMultiplier = 1f;
    [SerializeField] private int packSizeBonus;
    [SerializeField] private int maxAliveBonus;
    [SerializeField] private float enemyHealthMultiplier = 1f;
    [SerializeField] private float enemyDamageMultiplier = 1f;
    [SerializeField] private float experienceMultiplier = 1f;

    public string AffixId => affixId;
    public string DisplayName => displayName;
    public string Description => description;
    public float SpawnRateMultiplier => Mathf.Max(0.1f, spawnRateMultiplier);
    public int PackSizeBonus => packSizeBonus;
    public int MaxAliveBonus => maxAliveBonus;
    public float EnemyHealthMultiplier => Mathf.Max(0.1f, enemyHealthMultiplier);
    public float EnemyDamageMultiplier => Mathf.Max(0.1f, enemyDamageMultiplier);
    public float ExperienceMultiplier => Mathf.Max(0.1f, experienceMultiplier);
}

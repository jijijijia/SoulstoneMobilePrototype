using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterSystem : MonoBehaviour
{
    [SerializeField] private CharacterData characterData;
    [SerializeField] private bool initializeOnAwake = true;

    private RuntimeStats runtimeStats;
    private WeaponSystem weaponSystem;
    private SkillSystem skillSystem;
    private UpgradeSystem upgradeSystem;
    private ProgressionSystem progressionSystem;
    private ProgressionChoiceGenerator progressionChoiceGenerator;
    private StatusController statusController;
    private GameObject instantiatedModel;
    private readonly CharacterRuntimeState runtimeState = new();
    private float invulnerabilityTimer;

    public event Action<float, float> HealthChanged
    {
        add => runtimeState.HealthChanged += value;
        remove => runtimeState.HealthChanged -= value;
    }

    public event Action Died
    {
        add => runtimeState.Died += value;
        remove => runtimeState.Died -= value;
    }

    public CharacterData CharacterData => characterData;
    public RuntimeStats RuntimeStats => runtimeStats;
    public WeaponSystem WeaponSystem => weaponSystem;
    public SkillSystem SkillSystem => skillSystem;
    public UpgradeSystem UpgradeSystem => upgradeSystem;
    public ProgressionSystem ProgressionSystem => progressionSystem;
    public ProgressionChoiceGenerator ProgressionChoiceGenerator => progressionChoiceGenerator;
    public StatusController StatusController => statusController;
    public float CurrentHealth => runtimeState.CurrentHealth;
    public float MaxHealth => runtimeState.MaxHealth;
    public bool IsDead => runtimeState.IsDead;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;
    public int RemainingTotems => Mathf.Max(0, Mathf.FloorToInt(RuntimeStats?.GetValue(StatType.LifeTotemCount) ?? 0f) - runtimeState.SpentTotems);

    private void Awake()
    {
        if (initializeOnAwake)
        {
            CharacterData initialData = SelectedCharacterStore.ResolveSelectedCharacter(characterData);
            InitializeFromData(initialData);
        }
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        ApplyHealthRegeneration();
    }

    public void InitializeFromData(CharacterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("CharacterData is not assigned.");
            return;
        }

        if (runtimeStats != null)
        {
            runtimeStats.StatsChanged -= HandleStatsChanged;
        }

        characterData = data;
        EnsureSubsystems();
        ResetSubsystemState();

        runtimeStats = new RuntimeStats();
        runtimeStats.Initialize(characterData.BaseStats);
        runtimeStats.StatsChanged += HandleStatsChanged;

        EnsureModel();
        statusController.Initialize(this, runtimeStats);

        skillSystem.Initialize(this);
        upgradeSystem.Initialize(this);
        weaponSystem.Initialize(this, skillSystem);
        progressionChoiceGenerator = new ProgressionChoiceGenerator();
        progressionSystem.Initialize(this, skillSystem, upgradeSystem, progressionChoiceGenerator);

        WeaponData selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(characterData);

        if (selectedWeapon != null)
        {
            weaponSystem.Equip(selectedWeapon);
        }

        runtimeState.Reset(runtimeStats.GetValue(StatType.MaxHealth));
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable || IsDead)
        {
            return;
        }

        if (TryParryHit())
        {
            return;
        }

        float resolvedDamage = ResolveIncomingDamage(damage);
        int availableTotems = Mathf.Max(0, Mathf.FloorToInt(runtimeStats.GetValue(StatType.LifeTotemCount)));
        runtimeState.ApplyDamage(resolvedDamage, availableTotems, 0.5f);
    }

    public void RestoreHealth(float amount)
    {
        runtimeState.RestoreHealth(amount);
    }

    public void TriggerInvulnerability(float duration)
    {
        invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, duration);
    }

    public float GetSkillAttackPower()
    {
        return runtimeStats != null ? runtimeStats.GetValue(StatType.AttackPower) : 1f;
    }

    private void EnsureModel()
    {
        if (characterData.ModelPrefab == null)
        {
            return;
        }

        if (instantiatedModel != null)
        {
            Destroy(instantiatedModel);
            instantiatedModel = null;
        }

        instantiatedModel = Instantiate(characterData.ModelPrefab, transform);
        instantiatedModel.name = $"{characterData.DisplayName}_Model";
    }

    private void EnsureSubsystems()
    {
        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<WeaponSystem>();
        }

        if (weaponSystem == null)
        {
            weaponSystem = gameObject.AddComponent<WeaponSystem>();
        }

        if (skillSystem == null)
        {
            skillSystem = GetComponent<SkillSystem>();
        }

        if (skillSystem == null)
        {
            skillSystem = gameObject.AddComponent<SkillSystem>();
        }

        if (upgradeSystem == null)
        {
            upgradeSystem = GetComponent<UpgradeSystem>();
        }

        if (upgradeSystem == null)
        {
            upgradeSystem = gameObject.AddComponent<UpgradeSystem>();
        }

        if (progressionSystem == null)
        {
            progressionSystem = GetComponent<ProgressionSystem>();
        }

        if (progressionSystem == null)
        {
            progressionSystem = gameObject.AddComponent<ProgressionSystem>();
        }

        if (statusController == null)
        {
            statusController = GetComponent<StatusController>();
        }

        if (statusController == null)
        {
            statusController = gameObject.AddComponent<StatusController>();
        }
    }

    private void HandleStatsChanged()
    {
        if (runtimeStats == null)
        {
            return;
        }

        runtimeState.ReconcileMaxHealth(runtimeStats.GetValue(StatType.MaxHealth));
    }

    private void ApplyHealthRegeneration()
    {
        if (runtimeStats == null || IsDead || CurrentHealth >= MaxHealth)
        {
            return;
        }

        float regenerationPercent = runtimeStats.GetValue(StatType.HealthRegenPercent);

        if (regenerationPercent <= 0f)
        {
            return;
        }

        float regenerationAmount = MaxHealth * regenerationPercent * Time.deltaTime;

        if (regenerationAmount > 0f)
        {
            RestoreHealth(regenerationAmount);
        }
    }

    private bool TryParryHit()
    {
        float parryChance = Mathf.Clamp01(runtimeStats.GetValue(StatType.ParryChance));
        return parryChance > 0f && UnityEngine.Random.value <= parryChance;
    }

    private float ResolveIncomingDamage(float rawDamage)
    {
        float defense = Mathf.Max(0f, runtimeStats.GetValue(StatType.Defense));
        float damageMultiplier = CombatMath.ResolveDefenseDamageMultiplier(defense);
        return Mathf.Max(0f, rawDamage * damageMultiplier);
    }

    private void ResetSubsystemState()
    {
        invulnerabilityTimer = 0f;
        progressionSystem?.ResetState();
        weaponSystem?.ResetState();
        upgradeSystem?.ResetState();
        skillSystem?.ResetState();
        statusController?.ResetState();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class StatusController : MonoBehaviour
{
    private sealed class StatusRuntime
    {
        public StatusEffectType EffectType;
        public float RemainingDuration;
        public float Potency;
        public int Stacks;
        public float TickInterval;
        public float TickTimer;
    }

    private static readonly Dictionary<StatusEffectType, string> StatusSourceIds = new()
    {
        { StatusEffectType.Poison,    "status::Poison"    },
        { StatusEffectType.Burn,      "status::Burn"      },
        { StatusEffectType.Slow,      "status::Slow"      },
        { StatusEffectType.Weakness,  "status::Weakness"  },
        { StatusEffectType.Bleed,     "status::Bleed"     },
        { StatusEffectType.Doom,      "status::Doom"      },
        { StatusEffectType.Shattered, "status::Shattered" },
        { StatusEffectType.Dazed,     "status::Dazed"     },
        { StatusEffectType.Brittle,   "status::Brittle"   },
        { StatusEffectType.Fragility, "status::Fragility" },
    };

    private readonly Dictionary<StatusEffectType, StatusRuntime> activeStatuses = new();
    private readonly List<StatusEffectType> updateKeysBuffer = new();
    private readonly List<StatusEffectType> removalBuffer = new();
    private readonly List<StatModifierData> modifierBuffer = new();

    private CharacterSystem characterOwner;
    private EnemyAgent enemyOwner;
    private RuntimeStats runtimeStats;

    public event Action StatusesChanged;

    public void Initialize(CharacterSystem owner, RuntimeStats stats)
    {
        characterOwner = owner;
        enemyOwner = null;
        runtimeStats = stats;
        ResetState();
    }

    public void Initialize(EnemyAgent owner, RuntimeStats stats)
    {
        characterOwner = null;
        enemyOwner = owner;
        runtimeStats = stats;
        ResetState();
    }

    private void Update()
    {
        if (activeStatuses.Count == 0 || runtimeStats == null || IsOwnerDead())
        {
            return;
        }

        removalBuffer.Clear();
        updateKeysBuffer.Clear();

        foreach (StatusEffectType effectType in activeStatuses.Keys)
        {
            updateKeysBuffer.Add(effectType);
        }

        for (int i = 0; i < updateKeysBuffer.Count; i++)
        {
            StatusEffectType effectType = updateKeysBuffer[i];

            if (!activeStatuses.TryGetValue(effectType, out StatusRuntime status))
            {
                continue;
            }

            status.RemainingDuration -= Time.deltaTime;

            if (status.TickInterval > 0f)
            {
                status.TickTimer += Time.deltaTime;

                while (status.TickTimer >= status.TickInterval)
                {
                    status.TickTimer -= status.TickInterval;
                    ApplyTick(status);
                }
            }

            if (status.RemainingDuration <= 0f)
            {
                removalBuffer.Add(effectType);
            }
        }

        for (int i = 0; i < removalBuffer.Count; i++)
        {
            RemoveStatus(removalBuffer[i]);
        }
    }

    public void ApplyStatuses(StatusEffectData[] statuses)
    {
        if (statuses == null || statuses.Length == 0)
        {
            return;
        }

        for (int i = 0; i < statuses.Length; i++)
        {
            ApplyStatus(statuses[i]);
        }
    }

    public void ApplyStatus(StatusEffectData statusData)
    {
        if (statusData.effectType == StatusEffectType.None || statusData.duration <= 0f)
        {
            return;
        }

        int maxStacks = Mathf.Max(1, statusData.maxStacks);
        float tickInterval = RequiresTick(statusData.effectType)
            ? Mathf.Max(0.1f, statusData.tickInterval > 0f ? statusData.tickInterval : 1f)
            : 0f;

        if (!activeStatuses.TryGetValue(statusData.effectType, out StatusRuntime runtime))
        {
            runtime = new StatusRuntime
            {
                EffectType = statusData.effectType,
                RemainingDuration = statusData.duration,
                Potency = statusData.potency,
                Stacks = 1,
                TickInterval = tickInterval,
                TickTimer = 0f
            };

            activeStatuses.Add(statusData.effectType, runtime);
        }
        else
        {
            runtime.Stacks = Mathf.Clamp(runtime.Stacks + 1, 1, maxStacks);
            runtime.Potency = Mathf.Max(runtime.Potency, statusData.potency);
            runtime.TickInterval = tickInterval > 0f ? tickInterval : runtime.TickInterval;
            runtime.TickTimer = 0f;

            if (statusData.refreshDuration)
            {
                runtime.RemainingDuration = statusData.duration;
            }
            else
            {
                runtime.RemainingDuration = Mathf.Max(runtime.RemainingDuration, statusData.duration);
            }
        }

        RebuildStatusModifiers(statusData.effectType);
        StatusesChanged?.Invoke();
    }

    public void ResetState()
    {
        if (runtimeStats != null)
        {
            updateKeysBuffer.Clear();

            foreach (StatusEffectType statusType in activeStatuses.Keys)
            {
                updateKeysBuffer.Add(statusType);
            }

            for (int i = 0; i < updateKeysBuffer.Count; i++)
            {
                runtimeStats.RemoveModifiers(GetStatusSourceId(updateKeysBuffer[i]));
            }
        }

        activeStatuses.Clear();
        updateKeysBuffer.Clear();
        removalBuffer.Clear();
        StatusesChanged?.Invoke();
    }

    private void ApplyTick(StatusRuntime status)
    {
        float damagePerTick = CombatMath.ResolveStatusTickDamage(
            status.EffectType,
            status.Potency,
            status.Stacks,
            status.TickInterval);

        if (damagePerTick <= 0f)
        {
            return;
        }

        if (characterOwner != null)
        {
            characterOwner.TakeDamage(damagePerTick);
        }
        else if (enemyOwner != null)
        {
            enemyOwner.TakeDamage(damagePerTick);
        }
    }

    private void RemoveStatus(StatusEffectType effectType)
    {
        if (!activeStatuses.Remove(effectType))
        {
            return;
        }

        runtimeStats?.RemoveModifiers(GetStatusSourceId(effectType));
        StatusesChanged?.Invoke();
    }

    private void RebuildStatusModifiers(StatusEffectType effectType)
    {
        if (runtimeStats == null)
        {
            return;
        }

        modifierBuffer.Clear();

        if (activeStatuses.TryGetValue(effectType, out StatusRuntime status))
        {
            float totalPotency = status.Potency * status.Stacks;

            switch (effectType)
            {
                case StatusEffectType.Slow:
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.MoveSpeed,
                        additive = 0f,
                        multiplier = -Mathf.Clamp(totalPotency, 0f, 0.85f)
                    });
                    break;

                case StatusEffectType.Weakness:
                    float clampedWeakness = -Mathf.Clamp(totalPotency, 0f, 0.85f);
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.Damage,
                        additive = 0f,
                        multiplier = clampedWeakness
                    });
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.ContactDamage,
                        additive = 0f,
                        multiplier = clampedWeakness
                    });
                    break;

                case StatusEffectType.Shattered:
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.Armor,
                        additive = -Mathf.Clamp(totalPotency, 0f, 95f),
                        multiplier = 0f
                    });
                    break;

                case StatusEffectType.Dazed:
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.CritChance,
                        additive = Mathf.Clamp(totalPotency, 0f, 1f),
                        multiplier = 0f
                    });
                    break;

                case StatusEffectType.Brittle:
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.Armor,
                        additive = -Mathf.Clamp(totalPotency * 0.5f, 0f, 75f),
                        multiplier = 0f
                    });
                    break;

                case StatusEffectType.Fragility:
                    modifierBuffer.Add(new StatModifierData
                    {
                        statType = StatType.Damage,
                        additive = 0f,
                        multiplier = -Mathf.Clamp(totalPotency, 0f, 0.65f)
                    });
                    break;
            }
        }

        string sourceId = GetStatusSourceId(effectType);

        if (modifierBuffer.Count == 0)
        {
            runtimeStats.RemoveModifiers(sourceId);
        }
        else
        {
            runtimeStats.AddModifiers(sourceId, modifierBuffer);
        }
    }

    private bool IsOwnerDead()
    {
        return (characterOwner != null && characterOwner.IsDead) ||
               (enemyOwner != null && enemyOwner.IsDead);
    }

    private static bool RequiresTick(StatusEffectType effectType)
    {
        return effectType == StatusEffectType.Poison ||
               effectType == StatusEffectType.Burn ||
               effectType == StatusEffectType.Bleed ||
               effectType == StatusEffectType.Doom;
    }

    private static string GetStatusSourceId(StatusEffectType effectType)
    {
        return StatusSourceIds.TryGetValue(effectType, out string id) ? id : effectType.ToString();
    }
}

using UnityEngine;

public static class CombatMath
{
    public const float BaseEnemyArmor = 100f;
    public const float MinimumSkillCooldown = 0.15f;
    public const float DisplayCooldownFloor = 0.2f;
    public const float RecommendedStandardHeroHealth = 120f;
    public const float DefaultMulticastCascadeMultiplier = 0.4f;

    public static float ResolveArmorDamageMultiplier(float armor)
    {
        return armor >= BaseEnemyArmor
            ? 2f / ((armor / BaseEnemyArmor) + 1f)
            : (200f - armor) / 100f;
    }

    public static float ResolveDefenseDamageMultiplier(float defense)
    {
        return ResolveArmorDamageMultiplier(BaseEnemyArmor + Mathf.Max(0f, defense));
    }

    public static float ResolveCooldown(float baseCooldown, float cooldownReduction, int rank, float cooldownMultiplier, float skillFrequency)
    {
        float rankedCooldown = Mathf.Max(MinimumSkillCooldown, baseCooldown - Mathf.Max(0, rank - 1) * cooldownReduction);
        float legacyMultiplier = cooldownMultiplier > 0f ? Mathf.Max(0.05f, cooldownMultiplier) : 1f;
        float frequency = Mathf.Max(0.1f, skillFrequency);
        return Mathf.Max(MinimumSkillCooldown, rankedCooldown * legacyMultiplier / frequency);
    }

    public static AttackArchetypeProfile GetAttackArchetypeProfile(SkillHitType hitType)
    {
        return hitType switch
        {
            SkillHitType.Projectile => new AttackArchetypeProfile(22f, 55f, 0.85f, 1),
            SkillHitType.LobProjectile => new AttackArchetypeProfile(45f, 95f, 1.8f, 1),
            SkillHitType.MeleeArc => new AttackArchetypeProfile(55f, 120f, 1.4f, 1),
            SkillHitType.Frontal => new AttackArchetypeProfile(65f, 145f, 1.65f, 1),
            SkillHitType.AreaPulse => new AttackArchetypeProfile(70f, 160f, 2.2f, 1),
            SkillHitType.Burst => new AttackArchetypeProfile(95f, 220f, 3.5f, 1),
            SkillHitType.GroundAoe => new AttackArchetypeProfile(55f, 135f, 3.0f, 2),
            SkillHitType.LastingArea => new AttackArchetypeProfile(14f, 34f, 5.5f, 8),
            SkillHitType.Aura => new AttackArchetypeProfile(10f, 28f, 0.7f, 1),
            SkillHitType.Chain => new AttackArchetypeProfile(36f, 90f, 2.4f, 4),
            SkillHitType.Beam => new AttackArchetypeProfile(18f, 42f, 5.0f, 10),
            SkillHitType.Static => new AttackArchetypeProfile(30f, 75f, 1.3f, 1),
            SkillHitType.Summon => new AttackArchetypeProfile(24f, 60f, 6.0f, 1),
            SkillHitType.RandomStrike => new AttackArchetypeProfile(70f, 180f, 3.2f, 1),
            _ => new AttackArchetypeProfile(20f, 50f, 1.0f, 1)
        };
    }

    public static float ResolveExpectedMulticastCount(float multicastChance, float cascadeMultiplier = DefaultMulticastCascadeMultiplier, int maxExtraCasts = 8)
    {
        float chance = Mathf.Max(0f, multicastChance);
        float expected = 1f;
        float chainProbability = 1f;

        for (int i = 0; i < maxExtraCasts; i++)
        {
            float stepChance = Mathf.Min(1f, chance * Mathf.Pow(Mathf.Clamp01(cascadeMultiplier), i));
            chainProbability *= stepChance;
            expected += chainProbability;

            if (chainProbability <= 0.0001f)
            {
                break;
            }
        }

        return expected;
    }

    public static int RollExecutionCount(float doubleAttackChance)
    {
        int executions = 1;
        float chance = Mathf.Max(0f, doubleAttackChance);

        while (chance > 0f && executions < 8)
        {
            float stepChance = Mathf.Min(1f, chance * Mathf.Pow(DefaultMulticastCascadeMultiplier, executions - 1));

            if (Random.value > stepChance)
            {
                break;
            }

            executions++;
        }

        return executions;
    }

    public static float ResolveSkillDamage(
        float baseDamage,
        float damagePerRank,
        int rank,
        float attackPower,
        float flatDamageBonus,
        float ownerDamageMultiplier)
    {
        float rankedDamage = baseDamage + Mathf.Max(0, rank - 1) * damagePerRank;
        float totalAttackPower = Mathf.Max(0.05f, attackPower * ownerDamageMultiplier);
        return Mathf.Max(1f, rankedDamage * totalAttackPower + flatDamageBonus);
    }

    public static int RollCriticalDamage(float damage, float critChance, float critMultiplier)
    {
        float resolvedDamage = damage;

        if (Mathf.Clamp01(critChance) > 0f && Random.value <= Mathf.Clamp01(critChance))
        {
            resolvedDamage *= Mathf.Max(1f, critMultiplier);
        }

        return Mathf.Max(1, Mathf.RoundToInt(resolvedDamage));
    }

    public static float ResolveAreaRadius(float baseRadius, float flatAreaBonus, float areaMultiplier)
    {
        return Mathf.Max(0.05f, (baseRadius + flatAreaBonus) * Mathf.Max(0.1f, areaMultiplier));
    }

    public static float ResolveStatusTickDamage(StatusEffectType effectType, float potency, int stacks, float tickInterval)
    {
        float safePotency = Mathf.Max(0f, potency);
        int safeStacks = Mathf.Max(1, stacks);

        // Current project assets store potency as damage per tick. The table reference
        // helps us choose default durations/tick counts, without silently rebaking old data.
        return effectType switch
        {
            StatusEffectType.Poison => safePotency * safeStacks,
            StatusEffectType.Burn => safePotency * safeStacks,
            StatusEffectType.Bleed => safePotency * safeStacks,
            StatusEffectType.Doom => safePotency * safeStacks,
            _ => 0f
        };
    }

    public static float ResolveEnemyHealthMultiplier(float timelineMultiplier, float normalizedRunTime, EnemyCategory category)
    {
        float timeCurve = 1f + Mathf.Pow(Mathf.Clamp01(normalizedRunTime), 1.35f) * 1.45f;
        float categoryMultiplier = category switch
        {
            EnemyCategory.Empowered => 1.35f,
            EnemyCategory.Elite => 3.5f,
            EnemyCategory.MiniBoss => 12f,
            EnemyCategory.Boss => 35f,
            _ => 1f
        };

        return Mathf.Max(0.1f, timelineMultiplier) * timeCurve * categoryMultiplier;
    }

    public static float ResolveEnemyDamageMultiplier(float timelineMultiplier, float normalizedRunTime, EnemyCategory category)
    {
        float timeCurve = 1f + Mathf.Pow(Mathf.Clamp01(normalizedRunTime), 1.15f) * 0.75f;
        float categoryMultiplier = category switch
        {
            EnemyCategory.Empowered => 1.15f,
            EnemyCategory.Elite => 1.35f,
            EnemyCategory.MiniBoss => 1.75f,
            EnemyCategory.Boss => 2.5f,
            _ => 1f
        };

        return Mathf.Max(0.1f, timelineMultiplier) * timeCurve * categoryMultiplier;
    }
}

public readonly struct AttackArchetypeProfile
{
    public readonly float LightHitDamage;
    public readonly float HeavyHitDamage;
    public readonly float BaselineCooldown;
    public readonly int ExpectedHits;

    public AttackArchetypeProfile(float lightHitDamage, float heavyHitDamage, float baselineCooldown, int expectedHits)
    {
        LightHitDamage = lightHitDamage;
        HeavyHitDamage = heavyHitDamage;
        BaselineCooldown = baselineCooldown;
        ExpectedHits = expectedHits;
    }
}

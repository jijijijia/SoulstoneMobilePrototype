using System;
using UnityEngine;

[Serializable]
public class ConfigurableSkillAttackData
{
    public string attackId = "default_attack";
    public float baseCooldown = 1f;
    public float cooldownReductionPerRank = 0.05f;
    public ConfigurableSkillTargetingMode targetingMode = ConfigurableSkillTargetingMode.NearestEnemy;
    public int targetCount = 1;
    public float maxDistance = 12f;
    public ConfigurableSkillDeliveryMode deliveryMode = ConfigurableSkillDeliveryMode.Projectile;
    public float projectileSpeed = 14f;
    public float projectileLifetime = 2.5f;
    public float spawnHeight = 0.75f;
    public float areaRadius = 3f;
    public float areaVisualLifetime = 0.2f;
    public float baseDamage = 20f;
    public float damagePerRank = 6f;
    public float ownerDamageMultiplier = 1f;
}

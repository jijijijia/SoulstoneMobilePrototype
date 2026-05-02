using System.Collections.Generic;
using UnityEngine;

public class RuntimeOrbitalAttack : MonoBehaviour
{
    private readonly List<EnemyAgent> enemies = new();
    private readonly Dictionary<EnemyAgent, float> hitTimers = new();
    private readonly List<EnemyAgent> staleEnemies = new();
    private readonly List<Transform> orbitals = new();

    private Transform owner;
    private StatusEffectData[] statuses;
    private float orbitRadius;
    private float hitRadius;
    private float duration;
    private float rotationSpeed;
    private float hitCooldown;
    private int damage;
    private float timer;
    private float angle;

    public void Initialize(Transform ownerTransform, int count, float radius, float orbitalHitRadius, float activeDuration, float degreesPerSecond, float damageCooldown, int orbitalDamage, StatusEffectData[] orbitalStatuses, GameObject visualPrefab)
    {
        owner = ownerTransform;
        orbitRadius = Mathf.Max(0.1f, radius);
        hitRadius = Mathf.Max(0.05f, orbitalHitRadius);
        duration = Mathf.Max(0.05f, activeDuration);
        rotationSpeed = degreesPerSecond;
        hitCooldown = Mathf.Max(0.02f, damageCooldown);
        damage = Mathf.Max(0, orbitalDamage);
        statuses = orbitalStatuses;
        timer = 0f;
        angle = 0f;

        for (int i = 0; i < count; i++)
        {
            Transform visual = CreateOrbitalVisual(visualPrefab, i);
            orbitals.Add(visual);
        }
    }

    private void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        angle += rotationSpeed * Time.deltaTime;
        transform.position = owner.position;

        UpdateHitTimers();

        for (int i = 0; i < orbitals.Count; i++)
        {
            float orbitalAngle = angle + 360f * i / orbitals.Count;
            Vector3 offset = Quaternion.AngleAxis(orbitalAngle, Vector3.up) * Vector3.forward * orbitRadius;
            orbitals[i].position = owner.position + offset + Vector3.up * 0.75f;
            ApplyHitsAt(orbitals[i].position);
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private Transform CreateOrbitalVisual(GameObject visualPrefab, int index)
    {
        GameObject visual = visualPrefab != null
            ? Instantiate(visualPrefab, transform)
            : GameObject.CreatePrimitive(PrimitiveType.Sphere);

        visual.name = $"Orbital_{index}";
        visual.transform.SetParent(transform, false);
        visual.transform.localScale = new Vector3(hitRadius * 1.5f, hitRadius * 1.5f, hitRadius * 1.5f);

        Collider collider = visual.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        return visual.transform;
    }

    private void ApplyHitsAt(Vector3 position)
    {
        EnemyRegistry.GetEnemiesInRadius(position, hitRadius, enemies);

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAgent enemy = enemies[i];

            if (enemy == null || hitTimers.ContainsKey(enemy))
            {
                continue;
            }

            if (damage > 0)
            {
                enemy.TakeDamage(damage);
            }

            enemy.ApplyStatuses(statuses);
            hitTimers[enemy] = hitCooldown;
        }
    }

    private void UpdateHitTimers()
    {
        staleEnemies.Clear();

        foreach (KeyValuePair<EnemyAgent, float> entry in hitTimers)
        {
            if (entry.Key == null || entry.Value <= Time.deltaTime)
            {
                staleEnemies.Add(entry.Key);
            }
        }

        for (int i = 0; i < staleEnemies.Count; i++)
        {
            hitTimers.Remove(staleEnemies[i]);
        }

        for (int i = 0; i < staleEnemies.Count; i++)
        {
            // Reuse staleEnemies as a temporary list; dictionary values are updated in a second pass below.
        }

        List<EnemyAgent> keys = staleEnemies;
        keys.Clear();

        foreach (EnemyAgent enemy in hitTimers.Keys)
        {
            keys.Add(enemy);
        }

        for (int i = 0; i < keys.Count; i++)
        {
            hitTimers[keys[i]] -= Time.deltaTime;
        }
    }
}

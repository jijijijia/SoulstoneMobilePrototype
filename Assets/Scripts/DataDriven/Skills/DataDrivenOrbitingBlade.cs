using System.Collections.Generic;
using UnityEngine;

public class DataDrivenOrbitingBlade : SkillBehaviourBase
{
    private readonly List<DataDrivenOrbitingBladeHitbox> blades = new();
    private float orbitRadius;
    private float orbitSpeed;
    private float bladeScale;
    private float bladeHeight;
    private int bladeCount;
    private float baseBladeDamage;
    private float bladeDamagePerRank;
    private float hitCooldown;

    private void Update()
    {
        if (blades.Count == 0)
        {
            return;
        }

        float rotationOffset = Time.time * orbitSpeed;

        for (int i = 0; i < blades.Count; i++)
        {
            float angle = rotationOffset + i * (360f / blades.Count);
            float radians = angle * Mathf.Deg2Rad;

            Vector3 offset = new(
                Mathf.Cos(radians) * orbitRadius,
                bladeHeight,
                Mathf.Sin(radians) * orbitRadius);

            DataDrivenOrbitingBladeHitbox blade = blades[i];
            blade.transform.position = transform.position + offset;
            blade.transform.Rotate(0f, orbitSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    protected override void ApplyRank()
    {
        orbitRadius = Context.ResolveAreaRadius(Context.SkillData.GetParameter("orbitRadius", 2f));
        orbitSpeed = Context.SkillData.GetParameter("orbitSpeed", 180f);
        bladeScale = Context.SkillData.GetParameter("bladeScale", 0.45f);
        bladeHeight = Context.SkillData.GetParameter("bladeHeight", 0.8f);
        hitCooldown = Context.ResolveCooldown(Context.SkillData.GetParameter("hitCooldown", 0.35f), 0f, 1);
        bladeCount = Mathf.Max(1, Mathf.RoundToInt(Context.SkillData.GetParameter("bladeCount", 1f)) + (Rank - 1));
        baseBladeDamage = Context.SkillData.GetParameter("damage", 18f);
        bladeDamagePerRank = 5f;

        SyncBlades();
    }

    private void SyncBlades()
    {
        GameObject bladePrefab = Context.SkillData.VisualPrefab != null
            ? Context.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetOrbitBladePrefab();

        while (blades.Count < bladeCount)
        {
            GameObject bladeObject = PoolManager.Spawn(bladePrefab, transform.position, Quaternion.identity);
            bladeObject.transform.localScale = Vector3.one * bladeScale;
            DataDrivenOrbitingBladeHitbox hitbox = bladeObject.GetComponent<DataDrivenOrbitingBladeHitbox>();

            if (hitbox == null)
            {
                hitbox = bladeObject.AddComponent<DataDrivenOrbitingBladeHitbox>();
                PoolManager.MarkPoolableCacheDirty(bladeObject);
            }

            blades.Add(hitbox);
        }

        for (int i = 0; i < blades.Count; i++)
        {
            bool shouldBeActive = i < bladeCount;

            if (shouldBeActive)
            {
                blades[i].gameObject.SetActive(true);
                blades[i].transform.localScale = Vector3.one * bladeScale;
                blades[i].Configure(Context, Rank, baseBladeDamage, bladeDamagePerRank, hitCooldown, Context.SkillData.AppliedStatuses);
            }
            else
            {
                blades[i].gameObject.SetActive(false);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (DataDrivenOrbitingBladeHitbox blade in blades)
        {
            if (blade != null)
            {
                PoolManager.Release(blade.gameObject);
            }
        }

        blades.Clear();
    }
}

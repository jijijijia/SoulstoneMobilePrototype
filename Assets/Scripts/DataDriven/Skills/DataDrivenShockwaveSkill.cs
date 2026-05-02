using UnityEngine;

public class DataDrivenShockwaveSkill : SkillBehaviourBase
{
    private float pulseInterval;
    private float pulseRadius;
    private float timer;

    private void Update()
    {
        if (Context == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer < pulseInterval)
        {
            return;
        }

        timer = 0f;
        FirePulse();
    }

    protected override void ApplyRank()
    {
        pulseInterval = Context.ResolveCooldown(Context.SkillData.GetParameter("interval", 4f), 0.35f, Rank);
        pulseRadius = Context.ResolveAreaRadius(Context.SkillData.GetParameter("radius", 3.5f) + (Rank - 1) * 0.6f);
    }

    private void FirePulse()
    {
        int executionCount = Context.GetExecutionCount();

        for (int castIndex = 0; castIndex < executionCount; castIndex++)
        {
            int damage = Context.ResolveDamage(Context.SkillData.GetParameter("damage", 30f), 10f, Rank, 1f);
            Collider[] hits = Physics.OverlapSphere(transform.position, pulseRadius, ~0, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                EnemyAgent enemy = hit.GetComponent<EnemyAgent>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    enemy.ApplyStatuses(Context.SkillData.AppliedStatuses);
                }
            }

            ShowVisual();
        }
    }

    private void ShowVisual()
    {
        GameObject visualPrefab = Context.SkillData.VisualPrefab != null
            ? Context.SkillData.VisualPrefab
            : DefaultRuntimePrefabFactory.GetShockwavePrefab();

        GameObject visualObject = PoolManager.Spawn(visualPrefab, transform.position, Quaternion.identity);
        ShockwavePulseVisual pulseVisual = visualObject.GetComponent<ShockwavePulseVisual>();

        if (pulseVisual == null)
        {
            pulseVisual = visualObject.AddComponent<ShockwavePulseVisual>();
        }

        pulseVisual.Play(transform.position, pulseRadius, 0.18f);
    }
}

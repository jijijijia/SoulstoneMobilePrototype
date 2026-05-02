using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Soulstone/Data/Attack Targeting/Nearest Enemy", fileName = "NearestEnemyTargeting")]
public class NearestEnemyTargetingDefinition : AttackTargetingDefinition
{
    [SerializeField] private int targetCount = 1;
    [SerializeField] private float maxDistance = 12f;

    public override void ResolveTargets(AttackRuntimeContext context, List<AttackTargetData> results)
    {
        results.Clear();

        if (context?.Owner == null)
        {
            return;
        }

        List<EnemyAgent> enemies = new();
        float range = context.SkillContext.ApplyTaggedModifiers(
            maxDistance + context.SkillContext.OwnerStats.GetValue(StatType.AttackRange),
            StatType.AttackRange);

        EnemyRegistry.GetClosestEnemies(context.Owner.transform.position, range, enemies, Mathf.Max(1, targetCount));

        foreach (EnemyAgent enemy in enemies)
        {
            results.Add(new AttackTargetData
            {
                Enemy = enemy,
                Position = enemy.transform.position
            });
        }
    }
}

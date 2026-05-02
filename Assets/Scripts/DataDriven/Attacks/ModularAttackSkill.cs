using System.Collections.Generic;
using UnityEngine;

public class ModularAttackSkill : SkillBehaviourBase
{
    private sealed class AttackState
    {
        public AttackDefinition Definition;
        public float Cooldown;
        public float Timer;
    }

    private readonly List<AttackState> attackStates = new();
    private readonly List<AttackTargetData> targetBuffer = new();
    private readonly AttackResolvedPayload payloadBuffer = new();

    public override void Initialize(SkillRuntimeContext context, int rank)
    {
        base.Initialize(context, rank);
    }

    private void Update()
    {
        if (Context?.SkillData?.AttackDefinitions == null || attackStates.Count == 0)
        {
            return;
        }

        for (int i = 0; i < attackStates.Count; i++)
        {
            AttackState state = attackStates[i];
            state.Timer += Time.deltaTime;

            if (state.Timer < state.Cooldown)
            {
                continue;
            }

            if (Execute(state))
            {
                state.Timer = 0f;
            }
        }
    }

    protected override void ApplyRank()
    {
        if (Context?.SkillData?.AttackDefinitions == null)
        {
            return;
        }

        RebuildStates();
    }

    private void RebuildStates()
    {
        attackStates.Clear();

        if (Context?.SkillData?.AttackDefinitions == null)
        {
            return;
        }

        foreach (AttackDefinition attackDefinition in Context.SkillData.AttackDefinitions)
        {
            if (attackDefinition == null)
            {
                continue;
            }

            attackStates.Add(new AttackState
            {
                Definition = attackDefinition,
                Cooldown = attackDefinition.ResolveCooldown(Context, Rank),
                Timer = 0f
            });
        }
    }

    private bool Execute(AttackState attackState)
    {
        if (attackState?.Definition == null ||
            attackState.Definition.Targeting == null ||
            attackState.Definition.Delivery == null)
        {
            return false;
        }

        AttackRuntimeContext attackContext = new()
        {
            Owner = Context.Owner,
            SkillContext = Context,
            AttackDefinition = attackState.Definition,
            Rank = Rank,
            Cooldown = attackState.Cooldown
        };

        int executionCount = Context.GetExecutionCount();
        bool executedAny = false;

        for (int i = 0; i < executionCount; i++)
        {
            attackState.Definition.Targeting.ResolveTargets(attackContext, targetBuffer);

            if (targetBuffer.Count == 0)
            {
                continue;
            }

            payloadBuffer.Reset();

            if (attackState.Definition.Impacts != null)
            {
                foreach (AttackImpactDefinition impact in attackState.Definition.Impacts)
                {
                    impact?.BuildPayload(attackContext, payloadBuffer);
                }
            }

            attackState.Definition.Delivery.Deliver(attackContext, targetBuffer, payloadBuffer);
            executedAny = true;
        }

        return executedAny;
    }
}

using UnityEngine;

public abstract class SkillBehaviourBase : MonoBehaviour
{
    protected SkillRuntimeContext Context { get; private set; }
    protected int Rank { get; private set; }

    public virtual void Initialize(SkillRuntimeContext context, int rank)
    {
        Unsubscribe();
        Context = context;
        Rank = rank;
        Subscribe();
        ApplyRank();
    }

    public void SetRank(int rank)
    {
        Rank = rank;
        ApplyRank();
    }

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (Context?.OwnerStats != null)
        {
            Context.OwnerStats.StatsChanged += ApplyRank;
        }

        if (Context?.UpgradeSystem != null)
        {
            Context.UpgradeSystem.SkillModifiersChanged += ApplyRank;
        }
    }

    private void Unsubscribe()
    {
        if (Context?.OwnerStats != null)
        {
            Context.OwnerStats.StatsChanged -= ApplyRank;
        }

        if (Context?.UpgradeSystem != null)
        {
            Context.UpgradeSystem.SkillModifiersChanged -= ApplyRank;
        }
    }

    protected abstract void ApplyRank();

    protected void PlayOwnerAttackVisual()
    {
        Context?.Owner?.PlayAttackVisual();
    }
}

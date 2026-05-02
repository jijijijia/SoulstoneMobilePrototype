using System.Collections.Generic;

public class AttackResolvedPayload
{
    private readonly List<StatusEffectData> statuses = new();

    public int Damage { get; private set; }
    public IReadOnlyList<StatusEffectData> Statuses => statuses;

    public void Reset()
    {
        Damage = 0;
        statuses.Clear();
    }

    public void AddDamage(int value)
    {
        Damage += value;
    }

    public void AddStatuses(IEnumerable<StatusEffectData> entries)
    {
        if (entries == null)
        {
            return;
        }

        foreach (StatusEffectData entry in entries)
        {
            statuses.Add(entry);
        }
    }

    public StatusEffectData[] ToStatusArray()
    {
        return statuses.Count == 0 ? null : statuses.ToArray();
    }
}

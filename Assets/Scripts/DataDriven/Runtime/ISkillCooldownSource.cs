public interface ISkillCooldownSource
{
    float CooldownDuration { get; }
    float CooldownRemaining { get; }
    float CooldownNormalized { get; }
    bool IsReady { get; }
}

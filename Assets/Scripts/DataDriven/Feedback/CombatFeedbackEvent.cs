using UnityEngine;

public readonly struct CombatFeedbackEvent
{
    public readonly Transform Target;
    public readonly Vector3 WorldPosition;
    public readonly float DamageAmount;
    public readonly bool IsPlayerTarget;
    public readonly bool IsCritical;

    public CombatFeedbackEvent(Transform target, Vector3 worldPosition, float damageAmount, bool isPlayerTarget, bool isCritical = false)
    {
        Target = target;
        WorldPosition = worldPosition;
        DamageAmount = damageAmount;
        IsPlayerTarget = isPlayerTarget;
        IsCritical = isCritical;
    }
}

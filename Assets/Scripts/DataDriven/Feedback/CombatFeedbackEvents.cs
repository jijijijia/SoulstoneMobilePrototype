using System;

public static class CombatFeedbackEvents
{
    public static event Action<CombatFeedbackEvent> DamageTaken;

    public static void RaiseDamageTaken(CombatFeedbackEvent feedbackEvent)
    {
        DamageTaken?.Invoke(feedbackEvent);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

// Legacy bridge kept for old scenes. New rewards are owned by DDRunManager + RunRewardSystem
// and are saved to PlayerProfileData through SaveSystem.
public sealed class SoulsRewardSystem : MonoBehaviour
{
    private const int SoulsPerKill = 5;
    private const int VictoryBonus = 50;
    private const int SoulsPerMinuteSurvived = 10;
    private const float AffixRewardMultiplier = 1.25f;

    private DDRunManager runManager;
    private SpawnSystem spawnSystem;
    private bool hasAwardedThisRun;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureRuntimeInstance()
    {
        // Intentionally disabled: rewards are no longer auto-created hidden runtime services.
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        ResolveReferences();
        SubscribeToRunManager();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        UnsubscribeFromRunManager();
    }

    private void OnSceneChanged(Scene _, Scene __)
    {
        hasAwardedThisRun = false;
        UnsubscribeFromRunManager();
        ResolveReferences();
        SubscribeToRunManager();
    }

    private void ResolveReferences()
    {
        runManager = FindFirstObjectByType<DDRunManager>();
        spawnSystem = FindFirstObjectByType<SpawnSystem>();
    }

    private void SubscribeToRunManager()
    {
        if (runManager != null)
        {
            runManager.StateChanged += HandleStateChanged;
        }
    }

    private void UnsubscribeFromRunManager()
    {
        if (runManager != null)
        {
            runManager.StateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(DDRunState state)
    {
        return;

#pragma warning disable CS0162
        if (hasAwardedThisRun)
        {
            return;
        }

        if (state != DDRunState.GameOver && state != DDRunState.Victory)
        {
            return;
        }

        hasAwardedThisRun = true;
        int souls = CalculateSouls(state == DDRunState.Victory);
        MetaProgressionStore.AddSouls(souls);
        Debug.Log($"[SoulsRewardSystem] Run ended ({state}). Awarded {souls} souls. Total: {MetaProgressionStore.TotalSouls}");
#pragma warning restore CS0162
    }

    private int CalculateSouls(bool isVictory)
    {
        int kills = spawnSystem != null ? spawnSystem.KillCount : 0;
        float elapsed = runManager != null ? runManager.ElapsedRunTime : 0f;
        int affixCount = runManager != null ? runManager.ActiveAffixes.Count : 0;

        float baseSouls = kills * SoulsPerKill
            + Mathf.FloorToInt(elapsed / 60f) * SoulsPerMinuteSurvived
            + (isVictory ? VictoryBonus : 0);

        float multiplier = Mathf.Pow(AffixRewardMultiplier, affixCount);
        return Mathf.Max(1, Mathf.RoundToInt(baseSouls * multiplier));
    }
}

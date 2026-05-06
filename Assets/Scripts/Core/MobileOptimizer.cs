using UnityEngine;

public static class MobileOptimizer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Time.fixedDeltaTime = 0.02f;

#if UNITY_ANDROID || UNITY_IOS
        Application.lowMemory += OnLowMemory;
#endif
    }

    private static void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}

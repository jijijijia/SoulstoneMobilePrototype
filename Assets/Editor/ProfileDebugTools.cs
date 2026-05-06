using UnityEditor;
using UnityEngine;

public static class ProfileDebugTools
{
    private const string RootMenu = "Soulstone/Profile/";

    [MenuItem(RootMenu + "Use Profile 1")]
    public static void UseProfile1()
    {
        SetActiveProfile(0);
    }

    [MenuItem(RootMenu + "Use Profile 2")]
    public static void UseProfile2()
    {
        SetActiveProfile(1);
    }

    [MenuItem(RootMenu + "Use Profile 3")]
    public static void UseProfile3()
    {
        SetActiveProfile(2);
    }

    [MenuItem(RootMenu + "Clear Active Profile")]
    public static void ClearActiveProfile()
    {
        if (!EditorUtility.DisplayDialog(
                "Clear Soulstone Profile",
                $"Clear all saved choices for {GameProfileStore.ActiveProfileId}?",
                "Clear",
                "Cancel"))
        {
            return;
        }

        GameProfileStore.ClearActiveProfile();
        Debug.Log($"Soulstone profile '{GameProfileStore.ActiveProfileId}' cleared.");
    }

    [MenuItem(RootMenu + "Print Active Profile")]
    public static void PrintActiveProfile()
    {
        Debug.Log($"Active Soulstone profile: {GameProfileStore.ActiveProfileId}");
    }

    private static void SetActiveProfile(int profileIndex)
    {
        GameProfileStore.ActiveProfileIndex = profileIndex;
        Debug.Log($"Active Soulstone profile changed to {GameProfileStore.ActiveProfileId}.");
    }
}

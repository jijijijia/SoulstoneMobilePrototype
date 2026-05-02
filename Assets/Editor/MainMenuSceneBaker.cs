using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class MainMenuSceneBaker
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BakedVersionKey = "Soulstone.MainMenuSceneBaker.BakedVersion";
    private const int BakedVersion = 6;

    static MainMenuSceneBaker()
    {
        EditorApplication.delayCall += BakeOpenMainMenuIfNeeded;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Soulstone/Bake Main Menu Scene")]
    public static void BakeOpenMainMenu()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.path != MainMenuScenePath)
        {
            EditorSceneManager.OpenScene(MainMenuScenePath);
        }

        MainMenuManager manager = Object.FindFirstObjectByType<MainMenuManager>();

        if (manager == null)
        {
            Debug.LogWarning("MainMenuSceneBaker could not find MainMenuManager in MainMenu scene.");
            return;
        }

        manager.RebuildSceneMenuLayout();
        manager.RefreshSelectedHeroPreviewInScene();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorPrefs.SetInt(BakedVersionKey, BakedVersion);
        Debug.Log("MainMenu scene baked: UI canvas and 3D room are now saved in Hierarchy.");
    }

    private static void BakeOpenMainMenuIfNeeded()
    {
        if (Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.path != MainMenuScenePath)
        {
            return;
        }

        bool alreadyBaked = EditorPrefs.GetInt(BakedVersionKey, 0) >= BakedVersion
            && GameObject.Find("MainMenu_Backdrop") != null
            && GameObject.Find("MainMenu_3DRoom") != null;

        if (alreadyBaked)
        {
            return;
        }

        BakeOpenMainMenu();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += BakeOpenMainMenuIfNeeded;
        }
    }
}

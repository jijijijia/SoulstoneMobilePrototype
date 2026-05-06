using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuSceneBaker
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BakedVersionKey = "Soulstone.MainMenuSceneBaker.BakedVersion";
    private const int BakedVersion = 8;

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
        MainMenuFantasySceneBaker.BakeOpenMainMenu();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorPrefs.SetInt(BakedVersionKey, BakedVersion);
        Debug.Log("MainMenu scene baked: UI canvas and 3D room are now saved in Hierarchy.");
    }

}

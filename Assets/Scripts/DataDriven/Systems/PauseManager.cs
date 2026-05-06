using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PauseManager : MonoBehaviour
{
    [SerializeField] private DDRunManager runManager;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (runManager == null)
        {
            runManager = FindFirstObjectByType<DDRunManager>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (runManager == null)
        {
            return;
        }

        if (runManager.CurrentState == DDRunState.Playing)
        {
            runManager.PauseRun();
        }
        else if (runManager.CurrentState == DDRunState.Paused)
        {
            runManager.UnpauseRun();
        }
    }

    public void Resume()
    {
        if (runManager != null)
        {
            runManager.UnpauseRun();
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

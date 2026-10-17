using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Main";
    public string mainMenuSceneName = "MainMenu";
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneTransition fader = FindObjectOfType<SceneTransition>();
        if (fader != null)
        {
            fader.LoadSceneWithFade(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void LoadMainMenu()
    {
        SceneTransition fader = FindObjectOfType<SceneTransition>();
        if (fader != null)
        {
            fader.LoadSceneWithFade(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
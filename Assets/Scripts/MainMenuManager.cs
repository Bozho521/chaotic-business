using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject quitButton;
    public string gameSceneName = "Main";

    void Start()
    {
#if UNITY_WEBGL
        if (quitButton != null)
        {
            quitButton.SetActive(false);
        }
#endif
    }

    public void PlayGame()
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

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Main";
    public string mainMenuSceneName = "MainMenu";
    
    [Header("Idle Timeout")]
    public float idleTimeout = 30f;
    
    private float timer = 0f;
    private bool isTransitioning = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (isTransitioning) return;

        timer += Time.deltaTime;

        if (timer >= idleTimeout)
        {
            LoadMainMenu();
        }
    }

    public void RestartGame()
    {
        if (isTransitioning) return;
        isTransitioning = true;

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
        if (isTransitioning) return;
        isTransitioning = true;

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
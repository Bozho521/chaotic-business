using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditSceneManager : MonoBehaviour
{
    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenu";
    public float autoMainMenuTimer = 30f;

    [Header("Credit Sprites")]
    public SpriteRenderer[] creditSprites;
    
    public float revealInterval = 5f;
    public float fadeDelay = 10f;
    public float fadeDuration = 1.5f;

    [Header("Skip")]
    public Image skipPromptImage;

    private float timer = 0f;
    private bool isTransitioning = false;
    private int clickCount = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (int i = 0; i < creditSprites.Length; i++)
        {
            if (creditSprites[i] != null)
            {
                Color c = creditSprites[i].color;
                c.a = (i == 0) ? 1f : 0f;
                creditSprites[i].color = c;

                StartCoroutine(SpriteLifecycle(i));
            }
        }

        if (skipPromptImage != null)
        {
            Color c = skipPromptImage.color;
            c.a = 0f;
            skipPromptImage.color = c;
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        timer += Time.deltaTime;
        if (timer >= autoMainMenuTimer)
        {
            LoadMainMenu();
        }

        if (WasAnyKeyPressedThisFrame())
        {
            clickCount++;

            if (clickCount == 1)
            {
                if (skipPromptImage != null)
                {
                    StartCoroutine(FadeImage(skipPromptImage, 0f, 1f));
                }
            }
            else if (clickCount >= 2)
            {
                LoadMainMenu();
            }
        }
    }

    private bool WasAnyKeyPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) return true;
        return false;
    }

    private IEnumerator SpriteLifecycle(int index)
    {
        if (index > 0)
        {
            yield return new WaitForSeconds(index * revealInterval);
            yield return StartCoroutine(FadeSprite(creditSprites[index], 0f, 1f));
        }

        yield return new WaitForSeconds(fadeDelay);

        if (creditSprites[index] != null)
        {
            yield return StartCoroutine(FadeSprite(creditSprites[index], 1f, 0f));
        }
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color c = sr.color;

        while (elapsed < fadeDuration)
        {
            if (sr == null) break;
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            sr.color = c;
            yield return null;
        }

        if (sr != null)
        {
            c.a = endAlpha;
            sr.color = c;
        }
    }

    private IEnumerator FadeImage(Image img, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < fadeDuration)
        {
            if (img == null) break;
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            img.color = c;
            yield return null;
        }

        if (img != null)
        {
            c.a = endAlpha;
            img.color = c;
        }
    }

    private void LoadMainMenu()
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
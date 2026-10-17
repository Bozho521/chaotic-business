using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;
    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 1f);
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }
    public void LoadSceneWithFade(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeOut(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadeImage.gameObject.SetActive(false); 
    }

    private IEnumerator FadeOut(string sceneName)
    {
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        SceneManager.LoadScene(sceneName);
    }
}
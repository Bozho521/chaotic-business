using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Content")]
    [TextArea(2, 5)]
    public string[] messages;
    
    public float delayBetweenLines = 0.5f;

    [Header("Intro Settings")]
    public bool isIntroScene = false;
    public string nextSceneName = "MainMenu";

    private void Start()
    {
        if (isIntroScene)
        {
            StartCoroutine(PlaySequence());
        }
    }
    
    public void TriggerDialogue()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        if (isIntroScene) yield return new WaitForSeconds(1f);

        foreach (string message in messages)
        {
            DialogueManager.Instance.ShowMessage(message);

            yield return null;

            while (DialogueManager.Instance.IsPlaying)
            {
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenLines);
        }

        if (isIntroScene)
        {
            SceneTransition fader = FindObjectOfType<SceneTransition>();
            if (fader != null)
            {
                fader.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
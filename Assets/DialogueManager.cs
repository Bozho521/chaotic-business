using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool IsPlaying { get; private set; }

    [Header("UI Elements")]
    public CanvasGroup dialogueBoxGroup; 
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.04f; 
    
    [Header("Display Settings")]
    public float timePerCharacter = 0.05f;
    public float minDisplayTime = 1.5f;
    public float fadeSpeed = 3f; 

    private Coroutine activeDialogueRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (dialogueBoxGroup != null)
        {
            dialogueBoxGroup.alpha = 0f;
            dialogueBoxGroup.interactable = false;
            dialogueBoxGroup.blocksRaycasts = false;
        }
    }

    public void ShowMessage(string message)
    {
        if (activeDialogueRoutine != null)
        {
            StopCoroutine(activeDialogueRoutine);
        }
        
        activeDialogueRoutine = StartCoroutine(DialogueSequence(message));
    }

    private IEnumerator DialogueSequence(string message)
    {
        IsPlaying = true;

        dialogueText.text = "";
        while (dialogueBoxGroup.alpha < 1f)
        {
            dialogueBoxGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        dialogueBoxGroup.alpha = 1f;

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        float waitTime = Mathf.Max(minDisplayTime, message.Length * timePerCharacter);
        yield return new WaitForSeconds(waitTime);

        while (dialogueBoxGroup.alpha > 0f)
        {
            dialogueBoxGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        dialogueBoxGroup.alpha = 0f;

        IsPlaying = false;
    }
}
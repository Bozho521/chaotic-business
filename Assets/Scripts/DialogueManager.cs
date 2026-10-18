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
    
    private string queuedMessage = "";
    private bool hasQueuedMessage = false;

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
        if (!IsPlaying)
        {
            activeDialogueRoutine = StartCoroutine(DialogueSequence(message));
        }
        else if (!hasQueuedMessage)
        {
            queuedMessage = message;
            hasQueuedMessage = true;
        }
    }

    private IEnumerator DialogueSequence(string message)
    {
        IsPlaying = true;

        dialogueText.text = message;
        dialogueText.maxVisibleCharacters = 0;

        while (dialogueBoxGroup.alpha < 1f)
        {
            dialogueBoxGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        dialogueBoxGroup.alpha = 1f;

        dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        float waitTime = Mathf.Max(minDisplayTime, totalVisibleCharacters * timePerCharacter);
        yield return new WaitForSeconds(waitTime);

        while (dialogueBoxGroup.alpha > 0f)
        {
            dialogueBoxGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        dialogueBoxGroup.alpha = 0f;

        if (hasQueuedMessage)
        {
            string nextMessage = queuedMessage;
            hasQueuedMessage = false;
            queuedMessage = "";
            
            yield return new WaitForSeconds(0.2f);
            activeDialogueRoutine = StartCoroutine(DialogueSequence(nextMessage));
        }
        else
        {
            IsPlaying = false;
            activeDialogueRoutine = null;
        }
    }
}
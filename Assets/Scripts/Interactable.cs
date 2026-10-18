using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [TextArea(2, 4)]
    public string[] interactMessages;

    [Header("Settings")]
    public float interactionCooldown = 1.5f;

    private bool isOnCooldown = false;
    private int lastMessageIndex = -1;

    public void Interact()
    {
        if (isOnCooldown || interactMessages == null || interactMessages.Length == 0) return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
        {
            return;
        }

        int randomIndex = 0;

        if (interactMessages.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, interactMessages.Length);
            } 
            while (randomIndex == lastMessageIndex);
        }

        lastMessageIndex = randomIndex;
        string chosenMessage = interactMessages[randomIndex];

        if (!string.IsNullOrEmpty(chosenMessage))
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowMessage(chosenMessage);
            }
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        isOnCooldown = false;
    }
}
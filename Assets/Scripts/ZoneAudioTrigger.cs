using UnityEngine;

[System.Serializable]
public class AudioDialogueEvent
{
    public AudioClip clip;
    
    [TextArea(2, 4)]
    public string[] possibleMessages;
}

[RequireComponent(typeof(AudioSource))]
public class ZoneAudioTrigger : MonoBehaviour
{
    [Header("Audio & Dialogue")]
    public AudioDialogueEvent[] audioEvents;
    
    [Header("Cooldown")]
    public float minCooldown = 5f;
    public float maxCooldown = 15f;
    
    public bool playImmediatelyOnEnter = true;

    private AudioSource audioSource;
    private bool isPlayerInZone = false;
    private float cooldownTimer = 0f;
    private int lastPlayedIndex = -1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (isPlayerInZone && !audioSource.isPlaying)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                PlayRandomSoundAndDialogue();
            }
        }
    }

    private void PlayRandomSoundAndDialogue()
    {
        if (audioEvents == null || audioEvents.Length == 0) return;

        int randomIndex = 0;
        
        if (audioEvents.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, audioEvents.Length);
            } 
            while (randomIndex == lastPlayedIndex);
        }

        lastPlayedIndex = randomIndex;
        AudioDialogueEvent selectedEvent = audioEvents[randomIndex];

        if (selectedEvent.clip != null)
        {
            audioSource.clip = selectedEvent.clip;
            audioSource.Play();
        }

        if (selectedEvent.possibleMessages != null && selectedEvent.possibleMessages.Length > 0)
        {
            int randomMsgIndex = Random.Range(0, selectedEvent.possibleMessages.Length);
            string chosenMessage = selectedEvent.possibleMessages[randomMsgIndex];

            if (!string.IsNullOrEmpty(chosenMessage) && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowMessage(chosenMessage);
            }
        }

        cooldownTimer = Random.Range(minCooldown, maxCooldown);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            
            if (playImmediatelyOnEnter && !audioSource.isPlaying)
            {
                cooldownTimer = 0f; 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
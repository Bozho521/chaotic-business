using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ValveMinigame : MonoBehaviour
{
    public static float CurrentPressureRatio { get; private set; } = 0f;

    [Header("Game Settings")]
    public MiniGameType myGameType = MiniGameType.ValveMinigame;
    
    [Header("Dialogue - First Time")]
    [TextArea(2, 3)] public string entryMessage;
    [TextArea(2, 3)] public string warning30s;
    [TextArea(2, 3)] public string warning60s;
    [TextArea(2, 3)] public string warning80s;
    [TextArea(2, 3)] public string warning110s;

    [Header("Dialogue - Repeats")]
    [TextArea(2, 3)] public string repeatWarning30s;
    [TextArea(2, 3)] public string repeatWarning60s;
    [TextArea(2, 3)] public string repeatWarning80s;
    [TextArea(2, 3)] public string repeatWarning110s;

    [Header("Pressure Mechanics")]
    public float timeToExplode = 120f; 
    public float restartDelay = 60f;
    public float autoSabotageInterval = 8f; 

    [Header("Sprites & Models")]
    public Transform[] valveModels = new Transform[3]; 
    public Vector3 rotationAxis = new Vector3(0, 0, 1); 
    public float rotationStep = 36f; 
    public float rotationSpeed = 150f; 
    public float holdRepeatRate = 0.15f; 

    [Header("Number")]
    public MeshRenderer[] digitDisplays = new MeshRenderer[3];
    public Material[] digitMaterials = new Material[10];

    [Header("Meter")]
    public Transform arrowPivot;
    public Vector3 arrowRotationAxis = new Vector3(0, 0, 1);
    public float safeAngle = 45f;  
    public float dangerAngle = -45f;
    public float arrowSpeed = 5f;

    [Header("Game Over Logic")]
    public string gameOverSceneName = "GameOver";

    private bool isSystemActive = false; 
    private int completionCount = 0;
    private float pressureTimer = 0f;
    private float sabotageTimer = 0f;
    private int lastLoggedSecond = -1;

    private bool w30, w60, w80, w110;

    private int[] targetSteps = new int[3]; 
    private float[] currentVisualAngles = new float[3];
    private Quaternion[] initialRotations = new Quaternion[3];
    private Quaternion initialArrowRotation; 
    private float[] inputTimers = new float[3];
    private int[] currentDigits = new int[3];
    private int[] targetDigits = new int[3];
    
    private bool isFailed = false;
    private float targetArrowAngle;
    
    private bool wasPlaying = false;
    private bool hasPlayedEntryMessage = false;

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            if (valveModels.Length > i && valveModels[i] != null)
            {
                initialRotations[i] = valveModels[i].localRotation;
            }
        }
        
        if (arrowPivot != null)
        {
            initialArrowRotation = arrowPivot.localRotation;
        }

        StartCrisis();
    }

    void Update()
    {
        if (isFailed) return;

        HandleActiveSystemLogics();

        bool isCurrentlyPlaying = MainPlayerController.IsPlayingMiniGame && MainPlayerController.ActiveGameType == myGameType;

        if (isCurrentlyPlaying && !wasPlaying)
        {
            wasPlaying = true;
            if (!hasPlayedEntryMessage && completionCount == 0 && !string.IsNullOrEmpty(entryMessage))
            {
                if (DialogueManager.Instance != null) DialogueManager.Instance.ShowMessage(entryMessage);
                hasPlayedEntryMessage = true;
            }
        }
        else if (!isCurrentlyPlaying && wasPlaying)
        {
            wasPlaying = false;
            hasPlayedEntryMessage = false;
        }

        if (isCurrentlyPlaying && isSystemActive)
        {
            HandleValveInputs();
        }

        UpdateVisuals();

        if (arrowPivot != null)
        {
            Quaternion targetRot = Quaternion.AngleAxis(targetArrowAngle, arrowRotationAxis.normalized);
            arrowPivot.localRotation = Quaternion.Slerp(arrowPivot.localRotation, initialArrowRotation * targetRot, Time.deltaTime * arrowSpeed);
        }
    }

    private void HandleActiveSystemLogics()
    {
        if (!isSystemActive) return;

        pressureTimer += Time.deltaTime;
        sabotageTimer += Time.deltaTime;

        CurrentPressureRatio = Mathf.Clamp01(pressureTimer / timeToExplode);

        int currentSec = Mathf.FloorToInt(pressureTimer);
        if (currentSec != lastLoggedSecond && currentSec > 0 && currentSec % 10 == 0)
        {
            float timeRemaining = timeToExplode - currentSec;

            Debug.Log($"<color=red>[PRESSURE SYSTEM] Time Passed: {currentSec}s | Eruption in: {timeRemaining}s</color>");
            lastLoggedSecond = currentSec;
        }

        if (sabotageTimer >= autoSabotageInterval)
        {
            sabotageTimer = 0f;
            TriggerSabotage();
        }

        CheckWarnings();

        if (pressureTimer >= timeToExplode)
        {
            TriggerFailState();
        }
    }

    private void CheckWarnings()
    {
        if (pressureTimer >= 30f && !w30) { TriggerWarning(warning30s, repeatWarning30s); w30 = true; }
        if (pressureTimer >= 60f && !w60) { TriggerWarning(warning60s, repeatWarning60s); w60 = true; }
        if (pressureTimer >= 80f && !w80) { TriggerWarning(warning80s, repeatWarning80s); w80 = true; }
        if (pressureTimer >= 110f && !w110) { TriggerWarning(warning110s, repeatWarning110s); w110 = true; }
    }

    private void TriggerWarning(string firstTimeMsg, string repeatMsg)
    {
        if (DialogueManager.Instance == null) return;
        
        string msg = (completionCount == 0) ? firstTimeMsg : repeatMsg;
        if (!string.IsNullOrEmpty(msg))
        {
            string coloredMsg = "<color=#FFA500>" + msg + "</color>";
            DialogueManager.Instance.ShowMessage(coloredMsg);
        }
    }

    private void TriggerSabotage()
    {
        int randomValve = Random.Range(0, 3);
        int direction = Random.value > 0.5f ? 1 : -1;
        targetSteps[randomValve] += direction;
    }

    private void StartCrisis()
    {
        isSystemActive = true;
        pressureTimer = 0f;
        sabotageTimer = 0f;
        CurrentPressureRatio = 0f;
        lastLoggedSecond = -1;
        
        w30 = false; w60 = false; w80 = false; w110 = false;

        for (int i = 0; i < 3; i++)
        {
            targetDigits[i] = Random.Range(0, 10);
            targetSteps[i] = Random.Range(1, 5); 
            UpdateDisplayMaterial(i); 
        }
        
        UpdateMeterTarget();
    }

    private void HandleValveInputs()
    {
        if (Keyboard.current == null) return;

        ProcessInput(0, Keyboard.current.aKey, Keyboard.current.dKey);
        ProcessValve1Input();
        ProcessInput(2, Keyboard.current.sKey, Keyboard.current.wKey);
    }

    private void ProcessInput(int index, UnityEngine.InputSystem.Controls.KeyControl negKey, UnityEngine.InputSystem.Controls.KeyControl posKey)
    {
        if (negKey.wasPressedThisFrame) RegisterStep(index, -1);
        else if (posKey.wasPressedThisFrame) RegisterStep(index, 1);
        else if (negKey.isPressed) HandleHold(index, -1);
        else if (posKey.isPressed) HandleHold(index, 1);
        else inputTimers[index] = 0f; 
    }

    private void ProcessValve1Input()
    {
        var k = Keyboard.current;
        bool negDown = k.commaKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame;
        bool posDown = k.periodKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame;
        bool negHeld = k.commaKey.isPressed || k.leftArrowKey.isPressed;
        bool posHeld = k.periodKey.isPressed || k.rightArrowKey.isPressed;

        if (negDown) RegisterStep(1, -1);
        else if (posDown) RegisterStep(1, 1);
        else if (negHeld) HandleHold(1, -1);
        else if (posHeld) HandleHold(1, 1);
        else inputTimers[1] = 0f;
    }

    private void RegisterStep(int index, int direction)
    {
        targetSteps[index] += direction;
        inputTimers[index] = holdRepeatRate + 0.2f; 
    }

    private void HandleHold(int index, int direction)
    {
        inputTimers[index] -= Time.deltaTime;
        if (inputTimers[index] <= 0f)
        {
            targetSteps[index] += direction;
            inputTimers[index] = holdRepeatRate; 
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            float targetAngle = targetSteps[i] * rotationStep;
            
            if (currentVisualAngles[i] != targetAngle)
            {
                currentVisualAngles[i] = Mathf.MoveTowards(currentVisualAngles[i], targetAngle, rotationSpeed * Time.deltaTime);
                
                if (valveModels != null && valveModels.Length > i && valveModels[i] != null)
                {
                    Quaternion offsetRot = Quaternion.AngleAxis(currentVisualAngles[i], rotationAxis.normalized);
                    valveModels[i].localRotation = initialRotations[i] * offsetRot;
                }

                int currentDigit = Mathf.RoundToInt(currentVisualAngles[i] / rotationStep);
                currentDigit = currentDigit % 10;
                if (currentDigit < 0) currentDigit += 10; 

                if (currentDigits[i] != currentDigit)
                {
                    currentDigits[i] = currentDigit;
                    UpdateDisplayMaterial(i); 
                    
                    if (isSystemActive)
                    {
                        UpdateMeterTarget(); 
                        CheckWinCondition();
                    }
                }
            }
        }
    }

    private void UpdateDisplayMaterial(int index)
    {
        if (digitDisplays != null && digitDisplays.Length > index && digitDisplays[index] != null)
        {
            if (digitMaterials != null && digitMaterials.Length == 10)
            {
                int currentNumber = currentDigits[index];
                if (digitMaterials[currentNumber] != null)
                {
                    digitDisplays[index].material = digitMaterials[currentNumber];
                }
            }
        }
    }

    private void UpdateMeterTarget()
    {
        int totalError = 0;
        
        for (int i = 0; i < 3; i++)
        {
            int difference = Mathf.Abs(currentDigits[i] - targetDigits[i]);
            int shortestPath = Mathf.Min(difference, 10 - difference);


            totalError += shortestPath;
        }

        float errorPercentage = totalError / 15f;
        targetArrowAngle = Mathf.Lerp(safeAngle, dangerAngle, errorPercentage);

        if (totalError >= 15 && isSystemActive)
        {
            TriggerFailState();
        }
    }

    private void CheckWinCondition()
    {
        if (currentDigits[0] == targetDigits[0] &&
            currentDigits[1] == targetDigits[1] &&
            currentDigits[2] == targetDigits[2])
        {
            Debug.Log("<color=green>PRESSURE STABILIZED</color>");
            isSystemActive = false;
            CurrentPressureRatio = 0f;
            completionCount++;
            StartCoroutine(WaitAndRestartRoutine());
        }
    }

    private IEnumerator WaitAndRestartRoutine()
    {
        targetArrowAngle = safeAngle;
        
        yield return new WaitForSeconds(restartDelay);
        
        Debug.Log("<color=yellow>NEW CRISIS STARTED</color>");
        StartCrisis();
    }

    private void TriggerFailState()
    {
        isFailed = true;
        isSystemActive = false;
        CurrentPressureRatio = 1f;
        
        Debug.Log("<color=red>CATASTROPHIC PRESSURE FAILURE</color>");
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowMessage("HULL RUPTURE DETECTED. PRESSURE CRITICAL...");
        }

        SceneTransition fader = FindObjectOfType<SceneTransition>();
        if (fader != null)
        {
            fader.LoadSceneWithFade(gameOverSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
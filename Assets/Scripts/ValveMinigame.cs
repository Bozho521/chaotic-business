using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ValveMinigame : MonoBehaviour
{
    [Header("Game Settings")]
    public MiniGameType myGameType = MiniGameType.ValveMinigame;
    
    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string entryMessage;

    [Header("Sprites")]
    public Transform[] valveModels = new Transform[3]; 
    public Vector3 rotationAxis = new Vector3(0, 0, 1); 
    public float rotationStep = 36f; 

    [Header("Rotation Settings")]
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

    private int[] targetSteps = new int[3]; 
    private float[] currentVisualAngles = new float[3];
    
    private Quaternion[] initialRotations = new Quaternion[3];
    private Quaternion initialArrowRotation; 
    
    private float[] inputTimers = new float[3];

    private int[] currentDigits = new int[3];
    private int[] targetDigits = new int[3];
    
    private bool isCompleted = false;
    private bool isFailed = false;
    
    private float targetArrowAngle;
    
    private bool wasPlaying = false;
    private bool hasPlayedMessage = false;

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

        GenerateNewTask();
    }

    void Update()
    {
        bool isCurrentlyPlaying = MainPlayerController.IsPlayingMiniGame && MainPlayerController.ActiveGameType == myGameType;

        if (isCurrentlyPlaying && !wasPlaying)
        {
            wasPlaying = true;
            
            if (!hasPlayedMessage && !string.IsNullOrEmpty(entryMessage))
            {
                if (DialogueManager.Instance != null) DialogueManager.Instance.ShowMessage(entryMessage);
                hasPlayedMessage = true;
            }
        }
        else if (!isCurrentlyPlaying && wasPlaying)
        {
            wasPlaying = false;
            hasPlayedMessage = false;
        }

        if (isCurrentlyPlaying)
        {
            if (!isCompleted && !isFailed) 
            {
                HandleValveInputs();
                UpdateVisuals();
            }
        }

        if (arrowPivot != null)
        {
            Quaternion targetRot = Quaternion.AngleAxis(targetArrowAngle, arrowRotationAxis.normalized);
            arrowPivot.localRotation = Quaternion.Slerp(arrowPivot.localRotation, initialArrowRotation * targetRot, Time.deltaTime * arrowSpeed);
        }
    }

    private void GenerateNewTask()
    {
        isCompleted = false;
        isFailed = false;
        
        for (int i = 0; i < 3; i++)
        {
            targetDigits[i] = Random.Range(0, 10);
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
                    UpdateMeterTarget(); 
                    CheckWinCondition();
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

        Debug.Log($"Current -> Target:  [ {currentDigits[0]} | {currentDigits[1]} | {currentDigits[2]} ]  --->  [ {targetDigits[0]} | {targetDigits[1]} | {targetDigits[2]} ]   (Total Error: {totalError})");

        if (totalError >= 15 && !isFailed && !isCompleted)
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
            isCompleted = true;
            Debug.Log("<color=green>SUCCESS</color>");
        }
    }

    private void TriggerFailState()
    {
        isFailed = true;
        Debug.Log("<color=red>FAIL</color>");
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
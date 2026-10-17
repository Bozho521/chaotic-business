using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TimerMinigame : MonoBehaviour
{
    [Header("Game Settings")]
    public MiniGameType myGameType = MiniGameType.TimerGame;
    
    [Header("Timer Settings")]
    public float maxTime = 60f;
    public float timeDrainRate = 1f;
    public float timeFillRate = 5f;

    [Header("UI Displays")]
    public TextMeshProUGUI timerTextDisplay;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public float warningThreshold = 15f;

    [Header("Button Visuals")]
    public GameObject pushedButtonGraphic;
    public GameObject releasedButtonGraphic;

    [Header("Oxygen Tank Mechanics")]
    public Transform oxygenTank;
    public GameObject normalTankGraphic;
    public GameObject brokenTankGraphic;
    
    public Transform o2IndicatorLine;
    public Vector3 scaleEmpty = new Vector3(1.5f, 0f, 1f);
    public Vector3 scaleFull = new Vector3(1.5f, 5f, 1f);
    
    [Header("Overcharge")]
    public float shakeThreshold = 3f;    
    public float explodeThreshold = 5f;  
    public float shakeIntensity = 0.05f;

    private float currentTime;
    private bool isGameOver = false;
    private bool isExploded = false;
    
    private int lastDecade; 
    private bool wasPlaying = false;
    
    private Vector3 initialTankPos;
    private int lastFillLogSecond;

    void Start()
    {
        currentTime = maxTime;
        lastDecade = Mathf.FloorToInt(currentTime / 10f);
        lastFillLogSecond = Mathf.FloorToInt(currentTime);
        
        if (oxygenTank != null) initialTankPos = oxygenTank.localPosition;
        
        if (normalTankGraphic != null) normalTankGraphic.SetActive(true);
        if (brokenTankGraphic != null) brokenTankGraphic.SetActive(false);
        
        UpdateButtonVisual(false);
    }

    void Update()
    {
        if (isGameOver || isExploded) return;

        currentTime -= timeDrainRate * Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            TriggerGameOver();
        }

        bool isCurrentlyPlaying = MainPlayerController.IsPlayingMiniGame && MainPlayerController.ActiveGameType == myGameType;

        if (isCurrentlyPlaying && !wasPlaying)
        {
            Debug.Log("<color=cyan>Hold Space to charge.</color>");
            wasPlaying = true;
        }
        else if (!isCurrentlyPlaying && wasPlaying)
        {
            wasPlaying = false;
            UpdateButtonVisual(false); 
        }

        if (isCurrentlyPlaying)
        {
            HandleInput();
        }
        else
        {
            lastFillLogSecond = Mathf.FloorToInt(currentTime);
        }

        UpdateVisuals();
        CheckTenSecondInterval();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        bool isPressingSpace = Keyboard.current.spaceKey.isPressed;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("<color=green>Timer is going up.</color>");
        }

        if (isPressingSpace)
        {
            currentTime += timeFillRate * Time.deltaTime;
            
            if (currentTime > maxTime + explodeThreshold)
            {
                currentTime = maxTime + explodeThreshold;
            }

            int currentSecond = Mathf.FloorToInt(currentTime);
            if (currentSecond > lastFillLogSecond)
            {
                Debug.Log($"<color=cyan>Current Level: {currentSecond}</color>");
                lastFillLogSecond = currentSecond;
            }
        }
        else
        {
            lastFillLogSecond = Mathf.FloorToInt(currentTime);
        }

        UpdateButtonVisual(isPressingSpace);
    }

    private void UpdateButtonVisual(bool isPressed)
    {
        if (pushedButtonGraphic != null) pushedButtonGraphic.SetActive(isPressed);
        if (releasedButtonGraphic != null) releasedButtonGraphic.SetActive(!isPressed);
    }

    private void CheckTenSecondInterval()
    {
        if (currentTime > maxTime) return;

        int currentDecade = Mathf.FloorToInt(currentTime / 10f);

        if (currentDecade < lastDecade)
        {
            int passedMark = (currentDecade * 10) + 10;
            
            if (passedMark < maxTime && passedMark > 0)
            {
                Debug.Log($"<color=yellow>Timer: {passedMark} seconds remaining!</color>");
            }
            
            lastDecade = currentDecade;
        }
        else if (currentDecade > lastDecade)
        {
            lastDecade = currentDecade;
        }
    }

    private void UpdateVisuals()
    {
        if (timerTextDisplay != null)
        {
            float displayTime = Mathf.Min(currentTime, maxTime);
            timerTextDisplay.text = displayTime.ToString("00.00");

            if (currentTime <= warningThreshold)
            {
                timerTextDisplay.color = Color.Lerp(warningColor, normalColor, Mathf.PingPong(Time.time * 2f, 1f));
            }
            else
            {
                timerTextDisplay.color = normalColor;
            }
        }

        if (o2IndicatorLine != null)
        {
            float fillPercentage = Mathf.Clamp01(currentTime / maxTime);
            o2IndicatorLine.localScale = Vector3.Lerp(scaleEmpty, scaleFull, fillPercentage);
        }

        if (oxygenTank != null && !isExploded)
        {
            float overcharge = currentTime - maxTime;
            if (overcharge >= shakeThreshold && overcharge < explodeThreshold)
            {
                float currentShake = shakeIntensity * (overcharge - shakeThreshold); 
                oxygenTank.localPosition = initialTankPos + Random.insideUnitSphere * currentShake;
            }
            else
            {
                oxygenTank.localPosition = initialTankPos;
            }
        }

        if (currentTime >= maxTime + explodeThreshold && !isExploded)
        {
            TriggerExplosion();
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        
        if (timerTextDisplay != null)
        {
            timerTextDisplay.color = Color.red;
            timerTextDisplay.text = "00.00";
        }
        
        UpdateButtonVisual(false);
        Debug.Log("<color=red>ERROR. Time expired</color>");
    }

    private void TriggerExplosion()
    {
        isExploded = true;
        UpdateButtonVisual(false);
        
        if (timerTextDisplay != null)
        {
            timerTextDisplay.color = Color.red;
            timerTextDisplay.text = "ERR";
        }
        
        if (normalTankGraphic != null) normalTankGraphic.SetActive(false);
        if (brokenTankGraphic != null) brokenTankGraphic.SetActive(true);
        
        if (o2IndicatorLine != null) o2IndicatorLine.gameObject.SetActive(false);
        
        Debug.Log("<color=red>EXPLOSION</color>");
    }
}
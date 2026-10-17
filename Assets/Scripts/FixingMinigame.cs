using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FixingMinigame : MonoBehaviour
{
    [Header("Game Settings")]
    public MiniGameType myGameType = MiniGameType.FixingGame;
    
    [Header("Visuals")]
    public SpriteRenderer[] holeLocations; 
    public SpriteRenderer playerShapeDisplay;
    public Color holeTint = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Shape Sprites ")]
    public Sprite[] triangleSprites;
    public Sprite[] squareSprites;
    public Sprite[] sphereSprites;

    [Header("Mechanics")]
    public float timeToRepair = 3f; 
    public Transform progressBar; 
    
    private int[] holesPerStage = { 2, 4, 6 };
    private int currentStageIndex = 0;
    
    private int currentShapeIndex = 0; 
    private int currentRotIndex = 0;

    private float repairProgress = 0f;      //questionable
    private bool isPlaying = false;
    private bool isCompleted = false;

    private class HoleData 
    {
        public SpriteRenderer renderer;
        public int shapeGroup;
        public bool isFixed;
    }
    
    private List<HoleData> activeHoles = new List<HoleData>();
    private HoleData holeBeingRepaired = null;

    void Update()
    {
        if (MainPlayerController.IsPlayingMiniGame && MainPlayerController.ActiveGameType == myGameType)
        {
            if (!isPlaying) StartMinigame();
            else if (!isCompleted) 
            {
                HandleInputs();
                UpdateRepairProgress();
            }
        }
        else
        {
            isPlaying = false;
            repairProgress = 0f; 
            ResetHoleColors();
            UpdateProgressBar();
        }
    }

    private void StartMinigame()
    {
        isPlaying = true;
        isCompleted = false;
        currentStageIndex = 0;
        
        if (playerShapeDisplay != null) playerShapeDisplay.gameObject.SetActive(true);

        GenerateStage();
    }

    private void GenerateStage()
    {
        repairProgress = 0f;
        UpdateProgressBar();
        activeHoles.Clear();
        holeBeingRepaired = null;

        foreach (var loc in holeLocations)
        {
            if (loc != null) loc.gameObject.SetActive(false);
        }

        int numHoles = holesPerStage[currentStageIndex];

        List<Vector2Int> availableShapes = new List<Vector2Int>();
        for (int s = 0; s < 3; s++)
        {
            int maxRots = GetMaxRotations(s);
            for (int r = 0; r < maxRots; r++) availableShapes.Add(new Vector2Int(s, r));
        }

        List<int> availableLocations = new List<int>();
        for (int i = 0; i < holeLocations.Length; i++) availableLocations.Add(i);

        ShuffleList(availableShapes);
        ShuffleList(availableLocations);

        for (int i = 0; i < numHoles; i++)
        {
            int locIndex = availableLocations[i];
            Vector2Int shapeCombo = availableShapes[i];

            SpriteRenderer targetRenderer = holeLocations[locIndex];
            targetRenderer.gameObject.SetActive(true);
            targetRenderer.sprite = GetShapeSprite(shapeCombo.x, shapeCombo.y);
            targetRenderer.color = holeTint;

            activeHoles.Add(new HoleData {
                renderer = targetRenderer,
                shapeGroup = shapeCombo.x,
                isFixed = false
            });
        }

        currentShapeIndex = 0;
        currentRotIndex = 0;
        UpdatePlayerVisual();
    }

    private void HandleInputs()
    {
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.spaceKey.isPressed) return; 

        if (Keyboard.current.aKey.wasPressedThisFrame) ChangeShape(-1);
        if (Keyboard.current.dKey.wasPressedThisFrame) ChangeShape(1);

        if (Keyboard.current.sKey.wasPressedThisFrame) ChangeRotation(-1);
        if (Keyboard.current.wKey.wasPressedThisFrame) ChangeRotation(1);
    }

    private void ChangeShape(int direction)
    {
        currentShapeIndex += direction;
        if (currentShapeIndex > 2) currentShapeIndex = 0;
        if (currentShapeIndex < 0) currentShapeIndex = 2;

        currentRotIndex = 0; 
        UpdatePlayerVisual();
    }

    private void ChangeRotation(int direction)
    {
        currentRotIndex += direction;
        int maxRots = GetMaxRotations(currentShapeIndex);

        if (currentRotIndex >= maxRots) currentRotIndex = 0;
        if (currentRotIndex < 0) currentRotIndex = maxRots - 1;

        UpdatePlayerVisual();
    }

    private void UpdatePlayerVisual()
    {
        if (playerShapeDisplay != null)
        {
            playerShapeDisplay.sprite = GetShapeSprite(currentShapeIndex, currentRotIndex);
        }
    }

    private void UpdateRepairProgress()
    {
        HoleData targetHole = GetMatchingUnfixedHole();

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (targetHole != null) Debug.Log("<color=yellow>MATCH FOUND!</color>");
            else Debug.Log("<color=orange>WRONG SHAPE!</color>");
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            if (targetHole != null)
            {
                holeBeingRepaired = targetHole;
                repairProgress += Time.deltaTime;
                
                float fill = repairProgress / timeToRepair;
                holeBeingRepaired.renderer.color = Color.Lerp(holeTint, Color.white, fill);

                if (repairProgress >= timeToRepair)
                {
                    repairProgress = 0f; 
                    FixHole(holeBeingRepaired);
                    holeBeingRepaired = null;
                }
            }
            else
            {
                repairProgress = 0f; 
                ResetHoleColors();
            }
        }
        else
        {
            repairProgress -= Time.deltaTime * 2f; 
            if (repairProgress < 0f) repairProgress = 0f;
            
            ResetHoleColors();
        }

        UpdateProgressBar();
    }

    private HoleData GetMatchingUnfixedHole()
    {
        foreach (var hole in activeHoles)
        {
            if (hole.isFixed) continue;

            bool exactSpriteMatch = (playerShapeDisplay.sprite != null && playerShapeDisplay.sprite == hole.renderer.sprite);
            
            bool sphereFreebie = (currentShapeIndex == 2 && hole.shapeGroup == 2); //questionable for spheres?

            if (exactSpriteMatch || sphereFreebie)
            {
                return hole;
            }
        }
        return null;
    }

    private void FixHole(HoleData hole)
    {
        hole.isFixed = true;
        hole.renderer.color = Color.white;
        Debug.Log("<color=green>Patched!</color>");

        CheckStageCompletion();
    }

    private void ResetHoleColors()
    {
        foreach (var hole in activeHoles)
        {
            if (!hole.isFixed) hole.renderer.color = holeTint;
        }
    }

    private void CheckStageCompletion()
    {
        bool allFixed = true;
        foreach (var hole in activeHoles)
        {
            if (!hole.isFixed) allFixed = false;
        }

        if (allFixed)
        {
            currentStageIndex++;
            if (currentStageIndex >= holesPerStage.Length)
            {
                CompleteMinigame();
            }
            else
            {
                GenerateStage();
            }
        }
    }

    private void CompleteMinigame()
    {
        isCompleted = true;
        if (playerShapeDisplay != null) playerShapeDisplay.gameObject.SetActive(false);
        Debug.Log("<color=green>ALL STAGES CLEARED!</color>");
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            float fillAmount = repairProgress / timeToRepair;
            progressBar.localScale = new Vector3(fillAmount, progressBar.localScale.y, progressBar.localScale.z);
        }
    }
    
    private int GetMaxRotations(int shapeIndex)
    {
        if (shapeIndex == 0 && triangleSprites != null) return triangleSprites.Length;
        if (shapeIndex == 1 && squareSprites != null) return squareSprites.Length;
        if (shapeIndex == 2 && sphereSprites != null) return sphereSprites.Length;
        return 1;
    }

    private Sprite GetShapeSprite(int shapeIndex, int rotIndex)
    {
        if (shapeIndex == 0 && triangleSprites != null && triangleSprites.Length > rotIndex) return triangleSprites[rotIndex];
        if (shapeIndex == 1 && squareSprites != null && squareSprites.Length > rotIndex) return squareSprites[rotIndex];
        if (shapeIndex == 2 && sphereSprites != null && sphereSprites.Length > rotIndex) return sphereSprites[rotIndex];
        return null;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
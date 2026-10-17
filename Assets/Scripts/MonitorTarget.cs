using UnityEngine;

public enum MiniGameType
{
    None,
    MazeGame,
    ValveMinigame,
    TimerGame,
    FixingGame
}

public class MonitorTarget : MonoBehaviour
{
    [Header("Monitor Settings")]
    public Transform zoomPosition;
    public MiniGameType gameType;

    [Header("Lever")]
    public Transform leverPivot;
    
    public Vector3 leverUpRotation = Vector3.zero;
    public Vector3 leverDownRotation = new Vector3(45f, 0f, 0f);
    
    public float leverAnimationSpeed = 5f;

    void Update()
    {
        if (leverPivot == null) return;

        bool isActive = MainPlayerController.IsPlayingMiniGame && MainPlayerController.ActiveGameType == gameType;

        Vector3 targetEuler = isActive ? leverDownRotation : leverUpRotation;
        Quaternion targetRotation = Quaternion.Euler(targetEuler);

        leverPivot.localRotation = Quaternion.Lerp(leverPivot.localRotation, targetRotation, Time.deltaTime * leverAnimationSpeed);
    }
}
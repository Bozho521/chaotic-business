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
    public Transform zoomPosition;
    public MiniGameType gameType;
}
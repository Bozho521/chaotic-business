using UnityEngine;

public class EngineController : MonoBehaviour
{
    [Header("Pistons")]
    public Transform[] pistons;
    public float minPistonSpeed = 5f;
    public float maxPistonSpeed = 20f;
    public float speedFluctuationRate = 2f;
    public float pistonTravelDistance = 1f;
    public Vector3 pistonMovementAxis = new Vector3(0, 1, 0);
    
    [Header("Pipes")]
    public Transform[] pipes;
    public float pipeVibrationSpeed = 10f;
    public float pipeVibrationIntensity = 0.2f;

    [Header("Rotary Objects")]
    public Transform[] rotors;
    public float minRotorSpeed = 20f;
    public float maxRotorSpeed = 100f;
    public float rotorSpeedFluctuation = 1f;
    public Vector3 rotorAxis = new Vector3(0, 0, 1);

    private Vector3[] initialPistonPositions;
    private float[] pistonPhases;
    private float[] pistonNoiseSeeds;
    
    private Vector3[] initialPipePositions;
    
    private float[] rotorNoiseSeeds;

    void Start()
    {
        initialPistonPositions = new Vector3[pistons.Length];
        pistonPhases = new float[pistons.Length];
        pistonNoiseSeeds = new float[pistons.Length];
        
        for (int i = 0; i < pistons.Length; i++)
        {
            initialPistonPositions[i] = pistons[i].localPosition;
            pistonPhases[i] = Random.Range(0f, Mathf.PI * 2f);
            pistonNoiseSeeds[i] = Random.Range(0f, 1000f);
        }

        initialPipePositions = new Vector3[pipes.Length];
        for (int i = 0; i < pipes.Length; i++)
        {
            initialPipePositions[i] = pipes[i].localPosition;
        }

        rotorNoiseSeeds = new float[rotors.Length];
        for (int i = 0; i < rotors.Length; i++)
        {
            rotorNoiseSeeds[i] = Random.Range(0f, 1000f);
        }
    }

    void Update()
    {
        float actualPistonTravel = pistonTravelDistance * 0.001f;
        float actualPipeIntensity = pipeVibrationIntensity * 0.001f;

        for (int i = 0; i < pistons.Length; i++)
        {
            float noise = Mathf.PerlinNoise(Time.time * speedFluctuationRate, pistonNoiseSeeds[i]);
            float currentSpeed = Mathf.Lerp(minPistonSpeed, maxPistonSpeed, noise);
            
            pistonPhases[i] += currentSpeed * Time.deltaTime;
            
            float offset = Mathf.Sin(pistonPhases[i]) * actualPistonTravel;
            Vector3 localDirection = pistons[i].localRotation * pistonMovementAxis.normalized;
            pistons[i].localPosition = initialPistonPositions[i] + (localDirection * offset);
        }

        for (int i = 0; i < pipes.Length; i++)
        {
            float jitterX = (Mathf.PerlinNoise(Time.time * pipeVibrationSpeed, i) - 0.5f) * actualPipeIntensity;
            float jitterY = (Mathf.PerlinNoise(i, Time.time * pipeVibrationSpeed) - 0.5f) * actualPipeIntensity;
            float jitterZ = (Mathf.PerlinNoise(Time.time * pipeVibrationSpeed + i, Time.time * pipeVibrationSpeed - i) - 0.5f) * actualPipeIntensity;

            pipes[i].localPosition = initialPipePositions[i] + new Vector3(jitterX, jitterY, jitterZ);
        }

        for (int i = 0; i < rotors.Length; i++)
        {
            float noise = Mathf.PerlinNoise(Time.time * rotorSpeedFluctuation, rotorNoiseSeeds[i]);
            float currentSpeed = Mathf.Lerp(minRotorSpeed, maxRotorSpeed, noise);

            rotors[i].Rotate(rotorAxis.normalized, currentSpeed * Time.deltaTime, Space.Self);
        }
    }
}
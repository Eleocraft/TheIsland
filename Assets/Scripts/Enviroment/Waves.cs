using UnityEngine;
using System.Collections.Generic;

public class Waves : MonoSingleton<Waves>
{
    [Header("--WaveProperties")]
    [SerializeField] private float waveSpeed = 0.07f;
    [SerializeField] private float waveFrequency = 4f;
    [SerializeField] private float waveAmplitude = 1f;
    [SerializeField] private float turningSpeed;
    [SerializeField] private float turningTime;
    [SerializeField] private float StartAngle;
    [Header("--for the shader")]
    [SerializeField] private float chunkSize = 100f;
    [SerializeField] private Material waterShader;
    private static float waterHeight;
    public static Vector2 WaveDirektion { get; private set; }
    private float waveAngle;
    private float targetAngle;
    private float timer;
    void Start()
    {
        waveAngle = StartAngle;
        timer = turningTime;
        waterHeight = MapRuntimeHandler.waterHeight;
        ActualizeDirection();
    }
    void ActualizeDirection()
    {
        WaveDirektion = Vector2.up.Rotate(waveAngle);
        waterShader.SetFloat("_WaveFrequency", waveFrequency);
        waterShader.SetFloat("_WaveAmplitude", waveAmplitude);
        waterShader.SetFloat("_WaveSpeed", waveSpeed);
        waterShader.SetVector("_WaveDir", WaveDirektion);
    }
    void Update()
    {
        if (Mathf.Abs(waveAngle - targetAngle) > 0)
        {
            waveAngle = Utility.RotateTowards(waveAngle, targetAngle, turningSpeed * Time.deltaTime);
            ActualizeDirection();
            return;
        }
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = turningTime;
            targetAngle = Random.Range(0f, 360f);
        }
    }
    private static float GetNoiseValue(float t)
    {
        float cos = Mathf.Cos(t * 0.5f);
        float sin = Mathf.Sin(t);
        return sin + cos;
    }
    private static float GetCombinedValue(Vector2 transformedUV, Vector2 inversedTransformedUV)
    {
        float HeightPosX = GetNoiseValue(transformedUV.x) * Mathf.Clamp01(WaveDirektion.x);
        float HeightPosY = GetNoiseValue(transformedUV.y) * Mathf.Clamp01(WaveDirektion.y);
        float HeightNegX = GetNoiseValue(inversedTransformedUV.x) * Mathf.Clamp01(WaveDirektion.x * -1f);
        float HeightNegY = GetNoiseValue(inversedTransformedUV.y) * Mathf.Clamp01(WaveDirektion.y * -1f);
        return HeightPosX + HeightPosY + HeightNegX + HeightNegY;
    }
    public static float GetWaveHeight(Vector2 pos)
    {
        Vector2 UV = pos * -1f / new Vector2(-Instance.chunkSize, Instance.chunkSize) + 0.5f * Vector2.one;
        Vector2 transformedUV = (Time.time * Instance.waveSpeed * Vector2.one + UV) * Instance.waveFrequency;
        Vector2 inversedTransformedUV = (-1f * Time.time * Instance.waveSpeed * Vector2.one + UV) * Instance.waveFrequency;
        float NoiseValue = GetCombinedValue(transformedUV, inversedTransformedUV) * Instance.waveAmplitude;
        return NoiseValue + waterHeight;
    }
    [Command]
    public static void SetWaveAngle(List<string> args)
    {
        Instance.targetAngle = float.Parse(args[0]);
    }
}

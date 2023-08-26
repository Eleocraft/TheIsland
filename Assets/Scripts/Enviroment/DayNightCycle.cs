using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[CommandGroup("dayNightCycle")]
public class DayNightCycle : MonoSingleton<DayNightCycle>
{
    public float time {get; private set;}
    [SerializeField] private float fullDayLength;
    [SerializeField] private float startTime = 0.4f;
    [HideInInspector] public float timeRate;

    [Header("Sun")]
    [SerializeField] private Light sun;
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity;
    [SerializeField] private Vector3 sunAxis;

    [Header("Moon")]
    [SerializeField] private Light moon;
    [SerializeField] private Gradient moonColor;
    [SerializeField] private AnimationCurve moonIntensity;
    [SerializeField] private Vector3 moonAxis;
    [SerializeField] private float moonOrbitDuration;

    [Header("Other Lighting")]
    [SerializeField] private AnimationCurve lightingIntensityMultiplier;
    [SerializeField] private AnimationCurve reflectionsIntensityMultiplier;

    [Header("Fog/Cloud Color")]
    [SerializeField] private Texture2D SunZenithGrad;
    [SerializeField] private Texture2D ViewZenithGrad;
    [SerializeField] private float fogBrightness;
    [SerializeField] private Material cloudMaterial;
    [SerializeField] private Gradient cloudColor;

    private float moonTime;
    private float moonTimeRate;

    [Save(SaveType.world)]
    public static object SavableTime
    {
        get => Instance.time;
        set => Instance.time = (float)value;
    }
    [Save(SaveType.world)]
    public static object SavableTimeScale
    {
        get => Instance.timeRate;
        set => Instance.timeRate = (float)value;
    }
    [Save(SaveType.world)]
    public static object SavableMoonTime
    {
        get => Instance.moonTime;
        set => Instance.moonTime = (float)value;
    }

    protected override void SingletonAwake()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
        moonTimeRate = 1.0f / moonOrbitDuration;
    }

    void Update()
    {
        time += timeRate * Time.deltaTime;
        if (time >= 1.0f)
            time = 0.0f;

        moonTime += moonTimeRate * Time.deltaTime;
        if (moonTime >= 1.0f)
            moonTime = 0.0f;
        
        sun.transform.rotation = Quaternion.AngleAxis((time - 0.25f) * 360, sunAxis);
        moon.transform.rotation = Quaternion.AngleAxis(moonTime * 360, moonAxis);

        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(moonTime);

        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(moonTime);

        if (sun.intensity <= 0 && sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(false);
        else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(true);

        if (moon.intensity <= 0 && moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(false);
        else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(true);

        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);
        
        DynamicGI.UpdateEnvironment();

        // Fog
        float sunZenithDot01 = (-sun.transform.forward.y + 1.0f) * 0.5f;
        Color sunZenithColor = SunZenithGrad.GetPixel(Mathf.RoundToInt(sunZenithDot01 * SunZenithGrad.width), Mathf.RoundToInt(SunZenithGrad.height*0.5f));
        Color viewZenithColor = ViewZenithGrad.GetPixel(Mathf.RoundToInt(sunZenithDot01 * ViewZenithGrad.width), Mathf.RoundToInt(ViewZenithGrad.height*0.5f));
        Color skyColor = sunZenithColor + viewZenithColor;
        RenderSettings.fogColor = skyColor * fogBrightness;

        // Clouds
        cloudMaterial.SetColor("_DarkColor", cloudColor.Evaluate(time));
    }
    void LateUpdate()
    {
        // Set Rotations for sun and moon
        // Sun
        Shader.SetGlobalVector("_SunDir", -sun.transform.forward);

        // Moon
        Shader.SetGlobalVector("_MoonDir", -moon.transform.forward);
    }
    public void Sync(float time, float timeRate)
    {
        this.time = time;
        this.timeRate = timeRate;
    }
    // dayNightCircle Commands
    [Command("stop", description="stop time", serverOnly=true)]
    public static void StopTime(List<string> Parameters)
    {
        Instance.timeRate = 0f;
    }
    [Command("defaultDayLength", description="reset time speed", serverOnly=true)]
    public static void ResetTimeSpeed(List<string> Parameters)
    {
        Instance.timeRate = 1.0f / Instance.fullDayLength;
    }
    [Command("setDaylength", description="set time speed", serverOnly=true)]
    public static void SetDayLength(List<string> Parameters)
    {
        Instance.timeRate = 1.0f / float.Parse(Parameters[0]);
    }
    [Command("setTime", description="set time", serverOnly=true)]
    public static void SetTime(List<string> Parameters)
    {
        Instance.time = float.Parse(Parameters[0]) / Instance.fullDayLength;
    }
}

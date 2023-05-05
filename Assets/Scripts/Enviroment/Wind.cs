using UnityEngine;

public class Wind : MonoSingleton<Wind>
{
    [Header("--WindProperties")]
    [SerializeField] private float WindStrength;
    [SerializeField] private float WindFrequency;
    [SerializeField] private float turningSpeed;
    [SerializeField] private float turningTime;
    [SerializeField] private float StartAngle;
    [Header("--Particles")]
    [SerializeField] private GameObject WindParticleObject;
    [SerializeField] private float ParticleYOffset;
    private float windAngle;
    private float targetAngle;
    private float timer;
    private bool outOfTutorialZone;
    public static Vector2 WindVelocity { get; private set; }
    public static Vector2 WindPosition { get; private set; }
    void Start()
    {
        windAngle = StartAngle;
        targetAngle = windAngle;
        timer = turningTime;
        ActualizeDirection();
    }
    void ActualizeDirection()
    {
        WindVelocity = (Vector2.up * WindStrength).Rotate(windAngle);
        WindParticleObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, WindVelocity.AddHeight(0));

        Shader.SetGlobalVector("_WindVelocity", WindVelocity);
        Shader.SetGlobalFloat("_WindFrequency", WindFrequency);
    }
    void Update()
    {
        WindParticleObject.transform.position = PlayerData.Position + Vector3.up * ParticleYOffset;
        WindPosition += WindVelocity * Time.deltaTime;
        Shader.SetGlobalVector("_WindPosition", WindPosition);

        if (Mathf.Abs(windAngle - targetAngle) > 0)
        {
            windAngle = Utility.RotateTowards(windAngle, targetAngle, turningSpeed * Time.deltaTime);
            ActualizeDirection();
            return;
        }
        timer -= Time.deltaTime;
        if (timer < 0 && outOfTutorialZone)
        {
            timer = turningTime;
            targetAngle = Random.Range(0f, 360f);
        }
    }
    public static void StartTimer() => Instance.outOfTutorialZone = true;
}

using UnityEngine;

public class RaftMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float windEffectStrength;
    [SerializeField] private float waveEffectStrength;
    [SerializeField] private float RotationStrength;
    [SerializeField] private float SpeedRotImpact;
    [SerializeField] private AnimationCurve WindEffectiveness;
    RaftSteeringFin steeringFin;
    RaftSail sail;
    CarryPlayer carryPlayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        steeringFin = GetComponentInChildren<RaftSteeringFin>();
        sail = GetComponentInChildren<RaftSail>();
        carryPlayer = GetComponent<CarryPlayer>();
    }
    void FixedUpdate()
    {
        if (carryPlayer.CarryingPlayer)
            rb.AddForce(Waves.WaveDirektion * waveEffectStrength);
        rb.AddTorque(new Vector3(0, -steeringFin.realRotation * RotationStrength * Mathf.Clamp(rb.velocity.magnitude*SpeedRotImpact, 1, 10), 0));
        if (sail.open)
            rb.AddForce(transform.forward * windEffectStrength * WindEffectiveness.Evaluate(Vector2.Angle(Wind.WindVelocity, transform.forward.XZ()) / 180f));
    }
}
using UnityEngine;

public class SimpleFloater : MonoBehaviour, IFloater
{
    [SerializeField] private float Force = 50;
    [SerializeField] private float FloaterDepth = 0.5f;
    private Rigidbody rb;
    private bool _active = true;
    public bool Active
    {
        get => _active;
        set{
            _active = value;
            if (value)
                rb = GetComponent<Rigidbody>();
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        if (!_active)
            return;

        float WaveHeight = Waves.GetWaveHeight(transform.position.XZ());
        if (WaveHeight > transform.position.y)
            rb.AddForce(Vector3.up * Mathf.Clamp(WaveHeight - transform.position.y, 0, FloaterDepth) * Force, ForceMode.Force);
    }
}

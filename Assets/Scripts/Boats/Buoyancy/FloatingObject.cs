using UnityEngine;
public class FloatingObject : MonoBehaviour, IFloater
{
    [SerializeField] private Transform[] Floaters;
    [SerializeField] private float Force;
    [SerializeField] private float FloaterDepth;
    private Rigidbody rb;
    private float forcePerFloater;
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
        forcePerFloater = Force / Floaters.Length;
    }
    void FixedUpdate()
    {
        if (!_active)
            return;
        foreach (Transform floater in Floaters)
        {
            float WaveHeight = Waves.GetWaveHeight(new Vector2(floater.position.x, floater.position.z));
            if (WaveHeight > floater.position.y)
                rb.AddForceAtPosition(Vector3.up * Mathf.Clamp(WaveHeight - floater.position.y, 0, FloaterDepth) * forcePerFloater, floater.position, ForceMode.Force);
        }
    }
}

using UnityEngine;

public class FixedFloater : MonoBehaviour, IFloater
{
    private bool _active = true;
    public bool Active
    {
        get => _active;
        set => _active = value;
    }
    void FixedUpdate()
    {
        if (!_active)
            return;

        float WaveHeight = Waves.GetWaveHeight(transform.position.XZ());
        transform.position = transform.position.WithHeight(WaveHeight);
    }
}

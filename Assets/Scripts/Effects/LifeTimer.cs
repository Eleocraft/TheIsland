using UnityEngine;

public class LifeTimer : MonoBehaviour
{
    [SerializeField] private float Lifetime;
    void Start()
    {
        this.Invoke(() => Destroy(gameObject), Lifetime);
    }
}

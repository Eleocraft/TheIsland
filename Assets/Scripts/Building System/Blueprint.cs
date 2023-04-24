using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Blueprint : MonoBehaviour
{
    private int collisions;
    public bool Blocked => collisions > 0;
    private void OnTriggerEnter(Collider col)
    {
        collisions++;
    }
    private void OnTriggerExit(Collider col)
    {
        collisions--;
    }
}

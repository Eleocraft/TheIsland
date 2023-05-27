using UnityEngine;

public class OrientateTowardsCamera : MonoBehaviour
{
    private Transform cam;
    void Start()
    {
        cam = Camera.main.transform;
    }
    void Update()
    {
        transform.rotation = cam.transform.rotation;
    }
}

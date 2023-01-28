using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    private static Camera cam;
    private static UniversalAdditionalCameraData cameraData;
    void Start()
    {
        cam = Camera.main;
        cameraData = cam.GetUniversalAdditionalCameraData();
        UpdateVolumeStack();
    }

    public static void UpdateVolumeStack()
    {
        cam.UpdateVolumeStack(cameraData);
    }
}

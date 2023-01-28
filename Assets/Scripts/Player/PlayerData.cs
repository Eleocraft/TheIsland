using UnityEngine;

public class PlayerData : MonoSingleton<PlayerData>
{
    public static Vector3 Position
    {
        get => Instance.transform.position;
        set => Instance.transform.position = value;
    }
    public static Quaternion Roatation
    {
        get => Instance.transform.rotation;
        set => Instance.transform.rotation = value;
    }
    public static GameObject GameObject => Instance.gameObject;
    public static Camera MainCam => Camera.main;
}

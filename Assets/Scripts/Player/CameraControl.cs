using UnityEngine;
using System;

public class CameraControl : MonoSingleton<CameraControl>
{
    private InputMaster controls;   

    [Header("--View")]
    [SerializeField] private float mouseSensitivity;

    [Header("--FirstPersonView")]
    [SerializeField] private Transform XRotationTransform;
    [SerializeField] [Range(0, 90)] private int lookUpAngle = 90;
    [SerializeField] [Range(0, 90)] private int lookDownAngle = 90;

    [Header("--ThirdPersonView")]
    [SerializeField] private Transform ThirdPersonCameraTransform;
    [SerializeField] private float ThirdPersonCameraDist;
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float CamSize;

    [HideInInspector] public Vector2 Rotation;
    private Rigidbody Rb;

    private bool ThirdPersonView;
    private float YRotOffset;

    [Save(SaveType.player)]
    public static object PlayerRotation
    {
        get => Instance.Rotation.y;
        set => Instance.Rotation.y = (float)value;
    }
    void Start()
    {
        //headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        controls = GlobalData.controls;
        Rb = GetComponent<Rigidbody>();
    }
    public static void SetDummyRb(Rigidbody Rb) => Instance.Rb = Rb;
    public static void ResetRb() => Instance.Rb = Instance.GetComponent<Rigidbody>();
    public static void SetYRotOffset(float YRotOffset) => Instance.YRotOffset = YRotOffset;
    public static void SetYRotation(float YRotation)
    {
        Instance.Rotation.y = YRotation;
        Instance.Rb.MoveRotation(Quaternion.Euler(0, YRotation, 0));
    }
    void Update()
    {
        // View
        Vector2 View = controls.Mouse.View.ReadValue<Vector2>() * mouseSensitivity * 0.01f;
        Rotation.x -= View.y;
        Rotation.y += View.x;
        Rotation.x = Mathf.Clamp(Rotation.x, -lookUpAngle, lookDownAngle);
        
        // Fix Rotation y between 0 and 360
        Rotation.y = Rotation.y.ClampToAngle();
        Rb.MoveRotation(Quaternion.Euler(0, Rotation.y, 0));
        XRotationTransform.localRotation = Quaternion.Euler(Rotation.x, 0, 0);
    }
    void LateUpdate()
    {
        //headTransform.rotation = Quaternion.Euler(Rotation.x, transform.rotation.eulerAngles.y, 0f);
        if (ThirdPersonView)
        {
            Vector3 LookRotation = Quaternion.Euler(Rotation.x, Rotation.y + YRotOffset, 0) * Vector3.back;
            if (Physics.SphereCast(transform.position, CamSize, LookRotation, out RaycastHit hitData, ThirdPersonCameraDist, cameraCollisionMask))
                ThirdPersonCameraTransform.position = hitData.point;
            else
                ThirdPersonCameraTransform.position = transform.position + LookRotation * ThirdPersonCameraDist;
        }
	}
    [SliderSetting(SettingCategory.Input, SettingLoadTime.MainSceneLoad, "View Sensitivity", "50", 0, 100)]
    public static void ChangeViewSensitivity(string settingValue)
    {
        Instance.mouseSensitivity = float.Parse(settingValue);
    }
}
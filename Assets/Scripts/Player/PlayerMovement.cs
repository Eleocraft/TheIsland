using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoSingleton<PlayerMovement>
{
    private InputMaster controls;
    public enum MoveMode {Walk, Fly}
    [SerializeField] private MoveMode _moveMode;
    private float drag;
    public MoveMode moveMode 
    {
        get => _moveMode;
        set {
            _moveMode = value;
            flySpeedMultiplier = (value == MoveMode.Fly) ? FlyingSpeedMultiplier : 1f;
            mainRB.drag = (value == MoveMode.Fly) ? 0 : drag;
        }
    }
    [HideInInspector] public Rigidbody mainRB;
    private Transform groundCheck;
    
    [Header("--Movement")]
    [Range(0f, 100f)] [SerializeField] private float jumpForce = 3f;
    [Range(0f, 5f)] [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [Range(0f, 10f)] [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private Transform defaultGroundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float gravity;

    [Header("--Animation")]
    [Range(0.1f, 0.5f)] [SerializeField] private float AnimationLerp;

    [Header("--flyMode")]
    [SerializeField] private float FlyingSpeedMultiplier;
    [SerializeField] private float VerticalSpeed;

    public static RaycastHit GroundHitData => Instance.groundHitData;
    private RaycastHit groundHitData;

    private float flySpeedMultiplier = 1;
    private bool Sprinting;
    Vector2 Direction;
    bool onSolidGround;

    [Save(SaveType.player)]
    public static object PlayerPositionSaveData
    {
        get => new SerializableVector3(Instance.transform.position);
        set => Instance.SetPosition(((SerializableVector3)value).getVec());
    }
    [Save(SaveType.player)]
    public static object PlayerMoveModeSaveData
    {
        get => Instance.moveMode;
        set => Instance.moveMode = (MoveMode)value;
    }
    protected override void SingletonAwake()
    {
        mainRB = GetComponent<Rigidbody>();
    }
    void Start()
    {
        groundCheck = defaultGroundCheck;
        drag = mainRB.drag;
        controls = GlobalData.controls;
        controls.Player.Jump.performed += Jump;
        controls.Player.Running.started += Sprint;
        controls.Player.Running.canceled += Sprint;

        if (moveMode == MoveMode.Fly)
            flySpeedMultiplier = FlyingSpeedMultiplier;
    }
    void OnDestroy()
    {
        controls.Player.Jump.performed -= Jump;
        controls.Player.Running.started -= Sprint;
        controls.Player.Running.canceled -= Sprint;
    }
    void Jump(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        // Jumping (possible if on ground or in the water)
        float waterHeight = Waves.GetWaveHeight(new Vector2(transform.position.x, transform.position.z));
        if ((onSolidGround || transform.position.y < waterHeight) && moveMode == MoveMode.Walk)
        {
            mainRB.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            PlayerAnimationManager.SetTrigger("Jump");
        }
        // Fly Up
        else if (moveMode == MoveMode.Fly)
            mainRB.velocity = new Vector3(mainRB.velocity.x, VerticalSpeed, mainRB.velocity.z);
    }
    void Sprint(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        bool nowSprinting = ctx.started;
        // Sprinting
        if (moveMode == MoveMode.Walk)
        {
            if (onSolidGround && nowSprinting)
                Sprinting = true;
            
            else if (!nowSprinting)
                Sprinting = false;
        }
        // Fly down
        else if (moveMode == MoveMode.Fly)
            mainRB.velocity = new Vector3(mainRB.velocity.x, -VerticalSpeed, mainRB.velocity.z);
    }
    void Update()
    {
        Vector2 Input = controls.Player.Movement.ReadValue<Vector2>().normalized;
        // Sprinting
        if (Input.y >= 0 && Sprinting)
            Input.y *= sprintSpeedMultiplier;

        Direction = Vector2.MoveTowards(Direction, Input, AnimationLerp);

        PlayerAnimationManager.SetFloat("vertical", Direction.x);
        PlayerAnimationManager.SetFloat("horizontal", Direction.y);
    }
    void FixedUpdate()
    {
        // Movement
        onSolidGround = Physics.Raycast(groundCheck.position, Vector3.down, out groundHitData, groundDistance, groundMask);

        Vector3 Movement = (mainRB.transform.right * Direction.x + mainRB.transform.forward * Direction.y) * normalSpeed * flySpeedMultiplier;
        float currentYVelocity = mainRB.velocity.y;
        if (moveMode == MoveMode.Walk)
        {
            if (onSolidGround)
            {
                Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, groundHitData.normal);
                Vector3 realMovement = groundRotation * Movement;
                currentYVelocity = realMovement.y;
            }
            else
                currentYVelocity += gravity * Time.fixedDeltaTime;
        }
        // If in Flymode and neither spacebar or shift is pressed, velocity.y = 0
        else if (!controls.Player.Running.ReadValue<float>().AsBool() && !controls.Player.Jump.ReadValue<float>().AsBool())
            currentYVelocity = 0;
        
        mainRB.velocity = new Vector3(Movement.x, currentYVelocity, Movement.z);
    }
    public void SetPosition(Vector3 newPosition)
    {
        mainRB.MovePosition(newPosition);
    }
    public static GameObject ActivateDummyObject()
    {
        GameObject playerDummy = new("playerDummy");
        playerDummy.gameObject.tag = Instance.gameObject.tag;
        playerDummy.gameObject.SetLayerAllChildren(LayerMask.NameToLayer("Dummy"));
        CapsuleCollider playerCol = Instance.GetComponent<CapsuleCollider>();
        Rigidbody dummyRb = playerDummy.AddComponent(Instance.mainRB);
        dummyRb.position = Vector3.zero;
        playerDummy.AddComponent(playerCol);
        CameraControl.SetDummyRb(dummyRb);
        
        Destroy(Instance.mainRB);
        Destroy(playerCol);

        GameObject groundCheck = new("groundCheck");
        groundCheck.transform.parent = playerDummy.transform;
        groundCheck.transform.localPosition = Instance.groundCheck.localPosition;
        Instance.groundCheck = groundCheck.transform;
        Instance.mainRB = dummyRb;
        Instance.GetComponent<IFloater>().Active = false; // Temp
        return playerDummy;
    }
    public static void DeactivateDummyObject(GameObject playerDummy)
    {
        Vector3 pos = Instance.transform.position;
        Instance.mainRB = Instance.gameObject.AddComponent(playerDummy.GetComponent<Rigidbody>());
        Instance.SetPosition(pos);
        Instance.gameObject.AddComponent(playerDummy.GetComponent<CapsuleCollider>());
        Instance.groundCheck = Instance.defaultGroundCheck;
        CameraControl.ResetRb();
        Destroy(playerDummy);
        Instance.GetComponent<IFloater>().Active = true; // Temp
    }
    // Commands
    [Command("moveMode", description="moveMode fly / walk")]
    public static void MoveModeCommand(List<string> Parameters)
    {
        if (Parameters.Count < 1)
            Debug.LogError("Missing Argument for <MoveMode> in 'moveMode <MoveMode>'");

        if (Parameters[0] == "fly")
        {
            Instance.moveMode = MoveMode.Fly;
        }
        else if (Parameters[0] == "walk")
        {
            Instance.moveMode = MoveMode.Walk;
        }
        else
            Debug.LogError("Unknown Argument '" + Parameters[0] + "' for <MoveMode> in 'moveMode <MoveMode>'");
    }
    [Command("TP", description="TP <x> <y> <z>")]
    public static void TPCommand(List<string> Parameters)
    {
        try
        {
            Instance.SetPosition(new Vector3(float.Parse(Parameters[0]), float.Parse(Parameters[1]), float.Parse(Parameters[2])));
        }
        catch (System.Exception)
        {
            Debug.LogError("wrong Arguments");
        }
    }
    [Command]
    public static void GetPosition(List<string> Parameters)
    {
        Debug.Log(Instance.transform.position);
    }
    [Command]
    public static void speed(List<string> Parameters)
    {
        Instance.flySpeedMultiplier = float.Parse(Parameters[0]);
    }
}

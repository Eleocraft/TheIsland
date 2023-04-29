using UnityEngine;

public class RaftSteeringFin : MonoBehaviour, IInteractable
{
    public string InteractionInfo => $"hold {GlobalData.controls.Interaction.MainInteraction.bindings[0].ToDisplayString()} to turn";
    private RaftMovement movementControl;
    private bool active;
    private InputMaster controls;
    private float rotation;
    [SerializeField] private float maxAngle;
    [SerializeField] private float snapAngle;
    [SerializeField] private float turningSpeed;
    public float realRotation => Mathf.Abs(rotation) < snapAngle ? 0 : rotation;

    void Start()
    {
        movementControl = GetComponentInParent<RaftMovement>();
        controls = GlobalData.controls;
    }
    void Update()
    {
        if (!active)
            return;
        Vector2 deltaMovement = controls.Raft.ChangeValue.ReadValue<Vector2>() * turningSpeed;
        rotation -= deltaMovement.x;
        rotation = Mathf.Clamp(rotation, -maxAngle, maxAngle);
        transform.localRotation = Quaternion.Euler(0, realRotation, 0);
    }
    public void StartInteraction()
    {
        active = true;
        controls.Player.Disable();
    }
    public void StopInteraction()
    {
        active = false;
        controls.Player.Enable();
    }
}

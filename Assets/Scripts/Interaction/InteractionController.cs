using UnityEngine;
using TMPro;

public class InteractionController : MonoSingleton<InteractionController>
{
    private InputMaster controls;
    private InteractionData interaction;
    private InteractionData lastInteraction;
    [Header("--Raycast")]
    [Range(0.1f, 3)] [SerializeField] private float HitRange;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private LayerMask InteractableLayers;
    [Header("--Interaction")]
    [SerializeField] private TMP_Text informationField;
    
    void Start()
    {
        controls = GlobalData.controls;
        controls.PlayerFP.Interaction.performed += Interact;
        controls.PlayerFP.Interaction.started += StartInteraction;
        controls.PlayerFP.Interaction.canceled += StopInteraction;
    }
    void OnDestroy()
    {
        controls.PlayerFP.Interaction.performed -= Interact;
        controls.PlayerFP.Interaction.started -= StartInteraction;
        controls.PlayerFP.Interaction.canceled -= StopInteraction;
    }
    void FixedUpdate()
    {
        // Raycast to check if object is hit
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, InteractableLayers))
        {
            // Process the information
            if (hitData.collider.gameObject.TryGetComponent(out IInteractable interactable))
            {
                // the object can be interacted with directly
                interaction = new InteractionData(interactable, hitData);
                // Display Information
                if (interactable.InteractionInfo != informationField.text)
                    informationField.text = interactable.InteractionInfo;
                return;
            }
        }
        if (informationField.text != "")
            informationField.text = "";
        interaction = null;
    }
    void Interact(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (interaction != null)
            interaction.interactable.Interact();
    }
    void StartInteraction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (interaction != null)
        {
            interaction.interactable.StartInteraction();
            lastInteraction = interaction;
        }
    }
    void StopInteraction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (lastInteraction != null)
        {
            lastInteraction.interactable.StopInteraction();
            lastInteraction = null;
        }
    }
    // Get the interaction data
    public static InteractionData GetInteraction()
    {
        return Instance.interaction;
    }
    // Version of the GetInteraction for If-statements
    public static bool TryGetInteraction(out InteractionData interactionData)
    {
        interactionData = Instance.interaction;
        if (Instance.interaction != null)
            return true;
        return false;
    }
}
public class InteractionData
{
    public IInteractable interactable { get; private set; }
    public RaycastHit hitData { get; private set; }
    public GameObject interactableObject => hitData.collider.gameObject;

    public InteractionData(IInteractable interactable, RaycastHit hitData)
    {
        this.interactable = interactable;
        this.hitData = hitData;
    }
}
public interface IInteractable
{
    void Interact() { }
    void StartInteraction() { }
    void StopInteraction() { }
    string InteractionInfo { get; }
}
public class InteractionTrigger : MonoBehaviour, IInteractable
{
    private IInteractable interactable;
    public string InteractionInfo => interactable?.InteractionInfo;
    public void Initialize(IInteractable interactable)
    {
        this.interactable = interactable;
    }
    public void Interact() => interactable?.Interact();
    public void StartInteraction() => interactable?.StartInteraction();
    public void StopInteraction() => interactable?.StopInteraction();
}
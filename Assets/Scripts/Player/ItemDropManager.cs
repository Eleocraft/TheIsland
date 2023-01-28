using UnityEngine;

public class ItemDropManager : MonoSingleton<ItemDropManager>
{
    private InputMaster controls;
    [Header("--Drop info")]
    [SerializeField] private Transform dropSpawnPos;
    [SerializeField] private float dropForwardForce, dropUpwardForce;
    [Header("--Pickup info")]
    [SerializeField] private float pickUpRange;
    [SerializeField] private float pickUpSpeed;
    [Header("--Visuals")]
    [SerializeField] private GroundItem groundItemPrefab;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float upDownSpeed;
    [SerializeField] private float upDownDist;
    
    // Accessors
    public static float PickUpRange => Instance.pickUpRange;
    public static float PickUpSpeed => Instance.pickUpSpeed;
    public static float RotationSpeed => Instance.rotationSpeed;
    public static float UpDownSpeed => Instance.upDownSpeed;
    public static float UpDownDist => Instance.upDownDist;
    public static GroundItem GroundItemPrefab => Instance.groundItemPrefab;
    void Start()
    {
        controls = GlobalData.controls;
        controls.PlayerFP.Drop.performed += DropActiveItem;
    }
    void OnDestroy()
    {
        controls.PlayerFP.Drop.performed -= DropActiveItem;
    }
    public static void DropItemAmount(Item item, int amount) => Instance.dropItemAmount(item, amount);
    private void dropItemAmount(Item item, int amount)
    {
        GroundItem.Create(item, dropSpawnPos.position, Vector3.up * dropUpwardForce + transform.forward * dropForwardForce, amount, true);
    }
    private void DropActiveItem(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (PlayerInventory.UseActiveItem(out Item item))
            GroundItem.Create(item, dropSpawnPos.position, Vector3.up * dropUpwardForce + transform.forward * dropForwardForce, manualPickUp: true);
    }
}

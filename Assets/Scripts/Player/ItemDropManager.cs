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
        controls.Player.Drop.performed += DropActiveItem;
    }
    void OnDestroy()
    {
        controls.Player.Drop.performed -= DropActiveItem;
    }
    public static void DropItemAmount(ItemAmountInfo info) => Instance.dropItemFromPlayer(info);
    private void dropItemFromPlayer(ItemAmountInfo info)
    {
        GroundItem.Create(info.item, dropSpawnPos.position, Vector3.up * dropUpwardForce + transform.forward * dropForwardForce, info.amount, true);
    }
    private void DropActiveItem(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (PlayerInventory.UseActiveItem(out Item item))
            GroundItem.Create(item, dropSpawnPos.position, Vector3.up * dropUpwardForce + transform.forward * dropForwardForce, manualPickUp: true);
    }
}

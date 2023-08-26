using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    const float CheckTime = 0.5f;
    const float SqrDestroyDist = 0.5f;
    private float pickUpRange;
    private float pickUpSpeed;
    private float rotationSpeed;
    private float upDownSpeed;
    private float upDownDist;
    private Item item;
    private bool autoPickUp;
    private int amount;
    private GameObject itemModel;

    private float timer;
    private bool pickingUp;

    public string InteractionInfo => $"pick up {item.ItemObject.name} x {amount}";

    public static void Create(Item item, Vector3 position, Vector3 velocity, int amount = 1, bool manualPickUp = false)
    {
        GroundItem groundItem = Instantiate(ItemDropManager.GroundItemPrefab, position, Quaternion.identity);
        groundItem.item = item;
        groundItem.autoPickUp = !manualPickUp && !item.ItemObject.manualPickUp;
        groundItem.amount = amount;
        groundItem.GetComponent<Rigidbody>().velocity = velocity;
        groundItem.itemModel = Instantiate(item.ItemObject.GroundPrefab, groundItem.gameObject.transform);
        groundItem.itemModel.transform.localPosition = Vector3.zero;
    }
    void Start()
    {
        pickUpRange = ItemDropManager.PickUpRange;
        pickUpSpeed = ItemDropManager.PickUpSpeed;
        rotationSpeed = ItemDropManager.RotationSpeed;
        upDownSpeed = ItemDropManager.UpDownSpeed;
        upDownDist = ItemDropManager.UpDownDist;
        timer = CheckTime;
    }
    public void Interact()
    {
        pickingUp = true;
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
    }
    void Update()
    {
        itemModel.transform.Rotate(Vector3.up, rotationSpeed*Time.deltaTime);
        itemModel.transform.localPosition = Vector2.zero.AddHeight(Mathf.Sin(Time.time * upDownSpeed) * upDownDist);
        if (pickingUp)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerData.Position, Time.deltaTime * pickUpSpeed);
            if ((transform.position - PlayerData.Position).sqrMagnitude < SqrDestroyDist)
            {
                PlayerInventory.AddItem(new(item.ItemObject, amount));
                Destroy(gameObject);
            }
            return;
        }
        if (autoPickUp)
        {
            timer -= Time.deltaTime;
            if (timer > 0)
                return;
            timer = CheckTime;
            if (Physics.CheckSphere(transform.position, pickUpRange, LayerMask.GetMask("Player")))
            {
                pickingUp = true;
                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<Collider>());
            }
        }
    }
}
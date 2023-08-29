using UnityEngine;

public class BowActionController : MonoBehaviour
{
    [SerializeField] private Transform CameraTransform;
    private bool _drawing;
    private float _drawTime;
    private float _maxDraw;
    private Animator _bowAnimator;
    private void Start()
    {
        ItemActionController itemActionController = GetComponent<ItemActionController>();

        itemActionController.AddAction(ItemActionController.ActionType.MainActionStarted, ItemType.Bow, StartDrawing);
        itemActionController.AddAction(ItemActionController.ActionType.MainActionCanceled, ItemType.Bow, Loose);

        PlayerInventory.HotbarSlotChange += CancelDrawing;
    }
    private void OnDestroy()
    {
        PlayerInventory.HotbarSlotChange -= CancelDrawing;
    }
    private void Update()
    {
        if (_drawing && _drawTime < _maxDraw)
            _drawTime += Time.deltaTime;
    }
    private void StartDrawing(Item equippedItem)
    {
        BowItem bow = (BowItem)equippedItem.ItemObject;

        if (!PlayerInventory.UseItems(new ItemAmountInfo[] { new(bow.ArrowItem, 1) }))
            return;

        PlayerInventory.TryGetActiveItemGO(out GameObject go);
        _bowAnimator = go.GetComponent<Animator>();

        _bowAnimator.SetTrigger("StartDraw");
        _drawing = true;
        _drawTime = 0;
        _maxDraw = bow.TotalDrawTime;
    }
    private void Loose(Item equippedItem)
    {
        if (!_drawing)
            return;

        BowItem bow = (BowItem)equippedItem.ItemObject;
        _drawing = false;
        _bowAnimator.SetTrigger("Loose");
            
        if (_drawTime < bow.MinDrawTime)
        {
            PlayerInventory.AddItem(new(bow.ArrowItem, 1));
            return;
        }

        Vector3 velocity = CameraTransform.forward * bow.GetVelocity(_drawTime);
        Projectile.SpawnProjectile(bow.ArrowItem.Projectile, CameraTransform.position, velocity);
    }
    private void CancelDrawing()
    {
        if (_drawing)
        {
            _drawing = false;
            _bowAnimator.SetTrigger("Loose");
        }
    }
}

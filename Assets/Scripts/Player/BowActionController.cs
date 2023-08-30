using UnityEngine;

public class BowActionController : MonoBehaviour
{
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private Animator ArmAnimator;
    [SerializeField] private AnimationEventHandler armAnimationEventHandler;
    [SerializeField] private float MaxLockonRange = 80;
    [SerializeField] private float MinLockonRange = 2;
    private bool _drawing;
    private bool _inAction;
    private float _drawTime;
    private float _maxDraw;
    private Animator _bowAnimator;
    private GameObject _arrowDrawObject;
    private Transform _bowLoosePoint;
    private void Start()
    {
        ItemActionController itemActionController = GetComponent<ItemActionController>();

        itemActionController.AddAction(ItemActionController.ActionType.MainActionStarted, ItemType.Bow, StartDrawing);
        itemActionController.AddAction(ItemActionController.ActionType.MainActionCanceled, ItemType.Bow, Loose);

        PlayerInventory.HotbarSlotChange += HotbarSlotChanged;

        armAnimationEventHandler.Events["LooseBow"] += EndPlaying;
    }
    private void OnDestroy()
    {
        PlayerInventory.HotbarSlotChange -= HotbarSlotChanged;
        armAnimationEventHandler.Events["LooseBow"] -= EndPlaying;
    }
    private void Update()
    {
        if (!_drawing) return;

        if (_drawTime < _maxDraw)
            _drawTime += Time.deltaTime;
        
        _arrowDrawObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, _bowLoosePoint.position - _arrowDrawObject.transform.position);
    }
    private void StartDrawing(Item equippedItem)
    {
        BowItem bow = (BowItem)equippedItem.ItemObject;

        if (_inAction)
            return;

        if (!PlayerInventory.UseItems(new ItemAmountInfo[] { new(bow.ArrowItem, 1) }))
            return;

        _bowAnimator = PlayerInventory.GetActiveItemGO().GetComponent<Animator>();
        _bowAnimator.speed = 1f / bow.TotalDrawTime;
        ArmAnimator.speed = 1f / bow.TotalDrawTime;
        _arrowDrawObject.SetActive(true);

        _bowAnimator.SetBool("Draw", true);
        ArmAnimator.SetBool("DrawBow", true);
        _drawing = true;
        _inAction = true;
        _drawTime = 0;
        _maxDraw = bow.TotalDrawTime;
    }
    private void Loose(Item equippedItem)
    {
        if (!_drawing)
            return;

        BowItem bow = (BowItem)equippedItem.ItemObject;
        _drawing = false;
        _bowAnimator.SetBool("Draw", false);
        ArmAnimator.SetBool("DrawBow", false);
        _arrowDrawObject.SetActive(false);
            
        if (_drawTime < bow.MinDrawTime)
        {
            PlayerInventory.AddItem(new(bow.ArrowItem, 1));
            return;
        }
        Vector3 velocity = GetShootDirection() * bow.GetVelocity(_drawTime);
        Projectile.SpawnProjectile(bow.ArrowItem.Projectile, _bowLoosePoint.position, velocity);
    }
    private Vector3 GetShootDirection()
    {
        if (Physics.Raycast(CameraTransform.position, CameraTransform.forward, out RaycastHit hitInfo, MaxLockonRange) && hitInfo.distance > MinLockonRange)
            return (hitInfo.point - _bowLoosePoint.position).normalized;
        
        return (CameraTransform.position + CameraTransform.forward * MaxLockonRange - _bowLoosePoint.position).normalized;
    }
    private void EndPlaying()
    {
        _inAction = false;
        ArmAnimator.speed = 1;
    }
    private void HotbarSlotChanged()
    {
        if (_inAction) // cancel drawing
        {
            _drawing = false;
            _inAction = false;
            _bowAnimator.SetBool("Draw", false);
            ArmAnimator.SetBool("DrawBow", false);
            ArmAnimator.speed = 1;
        }
        else if (PlayerInventory.TryGetActiveItem(out Item equippedItem) && equippedItem.ItemObject.Type == ItemType.Bow)
        {
            BowInformation information = PlayerInventory.GetActiveItemGO().GetComponent<BowInformation>();
            _bowLoosePoint = information.BowLoosePoint;
            _arrowDrawObject = Instantiate(((BowItem)equippedItem.ItemObject).ArrowItem.ArrowObject, information.ArrowContainer);
            _arrowDrawObject.SetActive(false);
        }
        else if (_arrowDrawObject != null)
            Destroy(_arrowDrawObject);
    }
}

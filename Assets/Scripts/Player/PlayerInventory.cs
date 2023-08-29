using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerInventory : MonoSingleton<PlayerInventory>
{
    private Inventory inventory;
    private Inventory equipmentInventory;
    private Inventory hotbar;
    [Header("--Panels")]
    [SerializeField] private InventoryInterface inventoryInterface;
    [SerializeField] private InventoryInterface equipmentInventoryInterface;
    [SerializeField] private HotbarInterface hotbarInterface;
    public static InventoryInterface ChestInterface => Instance.chestInterface;
    [SerializeField] private InventoryInterface chestInterface;
    [Header("--Main Player Inv Settings")]
    [SerializeField] private int maxStack;
    [SerializeField] private int slotCount;
    [Header("--Hotbar and Equipmentpanel Settings")]
    [SerializeField] private int hotbarSlotCount;
    [SerializeField] private int eqSlotCount;
    [Header("--UI")]
    [SerializeField] private GameObject newItemDisplay;
    [SerializeField] private Transform itemAddMessagePlacement;
    [Range(20, 100)] [SerializeField] private float distanceBetweenDisplayObjects;
    [SerializeField] private float decayTime;
    [SerializeField] private float fadeTime;
    [Header("--PlayerModel")]
    [SerializeField] private Transform ToolContainer;
    [SerializeField] private Transform BowContainer;
    [SerializeField] private Animator armAnimator;
    private Dictionary<ItemObject, GameObject> newItemDisplayObjects = new Dictionary<ItemObject, GameObject>();
    private bool active;
    private int activeSlot = 0;
    private InputMaster controls;
    private event Action hotbarSlotChange;
    public static Action HotbarSlotChange { get => Instance.hotbarSlotChange; set { Instance.hotbarSlotChange = value; } }
    private GameObject ItemInHand;
    private event Action closeCallback;

    public static int MaxStack => Instance.maxStack;

    public static Item activeItem => Instance.hotbar.Slots[Instance.activeSlot].item;

    // Save data
    [Save(SaveType.player)]
    public static object PlayerInventorySaveData
    {
        get => Instance.inventory;
        set {
            Instance.inventory = (Inventory)value;
            Instance.inventoryInterface.Initialize(Instance.inventory);
            Instance.inventory.Load();
        }
    }
    [Save(SaveType.player)]
    public static object EquipmentInventorySaveData
    {
        get => Instance.equipmentInventory;
        set {
            Instance.equipmentInventory = (Inventory)value;
            Instance.equipmentInventoryInterface.Initialize(Instance.equipmentInventory);
            Instance.equipmentInventory.Load();
        }
    }
    [Save(SaveType.player)]
    public static object HotbarSaveData
    {
        get => Instance.hotbar;
        set {
            Instance.hotbar = (Inventory)value;
            Instance.hotbarInterface.Initialize(Instance.hotbar);
            Instance.hotbar.Load();
        }
    }
    [Save(SaveType.player)]
    public static object SelectedHotbarSlot
    {
        get => Instance.activeSlot;
        set => Instance.activeSlot = (int)value;
    }
    public static bool TryGetActiveItem(out Item item)
    {
        item = Instance.hotbar.Slots[Instance.activeSlot].item;
        return item != null;
    }
    public static bool TryGetActiveItemGO(out GameObject GO)
    {
        GO = Instance.ItemInHand;
        return GO != null;
    }
    protected override void SingletonAwake()
    {
        if (GlobalData.loadMode == LoadMode.Load)
            return;
        
        inventory = new Inventory(slotCount, maxStack);
        equipmentInventory = new Inventory(eqSlotCount, 1);
        hotbar = new Inventory(hotbarSlotCount, maxStack);

        inventoryInterface.Initialize(inventory);
        equipmentInventoryInterface.Initialize(equipmentInventory);
        hotbarInterface.Initialize(hotbar);
    }

    void Start()
    {
        controls = GlobalData.controls;
        controls.Menus.ToggleInventory.performed += toggleInventoryKey;

        controls.Player.Hotbarslot1.performed += ChangeToHotbarSlot1;
        controls.Player.Hotbarslot2.performed += ChangeToHotbarSlot2;
        controls.Player.Hotbarslot3.performed += ChangeToHotbarSlot3;
        controls.Player.Hotbarslot4.performed += ChangeToHotbarSlot4;
        controls.Player.Hotbarslot5.performed += ChangeToHotbarSlot5;
        controls.Player.Hotbarslot6.performed += ChangeToHotbarSlot6;
        controls.Player.Hotbarslot7.performed += ChangeToHotbarSlot7;

        inventoryInterface.OnItemDropped += ItemDropManager.DropItemAmount;
        equipmentInventoryInterface.OnItemDropped += ItemDropManager.DropItemAmount;
        hotbarInterface.OnItemDropped += ItemDropManager.DropItemAmount;

        for (int i = 0; i < hotbar.Slots.Length; i++)
        {
            int SlotNum = i;
            hotbar.Slots[i].OnAfterUpdate += slot => { if (activeSlot == SlotNum) ChangeHotbarSlot(SlotNum); };
        }

        controls.Mouse.HotbarslotChange.performed += Scroll;
        ChangeHotbarSlot(activeSlot);
    }
    void ChangeToHotbarSlot1(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(0);
    void ChangeToHotbarSlot2(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(1);
    void ChangeToHotbarSlot3(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(2);
    void ChangeToHotbarSlot4(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(3);
    void ChangeToHotbarSlot5(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(4);
    void ChangeToHotbarSlot6(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(5);
    void ChangeToHotbarSlot7(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeHotbarSlot(6);

    void OnDestroy()
    {
        controls.Menus.ToggleInventory.performed -= toggleInventoryKey;
        controls.Player.Hotbarslot1.performed -= ChangeToHotbarSlot1;
        controls.Player.Hotbarslot2.performed -= ChangeToHotbarSlot2;
        controls.Player.Hotbarslot3.performed -= ChangeToHotbarSlot3;
        controls.Player.Hotbarslot4.performed -= ChangeToHotbarSlot4;
        controls.Player.Hotbarslot5.performed -= ChangeToHotbarSlot5;
        controls.Player.Hotbarslot6.performed -= ChangeToHotbarSlot6;
        controls.Player.Hotbarslot7.performed -= ChangeToHotbarSlot7;

        inventoryInterface.OnItemDropped -= ItemDropManager.DropItemAmount;
        equipmentInventoryInterface.OnItemDropped -= ItemDropManager.DropItemAmount;
        hotbarInterface.OnItemDropped -= ItemDropManager.DropItemAmount;

        if(closeCallback != null)
            foreach (Delegate d in closeCallback.GetInvocationList())
                closeCallback -= d as Action;
    }
    void Scroll(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Vector2 ScrollDirection = ctx.ReadValue<Vector2>();
        int newSlot = activeSlot - (int)ScrollDirection.y;
        if (newSlot < 0)
            newSlot = hotbarSlotCount - 1;
        else if (newSlot > hotbarSlotCount - 1)
            newSlot = 0;
        ChangeHotbarSlot(newSlot);
    }
    void ChangeHotbarSlot(int newSlot)
    {
        hotbarInterface.ChangeHotbarSlot(newSlot);
        activeSlot = newSlot;
        UpdatePortableItem();
        hotbarSlotChange?.Invoke();
    }
    void UpdatePortableItem()
    {
        Destroy(ItemInHand);
        PortableItem portableItem = activeItem?.ItemObject as PortableItem;

        if (portableItem == null)
        {
            armAnimator.SetBool("HoldToolItem", false);
            armAnimator.SetBool("HoldBowItem", false);
        }
        else if (portableItem.portableItemAnimation == PortableItem.PortableItemAnimation.Tool)
        {
            armAnimator.SetBool("HoldToolItem", true);
            armAnimator.SetBool("HoldBowItem", false);
            ItemInHand = Instantiate(portableItem.portableItemPrefab, ToolContainer);
        }
        else if (portableItem.portableItemAnimation == PortableItem.PortableItemAnimation.Bow)
        {
            armAnimator.SetBool("HoldToolItem", false);
            armAnimator.SetBool("HoldBowItem", true);
            ItemInHand = Instantiate(portableItem.portableItemPrefab, BowContainer);
        } 
    }
    public static void ToggleInventory(Action closeCallback)
    {
        Instance.toggleInventory();
        Instance.closeCallback += closeCallback;
    }
    private void toggleInventoryKey(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default) => toggleInventory();
    private void toggleInventory()
    {
        if (CursorStateMachine.AlreadyLocked(this))
            return;
        active = !active;
        inventoryInterface.gameObject.SetActive(active);
        equipmentInventoryInterface.gameObject.SetActive(active);
        if (active)
        {
            EscQueue.Enqueue(toggleInventory);
            CursorStateMachine.ChangeCursorState(false, this);
        }
        else
        {
            EscQueue.Remove(toggleInventory);
            CursorStateMachine.ChangeCursorState(true, this);
            closeCallback?.Invoke();
        }
    }
    public Inventory GetOtherActiveInventory(Inventory call)
    {
        return (call != inventory) ? inventory : equipmentInventory;
    }
    private void createNewItemDisplay(Item item, int amount)
    {
        if (newItemDisplayObjects.ContainsKey(item.ItemObject))
        {
            newItemDisplayObjects[item.ItemObject].GetComponent<newItemDisplay>().newCount(amount);
            return;
        }

        GameObject itemDisplayObject = Instantiate(newItemDisplay, Vector2.zero, Quaternion.identity, itemAddMessagePlacement);
        itemDisplayObject.transform.localPosition = new Vector2(0, -newItemDisplayObjects.Count * distanceBetweenDisplayObjects);
        itemDisplayObject.GetComponent<newItemDisplay>().Initialize(decayTime, fadeTime, item.ItemObject, amount);
        newItemDisplayObjects.Add(item.ItemObject, itemDisplayObject);
    }
    public void RemoveItemDisplayObject(ItemObject key)
    {
        newItemDisplayObjects.Remove(key);
        for (int i = 0; i < newItemDisplayObjects.Count; i++)
            newItemDisplayObjects.Values.ElementAt(i).transform.localPosition = new Vector2(0, -i * distanceBetweenDisplayObjects);
    }
    /// <summary>
    /// Public functions to interact with the playerinventory
    /// </summary>
    public static void AddItem(ItemAmountInfo itemInfo) => Instance.addItem(itemInfo);
    public void addItem(ItemAmountInfo itemInfo)
    {
        // First try adding to existing stacks (To prevent items from being added to the hotbar if it can stack in the inventory)
        if (hotbar.AddItemToStack(itemInfo.item, itemInfo.amount, out int droppedAmount))
        {
            createNewItemDisplay(itemInfo.item, itemInfo.amount);
            return;
        }
        if (inventory.AddItemToStack(itemInfo.item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(itemInfo.item, itemInfo.amount);
            return;
        }
        // Regular adding
        if (hotbar.AddItem(itemInfo.item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(itemInfo.item, itemInfo.amount);
            return;
        }
        if (inventory.AddItem(itemInfo.item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(itemInfo.item, itemInfo.amount);
            return;
        }
        if (itemInfo.amount != droppedAmount)
            createNewItemDisplay(itemInfo.item, itemInfo.amount - droppedAmount);
        
        ItemDropManager.DropItemAmount(new(itemInfo.item, droppedAmount));
        return;
    }
    public static bool ContainsItems(ItemAmountInfo itemInfo) => Instance.containsItems(itemInfo);
    public bool containsItems(ItemAmountInfo itemInfo)
    {
        int amountInHotbar = hotbar.GetItemCount(itemInfo.item);
        int amountInMain = inventory.GetItemCount(itemInfo.item);
        return amountInHotbar + amountInMain >= itemInfo.amount;
    }
    public static bool UseItems(ItemAmountInfo[] items) => Instance.useItems(items);
    public bool useItems(ItemAmountInfo[] items)
    {
        foreach (ItemAmountInfo itemInfo in items)
            if (!containsItems(itemInfo))
                return false;
        foreach (ItemAmountInfo itemInfo in items)
            removeItem(itemInfo);
        return true;
    }
    public bool removeItem(ItemAmountInfo itemInfo)
    {
        int amount = itemInfo.amount;
        if (hotbar.RemoveItem(itemInfo.item, ref amount))
            return true;
        if (inventory.RemoveItem(itemInfo.item, ref amount))
            return true;
        return false;
    }
    public static bool UseActiveItem(out Item item) => Instance.useActiveItem(out item);
    public bool useActiveItem(out Item item)
    {
        item = null;
        if (activeItem == null)
            return false;
        item = activeItem;
        hotbar.Slots[activeSlot].RemoveAmount(1);
        return true;
    }
    
    [Command("give", description="give <ItemID> <Amount>")]
    public static void GiveCommand(List<string> Parameters)
    {
        if (Parameters.Count < 1)
            Debug.LogError("Missing Item Id");
        if (Parameters.Count < 2)
            Instance.addItem(new(ItemDatabase.ItemObjects[Parameters[0]], 1));
        else
            Instance.addItem(new(ItemDatabase.ItemObjects[Parameters[0]], int.Parse(Parameters[1])));
    }
}
[Serializable]
public class DisplayItemAmountInfo : ItemAmountInfo
{
    public ItemObject itemObj;

    public DisplayItemAmountInfo(Item item, int amount) : base(item, amount) { }

    public override Item item => _item ?? itemObj.CreateItem();

}
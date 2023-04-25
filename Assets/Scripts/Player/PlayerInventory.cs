using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerInventory : MonoSingleton<PlayerInventory>
{
    private Inventory inventory;
    private Inventory equipmentInventory;
    private Inventory hotbar;
    private Inventory fightingHotbar;
    [Header("--Panels")]
    [SerializeField] private InventoryInterface inventoryInterface;
    [SerializeField] private InventoryInterface equipmentInventoryInterface;
    [SerializeField] private HotbarInterface hotbarInterface;
    [SerializeField] private InventoryInterface fightingHotbarInterface;
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
    [SerializeField] private Transform WeaponContainer;
    [SerializeField] private SkinnedMeshRenderer[] ThirdPersonArms;
    [SerializeField] private Animator armAnimator;
    private Dictionary<ItemObject, GameObject> newItemDisplayObjects = new Dictionary<ItemObject, GameObject>();
    private bool active;
    private int activeSlot = 0;
    private InputMaster controls;
    private event Action hotbarSlotChange;
    public static Action HotbarSlotChange { get => Instance.hotbarSlotChange; set { Instance.hotbarSlotChange = value; } }
    private GameObject ItemInHand;
    private GameObject ActiveWeapon;

    private event Action closeCallback;

    public static int MaxStack => Instance.maxStack;

    public static Item activeItem => Instance.hotbar.Slots[Instance.activeSlot].item;

    private bool PortableItem => (PlayerStateManager.State == PlayerState.ThirdPerson) ? false : activeItem?.ItemObject as PortableItem;

    public static Item mainWeapon => Instance.fightingHotbar.Slots[0].item;

    public static Item bow => Instance.fightingHotbar.Slots[1].item;
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
    [Save(SaveType.player)]
    public static object FightingHotbarSaveData
    {
        get => Instance.fightingHotbar;
        set {
            Instance.fightingHotbar = (Inventory)value;
            Instance.fightingHotbarInterface.Initialize(Instance.fightingHotbar);
            Instance.fightingHotbar.Load();
            Instance.fightingHotbar.Slots[0].OnAfterUpdate += slot => Instance.UpdatePortableWeapon();
            //Instance.fightingHotbar.Slots[1].OnAfterUpdate += slot => Instance.UpdatePortableWeapon();
        }
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
        fightingHotbar = new Inventory(3, 1);

        inventoryInterface.Initialize(inventory);
        equipmentInventoryInterface.Initialize(equipmentInventory);
        hotbarInterface.Initialize(hotbar);
        fightingHotbarInterface.Initialize(fightingHotbar);
    }

    void Start()
    {
        controls = GlobalData.controls;
        controls.Menus.ToggleInventory.performed += toggleInventoryKey;

        controls.PlayerFP.Hotbarslot1.performed += ChangeToHotbarSlot1;
        controls.PlayerFP.Hotbarslot2.performed += ChangeToHotbarSlot2;
        controls.PlayerFP.Hotbarslot3.performed += ChangeToHotbarSlot3;
        controls.PlayerFP.Hotbarslot4.performed += ChangeToHotbarSlot4;
        controls.PlayerFP.Hotbarslot5.performed += ChangeToHotbarSlot5;
        controls.PlayerFP.Hotbarslot6.performed += ChangeToHotbarSlot6;
        controls.PlayerFP.Hotbarslot7.performed += ChangeToHotbarSlot7;
        PlayerStateManager.GamemodeChange += ThirdPersonToggle;

        inventoryInterface.OnItemDropped += ItemDropManager.DropItemAmount;
        equipmentInventoryInterface.OnItemDropped += ItemDropManager.DropItemAmount;
        hotbarInterface.OnItemDropped += ItemDropManager.DropItemAmount;
        fightingHotbarInterface.OnItemDropped += ItemDropManager.DropItemAmount;

        for (int i = 0; i < hotbar.Slots.Length; i++)
        {
            int SlotNum = i;
            hotbar.Slots[i].OnAfterUpdate += slot => { if (activeSlot == SlotNum) ChangeHotbarSlot(SlotNum); };
        }

        controls.MouseFP.HotbarslotChange.performed += Scroll;
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
        controls.PlayerFP.Hotbarslot1.performed -= ChangeToHotbarSlot1;
        controls.PlayerFP.Hotbarslot2.performed -= ChangeToHotbarSlot2;
        controls.PlayerFP.Hotbarslot3.performed -= ChangeToHotbarSlot3;
        controls.PlayerFP.Hotbarslot4.performed -= ChangeToHotbarSlot4;
        controls.PlayerFP.Hotbarslot5.performed -= ChangeToHotbarSlot5;
        controls.PlayerFP.Hotbarslot6.performed -= ChangeToHotbarSlot6;
        controls.PlayerFP.Hotbarslot7.performed -= ChangeToHotbarSlot7;
        PlayerStateManager.GamemodeChange -= ThirdPersonToggle;

        inventoryInterface.OnItemDropped -= ItemDropManager.DropItemAmount;
        equipmentInventoryInterface.OnItemDropped -= ItemDropManager.DropItemAmount;
        hotbarInterface.OnItemDropped -= ItemDropManager.DropItemAmount;
        fightingHotbarInterface.OnItemDropped -= ItemDropManager.DropItemAmount;

        if(closeCallback != null)
            foreach (Delegate d in closeCallback.GetInvocationList())
                closeCallback -= d as Action;
    }
    void ThirdPersonToggle(PlayerState newState)
    {
        hotbarInterface.gameObject.SetActive(newState == PlayerState.FirstPerson);
        fightingHotbarInterface.gameObject.SetActive(newState == PlayerState.ThirdPerson);
        if (newState == PlayerState.ThirdPerson)
        {
            ActivateThirdPersonArms();
            Destroy(ItemInHand);
            UpdatePortableWeapon();
        }
        else
            UpdatePortableItem();
    }
    void ActivateThirdPersonArms()
    {
        foreach (SkinnedMeshRenderer obj in ThirdPersonArms)
            obj.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        armAnimator.gameObject.SetActive(false);
    }
    void ActivateFirstPersonArms()
    {
        foreach (SkinnedMeshRenderer obj in ThirdPersonArms)
            obj.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        armAnimator.gameObject.SetActive(true);
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
        if (PortableItem)
        {
            ActivateFirstPersonArms();
            armAnimator.Play("TakeObj");
            ItemInHand = Instantiate(((PortableItem)activeItem.ItemObject).portableItemPrefab, ToolContainer);
        }
        else
            ActivateThirdPersonArms();
    }
    void UpdatePortableWeapon()
    {
        Destroy(ActiveWeapon);
        if (mainWeapon != null)
            ActiveWeapon = Instantiate(((MeleeWeaponItem)mainWeapon.ItemObject).weaponPrefab, WeaponContainer);
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
            if (PlayerStateManager.State == PlayerState.ThirdPerson)
                hotbarInterface.gameObject.SetActive(true);
            else
                fightingHotbarInterface.gameObject.SetActive(true);
        }
        else
        {
            EscQueue.Remove(toggleInventory);
            CursorStateMachine.ChangeCursorState(true, this);
            if (PlayerStateManager.State == PlayerState.ThirdPerson)
                hotbarInterface.gameObject.SetActive(false);
            else
                fightingHotbarInterface.gameObject.SetActive(false);
            closeCallback?.Invoke();
        }
    }
    public Inventory GetOtherActiveInventory(Inventory call)
    {
        return (call != inventory) ? inventory : equipmentInventory;
    }
    public static void AddItem(Item item, int amount) => Instance.addItem(item, amount);
    public static void AddItem(ItemAmountInfo info) => Instance.addItem(info.itemObj.CreateItem(), info.amount);
    public void addItem(Item item, int amount)
    {
        // First try adding to existing stacks
        if (hotbar.AddItemToStack(item, amount, out int droppedAmount))
        {
            createNewItemDisplay(item, amount);
            return;
        }
        if (inventory.AddItemToStack(item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(item, amount);
            return;
        }
        // Regular adding
        if (hotbar.AddItem(item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(item, amount);
            return;
        }
        if (inventory.AddItem(item, droppedAmount, out droppedAmount))
        {
            createNewItemDisplay(item, amount);
            return;
        }
        if (amount != droppedAmount)
            createNewItemDisplay(item, amount - droppedAmount);
        
        ItemDropManager.DropItemAmount(item, droppedAmount);
        return;
    }
    public static bool ContainsItems(Item item, int amount) => Instance.containsItems(item, amount);
    public bool containsItems(Item item, int amount)
    {
        int amountInHotbar = hotbar.GetItemCount(item);
        int amountInMain = inventory.GetItemCount(item);
        return amountInHotbar + amountInMain >= amount;
    }
    public int getItemCount(Item item)
    {
        return hotbar.GetItemCount(item) + inventory.GetItemCount(item);
    }
    public bool removeItem(Item item, int amount)
    {
        if (hotbar.RemoveItem(item, ref amount))
            return true;
        if (inventory.RemoveItem(item, ref amount))
            return true;
        return false;
    }
    public static bool UseItems(List<ItemAmountInfo> items) => Instance.useItems(items);
    public bool useItems(List<ItemAmountInfo> items)
    {
        foreach (ItemAmountInfo itemInfo in items)
            if (!containsItems(itemInfo.itemObj.CreateItem(), itemInfo.amount))
                return false;
        foreach (ItemAmountInfo itemInfo in items)
            removeItem(itemInfo.itemObj.CreateItem(), itemInfo.amount);
        return true;
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
    
    [Command("give", description="give <ItemID> <Amount>")]
    public static void GiveCommand(List<string> Parameters)
    {
        if (Parameters.Count < 1)
            Debug.LogError("Missing Item Id");
        if (Parameters.Count < 2)
            Instance.addItem(ItemDatabase.ItemObjects[Parameters[0]].CreateItem(), 1);
        else
            Instance.addItem(ItemDatabase.ItemObjects[Parameters[0]].CreateItem(), int.Parse(Parameters[1]));
    }
}
[Serializable]
public struct ItemAmountInfo
{
    public ItemObject itemObj;
    public int amount;
    public ItemAmountInfo(ItemObject itemObj, int amount)
    {
        this.itemObj = itemObj;
        this.amount = amount;
    }
}
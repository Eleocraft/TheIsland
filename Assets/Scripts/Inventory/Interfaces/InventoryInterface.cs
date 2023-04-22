using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public abstract class InventoryInterface : MonoBehaviour
{
    protected Inventory inventory;

    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new();

    public event Action<Item, int> OnItemDropped;
    private bool active;
    protected InputMaster controls;
    protected virtual void Start()
    {
        AddEvent(gameObject, EventTriggerType.PointerEnter, (BaseEventData data) => { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, (BaseEventData data) => { OnExitInterface(gameObject); });

        controls = GlobalData.controls;
    }
    public void Initialize(Inventory inventory)
    {
        this.inventory = inventory;
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            inventory.Slots[i].parent = this;
            inventory.Slots[i].OnAfterUpdate += OnSlotUpdate;
        }
        CreateSlots();
    }
    public void ChangeInventory(Inventory newInventory)
    {
        if (inventory == null)
        {
            Initialize(newInventory);
            return;
        }
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            inventory.Slots[i].parent = null;
            inventory.Slots[i].OnAfterUpdate -= OnSlotUpdate;
        }
        inventory = newInventory;
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            inventory.Slots[i].parent = this;
            inventory.Slots[i].OnAfterUpdate += OnSlotUpdate;
        }
        UpdateSlotsToNewInventory();
        UpdateSlots();
    }
    protected virtual void OnEnable() => UpdateSlots();
    void OnDisable()
    {
        if (active)
        {
            TooltipPanelManager.Deactivate();
            Destroy(MouseData.tempItemBeingDragged);
        }
    }

    public abstract void CreateSlots();
    public abstract void UpdateSlotsToNewInventory();

    private void UpdateSlots()
    {
        for (int i = 0; i < inventory.Slots.Length; i++)
            OnSlotUpdate(inventory.Slots[i]);
    }
    public void OnSlotUpdate(InventorySlot slot)
    {
        if (!gameObject.activeSelf)
            return;
        
        Image itemImage = slot.slotDisplay.transform.GetChild(0).GetComponentInChildren<Image>();
        if (!slot.isEmpty())
        {
            itemImage.sprite = slot.item.ItemObject.Image;
            itemImage.color = Color.white;
            slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount == 1 ? "" : slot.amount.ToString("n0");
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color = new Color(1, 1, 1, 0);
            slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }
    protected void PopulateEvents(GameObject obj)
    {
        AddEvent(obj, EventTriggerType.PointerEnter, (BaseEventData data) => { OnEnter(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, (BaseEventData data) => { OnExit(obj); });
        AddEvent(obj, EventTriggerType.PointerClick, (BaseEventData data) => { OnClick(obj, data as PointerEventData); });
        AddEvent(obj, EventTriggerType.BeginDrag, (BaseEventData data) => { OnDragStart(obj, data as PointerEventData); });
        AddEvent(obj, EventTriggerType.EndDrag, (BaseEventData data) => { OnDragEnd(obj, data as PointerEventData); });
        AddEvent(obj, EventTriggerType.Drag, (BaseEventData data) => { OnDrag(obj); });
    }
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry eventTrigger = new() { eventID = type };
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    public void OnEnterInterface(GameObject obj)
    {
        active = true;
        MouseData.interfaceMouseIsOver = obj.GetComponent<InventoryInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        active = false;
        MouseData.interfaceMouseIsOver = null;
    }
    public void OnEnter(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged == null && !slotsOnInterface[obj].isEmpty())
            TooltipPanelManager.CreateTooltips(slotsOnInterface[obj].item.ItemObject.GetTooltips());
        MouseData.slotHoveredOver = obj;
    }
    public void OnExit(GameObject obj)
    {
        TooltipPanelManager.Deactivate();
        MouseData.slotHoveredOver = null;
    }
    public void OnClick(GameObject obj, PointerEventData eventData)
    {
        if (MouseData.tempItemBeingDragged != null || slotsOnInterface[obj].isEmpty())
            return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            bool used = slotsOnInterface[obj].item.ItemObject.Use();
            if (used)
            {
                slotsOnInterface[obj].RemoveAmount(1);
                if (slotsOnInterface[obj].amount <= 0)
                    TooltipPanelManager.Deactivate();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Inventory otherInv = PlayerInventory.Instance.GetOtherActiveInventory(inventory);

            if (!otherInv.AddItem(slotsOnInterface[obj].item, slotsOnInterface[obj].amount, out int droppedAmount))
                slotsOnInterface[obj].UpdateAmount(droppedAmount);
            else
                slotsOnInterface[obj].RemoveItem();
        }
    }
    public void OnDragStart(GameObject obj, PointerEventData eventData)
    {
        if (slotsOnInterface[obj].isEmpty())
            return;
        MouseData.tempItemBeingDragged = CreateTempItem(obj, eventData.button == PointerEventData.InputButton.Right);
        TooltipPanelManager.Deactivate();
    }
    public GameObject CreateTempItem(GameObject obj, bool halfAmount)
    {
        GameObject tempItem = Instantiate(obj.transform.GetChild(0).gameObject);
        tempItem.transform.SetParent(transform.parent);
        tempItem.GetComponent<Image>().raycastTarget = false;

        TMP_Text textField = tempItem.GetComponentInChildren<TextMeshProUGUI>();
        textField.raycastTarget = false;
        if (halfAmount)
            textField.text = ((slotsOnInterface[obj].amount / 2) <= 1) ? "" : ((slotsOnInterface[obj].amount / 2)).ToString("n0");
        return tempItem;
    }
    public void OnDragEnd(GameObject obj, PointerEventData eventData)
    {
        if (slotsOnInterface[obj].isEmpty())
            return;
        
        Destroy(MouseData.tempItemBeingDragged);
        if (MouseData.interfaceMouseIsOver == null)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnItemDropped(slotsOnInterface[obj].item, slotsOnInterface[obj].amount / 2);
                slotsOnInterface[obj].RemoveAmount(slotsOnInterface[obj].amount / 2);
            }
            else
            {
                OnItemDropped(slotsOnInterface[obj].item, slotsOnInterface[obj].amount);
                slotsOnInterface[obj].RemoveItem();
            }
            return;
        }
        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventory.TransferAmount(slotsOnInterface[obj], mouseHoverSlotData, slotsOnInterface[obj].amount / 2);
                return;
            }
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
            if (!mouseHoverSlotData.isEmpty())
                TooltipPanelManager.CreateTooltips(mouseHoverSlotData.item.ItemObject.GetTooltips());
        }
    }
    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
            MouseData.tempItemBeingDragged.transform.position = controls.Menus.MousePosition.ReadValue<Vector2>();
    }
    private static class MouseData
    {
        public static InventoryInterface interfaceMouseIsOver;
        public static GameObject tempItemBeingDragged;
        public static GameObject slotHoveredOver;
    }
}
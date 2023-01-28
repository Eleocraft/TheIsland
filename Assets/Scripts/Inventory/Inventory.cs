using System;
using UnityEngine;

public class Inventory
{   
    private int maxStack;
    public InventorySlot[] Slots;

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].isEmpty())
                    counter++;
            }
            return counter;
        }
    }

    public Inventory(int SlotCount, int maxStack)
    {
        Slots = new InventorySlot[SlotCount];
        this.maxStack = maxStack;
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = new InventorySlot(maxStack);
        }
    }
    public bool AddItemToStack(Item item, int amount, out int droppedAmount)
    {
        droppedAmount = amount;
        if (amount == 0)
            return true;
        if (CanStackItem(item, out int stackableAmount))
        {
            AddItem(item, Mathf.Min(stackableAmount, amount), out int zero);
            droppedAmount -= Mathf.Min(stackableAmount, amount);
            if (droppedAmount <= 0)
                return true;
        }
        return false;
    }

    public bool AddItem(Item item, int amount, out int droppedAmount)
    {
        droppedAmount = 0;
        if (amount == 0)
            return true;
        if (item.ItemObject.stackable)
        {
            while (amount > 0)
            {
                InventorySlot slot = FindItemOnInventory(item);
                if (slot == null)
                    break;
                
                int storeSpace = maxStack - slot.amount;
                if (storeSpace < amount)
                {
                    slot.AddAmount(storeSpace);
                    amount -= storeSpace;
                }
                else
                {
                    slot.AddAmount(amount);
                    return true;
                }
            }
        }
        return SetEmptySlot(item, amount, item.ItemObject.stackable, out droppedAmount);
    }

    public InventorySlot FindItemOnInventory(Item item)
    {
        for (int i = 0; i < Slots.Length; i++)
            if (Slots[i].item == item && Slots[i].amount < maxStack)
                return Slots[i];
        return null;
    }

    public bool SetEmptySlot(Item item, int amount, bool stackable, out int droppedAmount)
    {
        droppedAmount = 0;
        if (EmptySlotCount <= 0)
        {
            droppedAmount = amount;
            return false;
        }

        int itemMaxStack = maxStack;
        if (!stackable)
            itemMaxStack = 1;
        
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].isEmpty() && Slots[i].CanPlaceInSlot(item.ItemObject))
            {
                if (amount > itemMaxStack)
                {
                    Slots[i].UpdateSlot(item, itemMaxStack);
                    amount -= itemMaxStack;
                }
                else
                {
                    Slots[i].UpdateSlot(item, amount);
                    return true;
                }
            }
        }
        droppedAmount = amount;
        return false;
    }

    public void TransferAmount(InventorySlot originalSlot, InventorySlot newSlot, int amount = 0)
    {
        if (originalSlot.isEmpty() || !newSlot.CanPlaceInSlot(originalSlot.item.ItemObject))
            return;
        if (amount <= 0 || amount > originalSlot.amount)
            amount = originalSlot.amount;

        if (!newSlot.isEmpty())
        {
            if (originalSlot.item != newSlot.item)
                return;

            if (!originalSlot.item.ItemObject.stackable)
                return;
            int storeSpace = newSlot.maxStack - newSlot.amount;
            if (amount < storeSpace)
            {
                newSlot.AddAmount(amount);
                originalSlot.RemoveAmount(amount);
                return;
            }
            newSlot.AddAmount(storeSpace);
            originalSlot.RemoveAmount(storeSpace);
            return;
        }
        
        if (amount < newSlot.maxStack)
        {
            newSlot.UpdateSlot(originalSlot.item, amount);
            originalSlot.RemoveAmount(amount);
        }
    }

    public void SwapItems(InventorySlot itemSlot1, InventorySlot itemSlot2)
    {
        if (itemSlot1 == itemSlot2)
            return;
        if (itemSlot1.item == itemSlot2.item)
        {
            if (!itemSlot1.item.ItemObject.stackable)
                return;
            int storeSpace = itemSlot2.maxStack - itemSlot2.amount;
            if (itemSlot1.amount < storeSpace)
            {
                itemSlot2.AddAmount(itemSlot1.amount);
                itemSlot1.RemoveItem();
                return;
            }
            itemSlot2.AddAmount(storeSpace);
            itemSlot1.RemoveAmount(storeSpace);
            return;
        }
        if (itemSlot2.CanSwapWith(itemSlot1) && itemSlot1.CanSwapWith(itemSlot2))
        {
            Item tempItem = itemSlot2.item;
            int tempAmount = itemSlot2.amount;
            itemSlot2.UpdateSlot(itemSlot1.item, itemSlot1.amount);
            itemSlot1.UpdateSlot(tempItem, tempAmount);
        }
    }
    public bool RemoveItem(Item item, ref int amount)
    {
        foreach (InventorySlot slot in Slots)
        {
            if (slot.item == item)
            {
                amount = slot.RemoveAmount(amount);
                if (amount <= 0)
                    return true;
            }
        }
        return false;
    }
    public int GetItemCount(Item item)
    {
        int amount = 0;
        foreach (InventorySlot slot in Slots)
            if (slot.item == item)
                amount += slot.amount;
        return amount;
    }
    public bool CanStackItem(Item item, out int amount)
    {
        amount = 0;
        foreach (InventorySlot slot in Slots)
            if (slot.item == item)
                amount += maxStack - slot.amount;
        return amount != 0;
    }
    public InventorySlot[] GetSaveData()
    {
        return Slots;
    }
    public void Load(InventorySlot[] newInventory)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].UpdateSlot(newInventory[i].item, newInventory[i].amount);
        }
    }
    public void Clear()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].RemoveItem();
        }
    }
}
[Serializable]
public class InventorySlot
{
    [NonSerialized]
    public ItemType[] AllowedItems = new ItemType[0];
    [NonSerialized]
    public InventoryInterface parent;
    [NonSerialized]
    public GameObject slotDisplay;

    [NonSerialized]
    public Action<InventorySlot> OnAfterUpdate;
    [NonSerialized]
    public Action<InventorySlot> OnBeforeUpdate;

    [NonSerialized]
    public int maxStack;

    private Item _item;
    public Item item => _item;

    private int _amount;
    public int amount => _amount;

    public InventorySlot(int maxStack)
    {
        this.maxStack = maxStack;
        UpdateSlot(null, 0);
    }
    public bool isEmpty()
    {
        return item == null;
    }
    public void RemoveItem(int value = 0)
    {
        UpdateSlot(null, 0);
    }
    public void AddAmount(int value)
    {
        UpdateSlot(item, amount + value);
    }
    public int RemoveAmount(int value)
    {
        int amountBeforeUpdate = amount;
        if (amount - value > 0)
        {
            UpdateSlot(item, amount - value);
            return 0;
        }
        else
        {
            RemoveItem();
            return value - amountBeforeUpdate;
        }
    }
    public void UpdateAmount(int value)
    {
        UpdateSlot(item, value);
    }
    public void UpdateSlot(Item item, int amount)
    {
        OnBeforeUpdate?.Invoke(this);
        _item = item;
        _amount = amount;
        OnAfterUpdate?.Invoke(this);
    }
    public bool CanSwapWith(InventorySlot slot)
    {
        if (CanPlaceInSlot(slot.item?.ItemObject) && slot.amount <= maxStack)
            return true;
        return false;
    }
    public bool CanPlaceInSlot(ItemObject itemObject)
    {
        if (AllowedItems.Length <= 0 || itemObject == null)
            return true;
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (itemObject.Type == AllowedItems[i])
                return true;
        }
        return false;
    }
}
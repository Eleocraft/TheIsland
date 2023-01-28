using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryInterface : InventoryInterface
{
    public GameObject[] slots;

    public override void CreateSlots()
    {
        if (slots.Length != inventory.Slots.Length)
            throw new System.Exception("the slot count does not mach");
        
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            GameObject obj = slots[i];
            PopulateEvents(obj);
            inventory.Slots[i].slotDisplay = obj;
            FieldItemRestriction restriction = obj.GetComponent<FieldItemRestriction>();
            if (restriction != null)
                inventory.Slots[i].AllowedItems = restriction.AllowedItems;

            slotsOnInterface.Add(obj, inventory.Slots[i]);
        }
    }
    public override void UpdateSlotsToNewInventory()
    {
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            slotsOnInterface[slots[i]] = inventory.Slots[i];
            inventory.Slots[i].slotDisplay = slots[i];

            FieldItemRestriction restriction = slots[i].GetComponent<FieldItemRestriction>();
            if (restriction != null)
                inventory.Slots[i].AllowedItems = restriction.AllowedItems;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicInventoryInterface : InventoryInterface
{
    public GameObject inventoryField;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            GameObject obj = Instantiate(inventoryField, Vector3.zero, Quaternion.identity, transform);

            PopulateEvents(obj);
            inventory.Slots[i].slotDisplay = obj;

            slotsOnInterface.Add(obj, inventory.Slots[i]);
        }
    }
    public override void UpdateSlotsToNewInventory()
    {
        for (int i = 0; i < slotsOnInterface.Count; i++)
            Destroy(slotsOnInterface.ElementAt(i).Key);
            
        CreateSlots();
    }
}

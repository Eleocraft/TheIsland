using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicInventoryInterface : InventoryInterface
{
    public GameObject inventoryField;
    public int X_START;
    public int Y_START;
    public int X_SPACE_BETWEEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWEEN_ITEMS;

    public override void CreateSlots()
    {
        slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        for (int i = 0; i < inventory.Slots.Length; i++)
        {
            GameObject obj = Instantiate(inventoryField, Vector3.zero, Quaternion.identity, transform);
            obj.GetComponent<RectTransform>().localPosition = GetPosition(i);

            PopulateEvents(obj);
            inventory.Slots[i].slotDisplay = obj;

            slotsOnInterface.Add(obj, inventory.Slots[i]);
        }
    }
    public override void UpdateSlotsToNewInventory()
    {
        for (int i = 0; i < slotsOnInterface.Count; i++)
        {
            Destroy(slotsOnInterface.ElementAt(i).Key);
        }
        CreateSlots();
    }
    private Vector3 GetPosition(int i)
    {
        return new Vector3(X_START + (X_SPACE_BETWEEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWEEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0f);
    }
}

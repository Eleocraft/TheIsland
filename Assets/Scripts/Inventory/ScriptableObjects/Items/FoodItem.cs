using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Item", menuName = "CustomObjects/Inventory/Items/FoodItem")]
public class FoodItem : ItemObject
{
    public float restoreHealthValue;
    public override ItemType Type => ItemType.Food;
    public override bool Use()
    {
        Debug.Log("Eat" + restoreHealthValue);
        return true;
    }
    public override List<TooltipAttributeData> GetTooltips()
    {
        List<TooltipAttributeData> data = base.GetTooltips();
        data.Add(new TooltipAttributeData(TooltipAttributeType.HealValue, restoreHealthValue.ToString()));
        return data;
    }
}

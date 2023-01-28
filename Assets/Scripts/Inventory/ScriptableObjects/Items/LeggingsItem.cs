using UnityEngine;

[CreateAssetMenu(fileName = "New Leggings", menuName = "CustomObjects/Inventory/Items/LeggingsItem")]
public class LeggingsItem : ArmorItem
{
    public override ItemType Type => ItemType.Leggings;
}

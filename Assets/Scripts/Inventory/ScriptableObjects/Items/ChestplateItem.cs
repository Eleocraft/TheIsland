using UnityEngine;

[CreateAssetMenu(fileName = "New Chestplate", menuName = "CustomObjects/Inventory/Items/ChestplateItem")]
public class ChestplateItem : ArmorItem
{
    public override ItemType Type => ItemType.Chestplate;
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Boots", menuName = "CustomObjects/Inventory/Items/BootsItem")]
public class BootsItem : ArmorItem
{
    public override ItemType Type => ItemType.Boots;
}

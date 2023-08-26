using UnityEngine;

[CreateAssetMenu(fileName = "New Helmet", menuName = "CustomObjects/Inventory/Items/HelmetItem")]
public class HelmetItem : ArmorItem
{
    public override ItemType Type => ItemType.Helmet;
}

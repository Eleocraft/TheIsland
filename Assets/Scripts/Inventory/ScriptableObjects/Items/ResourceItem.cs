using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Item", menuName = "CustomObjects/Inventory/Items/ResourceItem")]
public class ResourceItem : ItemObject
{
    public override ItemType Type => ItemType.Resource;
}

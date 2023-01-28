using UnityEngine;

[CreateAssetMenu(fileName = "New Bow Item", menuName = "CustomObjects/Inventory/Items/BowItem")]
public class BowItem : ItemObject
{
    public override ItemType Type => ItemType.Bow;
    public GameObject bowPrefab;
}

using UnityEngine;

[CreateAssetMenu(fileName = "New Hammer Item", menuName = "CustomObjects/Inventory/Items/HammerItem")]
public class HammerItem : PortableItem
{
    public int HammerLevel;
    public float Speed = 1;
    public override ItemType Type => ItemType.Hammer;
}

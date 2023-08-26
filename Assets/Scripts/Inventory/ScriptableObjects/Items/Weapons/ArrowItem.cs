using UnityEngine;

[CreateAssetMenu(fileName = "New Arrow Item", menuName = "CustomObjects/Inventory/Items/ArrowItem")]
public class ArrowItem : ItemObject
{
    public override ItemType Type => ItemType.Arrow;
    public ProjectileInfo Projectile;
}

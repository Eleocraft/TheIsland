using UnityEngine;

[CreateAssetMenu(fileName = "New Bow Item", menuName = "CustomObjects/Inventory/Items/BowItem")]
public class BowItem : PortableItem
{
    public override ItemType Type => ItemType.Bow;
    public override PortableItemAnimation portableItemAnimation => PortableItemAnimation.Bow;
    public ArrowItem ArrowItem;
    public float TotalDrawTime;
    public float MinDrawTime;
    public float MaxVelocity;

    public float GetVelocity(float drawTime) => MaxVelocity * (drawTime / TotalDrawTime);
}

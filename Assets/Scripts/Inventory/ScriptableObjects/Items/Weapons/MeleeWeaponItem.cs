using UnityEngine;

public class MeleeWeaponItem : PortableItem
{
    public override ItemType Type => ItemType.MeleeWeapon;
    public override PortableItemAnimation portableItemAnimation => PortableItemAnimation.Spear;
    public float damage;
}

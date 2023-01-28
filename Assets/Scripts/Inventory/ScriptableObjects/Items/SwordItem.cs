using UnityEngine;

[CreateAssetMenu(fileName = "New Sword Item", menuName = "CustomObjects/Inventory/Items/SwordItem")]
public class SwordItem : MeleeWeaponItem
{
    public override ItemType Type => ItemType.Sword;
}

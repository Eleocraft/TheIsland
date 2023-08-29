using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hammer Item", menuName = "CustomObjects/Inventory/Items/HammerItem")]
public class HammerItem : PortableItem
{
    [Range(0, 5)] public int HammerLevel;
    public override ItemType Type => ItemType.Hammer;
    public override PortableItemAnimation portableItemAnimation => PortableItemAnimation.Tool;
    public override List<TooltipAttributeData> GetTooltips()
    {
        List<TooltipAttributeData> tooltips = base.GetTooltips();
        tooltips.Add(new HammerLevelTooltipAttributeData(HammerLevel, true));
        return tooltips;
    }
}

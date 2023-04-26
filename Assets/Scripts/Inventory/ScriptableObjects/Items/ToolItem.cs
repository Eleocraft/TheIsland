using System.Collections.Generic;
using UnityEngine;

public enum ToolType { Axe, Pickaxe }
[CreateAssetMenu(fileName = "New Tool Item", menuName = "CustomObjects/Inventory/Items/ToolItem")]
public class ToolItem : PortableItem
{
    public ToolType toolType;
    public float CritTime = 0.2f;
    public float Speed = 1;
    public override ItemType Type => ItemType.Tool;

    public override List<TooltipAttributeData> GetTooltips()
    {
        List<TooltipAttributeData> tooltips = base.GetTooltips();
        tooltips.Add(new TextTooltipAttributeData(TooltipAttributeType.HarvestingSpeed, Speed.ToString()));
        return tooltips;
    }
}

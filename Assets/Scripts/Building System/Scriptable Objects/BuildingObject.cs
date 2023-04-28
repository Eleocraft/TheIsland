using UnityEngine;
using System.Collections.Generic;

public enum BuildingType { Standalone, Foundation, Wall, Floor }
public abstract class BuildingObject : ScriptableObject
{
    public string Id;
    public abstract BuildingType buildingType { get; }
    public Sprite Image;
    public Building buildingPrefab;
    public Blueprint blueprintPrefab;
    [Range(0, 5)] public int BuildingLevel;
    public List<ItemAmountInfo> Cost;
    public LayerMask FreePlacingLayer;
    public bool allowRotation = true;

    [TextArea(10,5)]
    public string description;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(Id))    
            Id = Utility.CreateID(name);
    }
    public virtual bool FreePlacingPossible(RaycastHit hitData) => FreePlacingLayer == (FreePlacingLayer | 1 << hitData.collider.gameObject.layer);
    public virtual Vector3 GetNormal(RaycastHit hitData) => Vector3.up;
    public virtual List<TooltipAttributeData> GetTooltips()
    {
        PlayerInventory.TryGetActiveItem(out Item item);
        List<TooltipAttributeData> data = new()
        {
            new TextTooltipAttributeData(TooltipAttributeType.Title, name),
            new TextTooltipAttributeData(TooltipAttributeType.description, description),
            new HammerLevelTooltipAttributeData(BuildingLevel, ((HammerItem)item.ItemObject).HammerLevel >= BuildingLevel)
        };
        foreach (ItemAmountInfo info in Cost)
            data.Add(new MaterialTooltipAttributeData(info));
        return data;
    }
}

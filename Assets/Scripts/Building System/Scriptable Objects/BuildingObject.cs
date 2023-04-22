using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Default")]
public class BuildingObject : ScriptableObject
{
    public string Id;
    public Sprite Image;
    public GameObject buildingPrefab;
    public GameObject blueprintPrefab;
    [Range(0, 4)] public int BuildingLevel;

    [TextArea(10,5)]
    public string description;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(Id))    
            Id = Utility.CreateID(name);
    }
    public virtual List<TooltipAttributeData> GetTooltips()
    {
        List<TooltipAttributeData> data = new()
        {
            new TooltipAttributeData(TooltipAttributeType.Title, name),
            new TooltipAttributeData(TooltipAttributeType.description, description)
        };
        return data;
    }
}

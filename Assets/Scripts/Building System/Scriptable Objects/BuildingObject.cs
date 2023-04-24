using UnityEngine;
using System.Collections.Generic;

public enum BuildingType { Standalone, Foundation, Wall, Floor }
[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Default")]
public class BuildingObject : ScriptableObject
{
    public string Id;
    public BuildingType buildingType;
    public Sprite Image;
    public GameObject buildingPrefab;
    public Blueprint blueprintPrefab;
    [Range(0, 4)] public int BuildingLevel;
    public LayerMask FreePlacingLayer;
    public bool AjustToNormals;
    public bool UseMaxSteepness;
    public float MaxSteepnessRadiants = Mathf.PI*2;

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

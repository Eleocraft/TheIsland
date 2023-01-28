using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Harvesting Data", menuName = "CustomObjects/Terrain/HarvestingData")]
public class HarvestingData : ScriptableObject // Scriptable Object that holds all harvesting informations for terrain objects
{
    [SerializeField] private List<EfficiencyByTool> toolEfficiencies;
    private Dictionary<ItemObject, ToolEfficiencyData> ToolEfficiencies;

    public ToolEfficiencyData this[ItemObject index]
    {
        get => ToolEfficiencies[index];
    }
    void OnEnable()
    {
        ToolEfficiencies = new();
        foreach (EfficiencyByTool ef in toolEfficiencies)
            ToolEfficiencies.Add(ef.item, ef.efficiencyData);
    }
    void OnValidate()
    {
        foreach (EfficiencyByTool ef in toolEfficiencies)
        {
            ef.Name = (ef.item == null) ? "Tool" : ef.item.name;
            foreach (ResourceDropInfo dropInfo in ef.efficiencyData.droppedItems)
                dropInfo.name = dropInfo.resourceItem?.name;
        }
        ToolEfficiencies = new();
        foreach (EfficiencyByTool ef in toolEfficiencies)
            ToolEfficiencies.Add(ef.item, ef.efficiencyData);
    }

    [System.Serializable]
    class EfficiencyByTool
    {
        [HideInInspector] public string Name = "Tool";
        public ToolItem item;
        public ToolEfficiencyData efficiencyData;
    }
}
[System.Serializable]
public class ToolEfficiencyData
{
    public List<ResourceDropInfo> droppedItems;
    public float DamagePerHit;
}
[System.Serializable]
public class ResourceDropInfo
{
    [HideInInspector] public string name;
    public ResourceItem resourceItem;
    [SerializeField] [MinMaxRange(0f, 10f)] private RangeSlider dropAmount;
    public int Amount => Mathf.RoundToInt(dropAmount.RandomValue());
}
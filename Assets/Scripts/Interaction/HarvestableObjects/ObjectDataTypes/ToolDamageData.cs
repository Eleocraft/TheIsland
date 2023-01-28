using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Tool Damage Data", menuName = "CustomObjects/Terrain/ToolDamageData")]
public class ToolDamageData : ScriptableObject // Scriptable Object that holds the damage information for all tools
{
    [SerializeField] private List<EfficiencyByTool> toolDamages;
    private Dictionary<ItemObject, float> ToolDamages;

    public float this[ItemObject index]
    {
        get {
            return ToolDamages[index];
        }
    }
    void OnEnable()
    {
        ToolDamages = new();
        foreach (EfficiencyByTool ef in toolDamages)
            ToolDamages.Add(ef.item, ef.damage);
    }
    void OnValidate()
    {
        foreach (EfficiencyByTool ef in toolDamages)
            ef.Name = (ef.item == null) ? "Tool" : ef.item.name;
        
        ToolDamages = new();
        foreach (EfficiencyByTool ef in toolDamages)
            ToolDamages.Add(ef.item, ef.damage);
    }

    [System.Serializable]
    class EfficiencyByTool
    {
        [HideInInspector] public string Name = "Tool";
        public ToolItem item;
        public float damage;
    }
}
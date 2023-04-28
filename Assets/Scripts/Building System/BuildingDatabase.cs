using UnityEngine;
using System.Collections.Generic;

public class BuildingDatabase : MonoSingleton<BuildingDatabase>
{
    [SerializeField] private List<BuildingObject> EditorBuildingObjects;
    public static Dictionary<string, BuildingObject> BuildingObjects { get; private set; }
    [SerializeField] private EnumDictionary<BuildingType, LayerMask> SnappingTypeLayerMasks;

    public static LayerMask BuildingTypeToSnappingLayer(BuildingType type) => Instance.SnappingTypeLayerMasks[type];
    protected override void SingletonAwake()
    {
#if UNITY_EDITOR
        GetItems();
#endif

        BuildingObjects = new();
        for (int i = 0; i < EditorBuildingObjects.Count; i++)
        {
            if (EditorBuildingObjects[i] == null)
            {
                EditorBuildingObjects.RemoveAt(i);
                i--;
                continue;
            }
            BuildingObjects.Add(EditorBuildingObjects[i].Id, EditorBuildingObjects[i]);
        }
    }
    public void OnValidate()
    {
        SnappingTypeLayerMasks.Update();
        for (int i = 0; i < EditorBuildingObjects.Count; i++)
            if (EditorBuildingObjects[i] == null)
                EditorBuildingObjects.RemoveAt(i);
    }
#if UNITY_EDITOR
    [ContextMenu("Update")]
    public void GetItems()
    {
        EditorBuildingObjects ??= new();

        foreach (BuildingObject i in Utility.FindAssetsByType<BuildingObject>())
            if (!EditorBuildingObjects.Contains(i))
                EditorBuildingObjects.Add(i);
        
        OnValidate();
    }
#endif
}

using UnityEngine;
using System.Collections.Generic;

public class BuildingDatabase : MonoSingleton<BuildingDatabase>
{
    [SerializeField] private List<BuildingObject> EditorBuildingObjects;
    public static Dictionary<string, BuildingObject> BuildingObjects { get; private set; }
    // [SerializeField] private EnumDictionary<BuildingType, LayerMask> BuildingTypeLayerMasks;

    // public static LayerMask BuildingTypeToLayer(BuildingType type) => Instance.BuildingTypeLayerMasks[type];

    public void Awake()
    {
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

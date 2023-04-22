using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoSingleton<ItemDatabase>
{
    [SerializeField] private List<ItemObject> EditorItemObjects;
    public static Dictionary<string, ItemObject> ItemObjects { get; private set; }

    public void Awake()
    {
        ItemObjects = new();
        for (int i = 0; i < EditorItemObjects.Count; i++)
        {
            if (EditorItemObjects[i] == null)
            {
                EditorItemObjects.RemoveAt(i);
                i--;
                continue;
            }
            ItemObjects.Add(EditorItemObjects[i].Id, EditorItemObjects[i]);
        }
    }
    public void OnValidate()
    {
        for (int i = 0; i < EditorItemObjects.Count; i++)
            if (EditorItemObjects[i] == null)
                EditorItemObjects.RemoveAt(i);
    }
#if UNITY_EDITOR
    [ContextMenu("Update")]
    public void GetItems()
    {
        EditorItemObjects ??= new();

        foreach (ItemObject i in Utility.FindAssetsByType<ItemObject>())
            if (!EditorItemObjects.Contains(i))
                EditorItemObjects.Add(i);
                
        OnValidate();
    }
#endif
}
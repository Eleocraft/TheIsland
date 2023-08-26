using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

public enum ItemType
{
    Resource,
    Food,
    Helmet,
    Chestplate,
    Leggings,
    Boots,
    Tool,
    Hammer,
    Bow,
    Arrow,
    MeleeWeapon
}
public abstract class ItemObject : ScriptableObject
{
    public string Id;
    public Sprite Image;
    public GameObject GroundPrefab;
    public bool stackable;
    public bool manualPickUp;
    [HideInInspector] public abstract ItemType Type { get; }
    [TextArea(10,5)]
    public string description;

    void OnValidate()
    {
        if (string.IsNullOrEmpty(Id))    
            Id = Utility.CreateID(name);
    }

    public Item CreateItem() => new(this);

    public virtual bool Use() { return false; }
    public virtual List<TooltipAttributeData> GetTooltips()
    {
        List<TooltipAttributeData> data = new()
        {
            new TextTooltipAttributeData(TooltipAttributeType.Title, name),
            new TextTooltipAttributeData(TooltipAttributeType.description, description)
        };
        return data;
    }
    public static Type GetItemObjectType(ItemType itemType)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ItemObject)))
                .Select(type => Activator.CreateInstance(type) as ItemObject)
                .Where(itemObject => itemObject.Type == itemType).Single().GetType();
    }
}
[Serializable]
public class Item
{
    [HideInInspector] public string Id;
    [NonSerialized] private ItemObject _itemObject;

    public ItemObject ItemObject
    {
        get {
            if (_itemObject == null)
                _itemObject = ItemDatabase.ItemObjects[Id];
            return _itemObject;
        }
    }

    public Item(ItemObject itemObj)
    {
        Id = itemObj.Id;
        _itemObject = itemObj;
    }
    // operator
    public static bool operator == (Item firstItem, Item secondItem)
    {
        return firstItem?.Id == secondItem?.Id;
    }
    public static bool operator != (Item firstItem, Item secondItem)
    {
        return firstItem?.Id != secondItem?.Id;
    }
    // To make the compiler happy
    public override bool Equals(object o)
    {
        return base.Equals(o);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
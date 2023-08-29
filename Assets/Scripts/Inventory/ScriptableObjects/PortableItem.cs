using UnityEngine;

public abstract class PortableItem : ItemObject
{
    public enum PortableItemAnimation { Tool, Bow, Spear }
    public abstract PortableItemAnimation portableItemAnimation { get; }
    public GameObject portableItemPrefab;
}

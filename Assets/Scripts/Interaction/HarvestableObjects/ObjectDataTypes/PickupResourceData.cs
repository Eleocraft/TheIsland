using UnityEngine;

[CreateAssetMenu(fileName = "New Object Data", menuName = "CustomObjects/Terrain/Recources/SimpleRecource")]
public class PickupResourceData : ObjectData
{
    public ItemObject Item;
    public string recourceName;
    public int Amount;
}

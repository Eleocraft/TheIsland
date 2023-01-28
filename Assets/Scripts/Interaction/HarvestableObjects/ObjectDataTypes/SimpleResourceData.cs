using UnityEngine;

[CreateAssetMenu(fileName = "New Object Data", menuName = "CustomObjects/Terrain/Recources/SimpleRecource")]
public class SimpleResourceData : ObjectData
{
    public HarvestingData harvestingInfo;
    public string recourceName;
    public float Life;
}

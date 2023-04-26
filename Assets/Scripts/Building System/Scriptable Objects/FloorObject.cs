using UnityEngine;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Floor")]
public class FloorObject : BuildingObject
{
    public override BuildingType buildingType => BuildingType.Floor;
}

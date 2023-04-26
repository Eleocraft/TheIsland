using UnityEngine;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Wall")]
public class WallObject : BuildingObject
{
    public override BuildingType buildingType => BuildingType.Wall;
}

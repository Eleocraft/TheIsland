using UnityEngine;

public class SimpleBuildingSnappingPoint : BuildingSnappingPoint
{
    [SerializeField] private BuildingType type;

    public override bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle)
    {
        position = transform.position;
        yangle = transform.eulerAngles.y;

        return buildingType == type && SnappingPointActive;
    }
}

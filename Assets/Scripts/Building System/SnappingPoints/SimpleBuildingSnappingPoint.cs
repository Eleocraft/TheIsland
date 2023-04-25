using UnityEngine;

public class SimpleBuildingSnappingPoint : BuildingSnappingPoint
{
    [SerializeField] private BuildingType type;
    [SerializeField] private bool allowRotation = true;

    protected override bool IncludesType(BuildingType buildingType) => type == buildingType;
    public override bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle, out bool allowRotation)
    {
        position = transform.position;
        yangle = transform.parent.eulerAngles.y;
        allowRotation = this.allowRotation;

        return buildingType == type && SnappingPointActive;
    }
}

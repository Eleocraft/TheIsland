using UnityEngine;
using System.Linq;

public class GroupBuildingSnappingPoint : BuildingSnappingPoint
{
    [SerializeField] private SerializableDictionary<BuildingType, Transform> snappingPoints;

    void Awake()
    {
        snappingPoints.Update();
    }
    public override bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle)
    {
        position = Vector3.zero;
        yangle = transform.parent.eulerAngles.y;

        if (!snappingPoints.Keys.Contains(buildingType))
            return false;
        
        position = snappingPoints[buildingType].position;
        return SnappingPointActive;
    }
}
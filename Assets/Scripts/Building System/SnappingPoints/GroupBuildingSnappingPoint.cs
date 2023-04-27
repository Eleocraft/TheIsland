using UnityEngine;
using System.Linq;

public class GroupBuildingSnappingPoint : BuildingSnappingPoint
{
    [SerializeField] private SerializableDictionary<BuildingType, GroupBuildingSnappingPointInfo> snappingPoints;

    void Awake()
    {
        snappingPoints.Update();
    }
    public override bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle, out bool allowRotation)
    {
        position = Vector3.zero;
        yangle = transform.parent.eulerAngles.y;
        allowRotation = false;

        if (!snappingPoints.Keys.Contains(buildingType))
            return false;
        
        position = snappingPoints[buildingType].transform.position;
        allowRotation = snappingPoints[buildingType].allowRotation;
        return SnappingPointActive;
    }
    [System.Serializable]
    private struct GroupBuildingSnappingPointInfo
    {
        [SerializeField]
        public Transform transform;
        [SerializeField]
        public bool allowRotation;
    }
}
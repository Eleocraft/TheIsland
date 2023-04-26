using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BuildingSnappingPoint : MonoBehaviour
{
    const float floatTolerance = 0.001f;
    protected bool SnappingPointActive => connectedBuilding == null;
    private Building connectedBuilding;
    [SerializeField] private int stabilityReduction = 1;
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out Building building) && TryGetSnappingInfo(building.BuildingObject.buildingType, out Vector3 position, out float yangle, out bool allowRotation)
            && (position - col.transform.position).sqrMagnitude < floatTolerance)
            connectedBuilding = building;
    }
    public int GetStability()
    {
        if (!connectedBuilding)
            return 0;
        
        int connectedBuildingStability = connectedBuilding.GetStability();
        return connectedBuildingStability - stabilityReduction;
    }
    public abstract bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle, out bool allowRotation);
}
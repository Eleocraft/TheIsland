using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BuildingSnappingPoint : MonoBehaviour
{
    protected bool SnappingPointActive => connectedBuilding == null;
    private Building connectedBuilding;
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out Building building) && IncludesType(building.Type))
            connectedBuilding = building;
    }
    protected abstract bool IncludesType(BuildingType buildingType);
    public abstract bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle, out bool allowRotation);
}
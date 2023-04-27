using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class BuildingSnappingPoint : MonoBehaviour
{
    const float floatTolerance = 0.001f;
    protected bool SnappingPointActive => connectedBuilding == null;
    private GridBuilding connectedBuilding;
    [SerializeField] private int stabilityReduction = 1;
    private int connectedBuildingStability = -1;
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out GridBuilding building) && TryGetSnappingInfo(building.BuildingObject.buildingType, out Vector3 position, out float yangle, out bool allowRotation)
            && (position - col.transform.position).sqrMagnitude < floatTolerance)
            connectedBuilding = building;
    }
    public int GetStability(bool recalculateStability)
    {
        if (!connectedBuilding)
            return 0;
        
        if (connectedBuildingStability == -1 && !connectedBuilding.checkingForStability)
            connectedBuildingStability = connectedBuilding.GetStability(recalculateStability);
        
        return connectedBuildingStability - stabilityReduction;
    }
    void Update()
    {
        if (connectedBuildingStability != -1)
            connectedBuildingStability = -1;
    }
    public void RecalculateConnectedBuildingStability()
    {
        if (!connectedBuilding)
            return;

        connectedBuilding.CalculateStability(false);
    }
    public void RecalculateConnectedBuildingStabilityAfterPhysicsUpdate()
    {
        if (!connectedBuilding)
            return;

        connectedBuilding.RecalculateCalculateStabilityAfterPhysicsUpdate();
    }
    public abstract bool TryGetSnappingInfo(BuildingType buildingType, out Vector3 position, out float yangle, out bool allowRotation);
}
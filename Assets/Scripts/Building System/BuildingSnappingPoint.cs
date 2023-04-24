using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuildingSnappingPoint : MonoBehaviour
{
    public BuildingType Type;
    public float YAngle => transform.parent.eulerAngles.y;
    public bool SnappingPointActive => connectedBuilding == null;
    private Building connectedBuilding;
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out Building building) && building.Type == Type)
            connectedBuilding = building;
    }
}

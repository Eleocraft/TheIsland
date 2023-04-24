using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuildingSnappingPoint : MonoBehaviour
{
    public BuildingType Type;
    public Vector3 upAxis => transform.parent.up;
    public float[] allowedAngles => new float[]
    {
        transform.parent.eulerAngles.y,
        (transform.parent.eulerAngles.y + 90f).ClampToAngle(),
        (transform.parent.eulerAngles.y + 180f).ClampToAngle(),
        (transform.parent.eulerAngles.y + 270f).ClampToAngle()
    };
    public bool SnappingPointActive => connectedBuilding == null;
    private Building connectedBuilding;
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent(out Building building) && building.Type == Type)
            connectedBuilding = building;
    }
}

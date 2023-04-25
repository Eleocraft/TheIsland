using UnityEngine;

public class FoundationBuilding : Building
{
    [SerializeField] private GameObject BottomSnappingPoint;
    [SerializeField] private GameObject FoundationBase;
    [SerializeField] private LayerMask CheckLayer;
    [SerializeField] private float maxDistance = 4;
    const int GroundLayer = 7;

    public override void Initialize()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, maxDistance, CheckLayer) && hitInfo.collider.gameObject.layer == GroundLayer)
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
        }
        else
            stability = 99; // Temp
        base.Initialize();
    }
    public override void Load(int Id)
    {
        base.Load(Id);
        if (stability >= 100)
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
        }
    }

}

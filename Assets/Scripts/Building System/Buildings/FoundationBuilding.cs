using UnityEngine;

public class FoundationBuilding : GridBuilding
{
    [SerializeField] private GameObject BottomSnappingPoint;
    [SerializeField] private GameObject FoundationBase;
    [SerializeField] private LayerMask CheckLayer;
    [SerializeField] private float maxDistance = 4;
    const int GroundLayer = 7;
    private bool connected;

    public override void Initialize()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hitInfo, maxDistance, CheckLayer)&& hitInfo.collider.gameObject.layer == GroundLayer)
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
            connected = true;
        }
        base.Initialize();
    }
    public override int GetStability()
    {
        return connected ? 100 : base.GetStability();
    }
    public override void Load(int Id)
    {
        base.Load(Id);
        if (buildings[Id].stability >= 100)
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
            connected = true;
        }
    }

}

using UnityEngine;
using System.Collections.Generic;

public class FoundationBuilding : GridBuilding
{
    [SerializeField] private GameObject BottomSnappingPoint;
    [SerializeField] private GameObject FoundationBase;
    [SerializeField] private LayerMask CheckLayer;
    [SerializeField] private float maxDistance = 4;
    [SerializeField] private Transform raycastOrigin;
    const int GroundLayer = 7;
    private bool connected;
    [ResetOnDestroy]
    private static List<int> groundConnections = new();
    [Save(SaveType.world)]
    public static object FoundationBuildingSaveData
    {
        get => groundConnections;
        set => groundConnections = (List<int>)value;
    }

    public override void Initialize()
    {
        base.Initialize();
        if (Physics.Raycast(raycastOrigin.position, Vector3.down, out RaycastHit hitInfo, maxDistance, CheckLayer) && hitInfo.collider.gameObject.layer == GroundLayer)
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
            connected = true;
            groundConnections.Add(Id);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        groundConnections.Remove(Id);
    }
    public override int GetStability(bool recalculateStability = false)
    {
        return connected ? 100 : base.GetStability(recalculateStability);
    }
    public override void Load(int Id)
    {
        base.Load(Id);
        StartCoroutine(Utility.WaitForFrames(1, checkGroundConnections));
    }
    private void checkGroundConnections()
    {
        if (groundConnections.Contains(Id))
        {
            BottomSnappingPoint.SetActive(false);
            Instantiate(FoundationBase, transform.position, transform.rotation, transform);
            connected = true;
        }
    }
}

using UnityEngine;

public class StandaloneGridBuilding : Building
{
    [SerializeField] private float maxDistance = 4;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask CheckLayer;
    void Start()
    {
        if (Physics.Raycast(raycastOrigin.position, transform.up * -1f, out RaycastHit hitInfo, maxDistance, CheckLayer) &&
            hitInfo.collider.TryGetComponent(out GridBuilding building))
        {
            building.AddStandaloneBuilding(this);
        }
    }
}

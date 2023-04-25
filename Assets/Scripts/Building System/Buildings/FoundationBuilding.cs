using UnityEngine;

public class FoundationBuilding : Building
{
    [SerializeField] private GameObject BottomSnappingPoint;

    void Start()
    {
        
    }
    void OnValidate()
    {
        Type = BuildingType.Foundation;
    }
}

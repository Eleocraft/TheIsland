using UnityEngine;

public class GridBuilding : Building
{
    [SerializeField] private BuildingSnappingPoint[] snappingPoints;
    private bool checkingForStability;
    public override int GetStability()
    {
        if (checkingForStability)
            return 0;
        int stability = 0;
        checkingForStability = true;
        foreach (BuildingSnappingPoint point in snappingPoints)
        {
            int pointStability = point.GetStability();
            if (stability < pointStability)
                stability = pointStability;
        }
        checkingForStability = false;
        Debug.Log(Id + "  " + stability);
        if (stability <= 0)
        {
            Destroy(gameObject);
            return 0;
        }
        return stability;
    }
}

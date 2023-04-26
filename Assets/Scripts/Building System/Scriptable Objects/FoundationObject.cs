using UnityEngine;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Foundation")]
public class FoundationObject : BuildingObject
{
    public override BuildingType buildingType => BuildingType.Foundation;
    public bool UseMaxSteepness;
    public float MaxSteepnessRadiants = Mathf.PI*2;
    public override bool FreePlacingPossible(RaycastHit hitData)
    {
        return base.FreePlacingPossible(hitData) && (!UseMaxSteepness || Utility.CalcualteSteepnessRadientFromNormal(hitData.normal) < MaxSteepnessRadiants);
    }
}

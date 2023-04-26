using UnityEngine;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Standalone")]
public class StandaloneObject : BuildingObject
{
    public override BuildingType buildingType => BuildingType.Standalone;
    public bool AjustToNormals;
    public bool UseMaxSteepness;
    public float MaxSteepnessRadiants = Mathf.PI*2;

    public override bool FreePlacingPossible(RaycastHit hitData)
    {
        return base.FreePlacingPossible(hitData) && (!UseMaxSteepness || Utility.CalcualteSteepnessRadientFromNormal(hitData.normal) < MaxSteepnessRadiants);
    }
    public override Vector3 GetNormal(RaycastHit hitData)
    {
        return base.FreePlacingPossible(hitData) && AjustToNormals ? hitData.normal : Vector3.up;
    }
}

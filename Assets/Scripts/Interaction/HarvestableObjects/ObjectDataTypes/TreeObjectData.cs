using UnityEngine;

[CreateAssetMenu(fileName = "New Object Data", menuName = "CustomObjects/Terrain/Recources/Tree")]
public class TreeObjectData : ObjectData
{
    [Header("--Tree Information")]
    public string treeName;
    public GameObject trunkPrefab;
    public GameObject stumpPrefab;
    [Header("--Harvesting Properties")]
    public float Life;
    public ToolDamageData toolDamageInfo;
    public TrunkObjectData trunkData;
}
[System.Serializable]
public class TrunkObjectData
{
    public HarvestingData harvestingInfo;
    public string recourceName = "Trunk";
    public float Life = 4;
}

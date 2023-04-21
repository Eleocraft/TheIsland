using UnityEngine;

[CreateAssetMenu(fileName = "new Building Object", menuName = "CustomObjects/Buildings/Default")]
public class BuildingObject : ScriptableObject
{
    public GameObject buildingPrefab;
    public GameObject blueprintPrefab;
    [Range(0, 4)] public int BuildingLevel;
}

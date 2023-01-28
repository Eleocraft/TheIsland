using UnityEngine;

[CreateAssetMenu(fileName = "New Object Spawn Data", menuName = "CustomObjects/Terrain/Object Spawn Data")]
public class ObjectSpawnData : ScriptableObject // Scriptable Object that holds all generation informations for terrain objects
{
    [Range(0, 2)]
    public float GeneraionSteepnessRadiant = 0.5f;
    [Range(ObjectPlacer.minRadius, ObjectPlacer.maxRadius)]
    public float radius = 10f;
    public float frequency = 10f;
    public float minHeight = 40f;
    public float maxHeight = float.MaxValue;
    public bool rotateWithTerrain;
}

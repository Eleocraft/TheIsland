using UnityEngine;

[CreateAssetMenu(fileName = "New Global Object", menuName = "CustomObjects/Terrain/Global Object")]
public class SpawnableGlobalObject : ScriptableObject
{
    public GameObject prefab;
    [Range(0, 2)]
    public float GeneraionSteepnessRadiant = 0.5f;
    public int minChunkDistance = 1;
    public float ObjectBlockRadius = 10f;
    public float frequency = 10f;
    public float minHeight = 40f;
    public float flatRadius = 0f;
}

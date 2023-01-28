using UnityEngine;

[CreateAssetMenu(fileName = "New Spawnable Object", menuName = "CustomObjects/Terrain/Spawnable Object")]
public class SpawnableObject : ScriptableObject // Scriptable Object that holds a combination of spawn data and object data
{
    public ObjectData objectData;
    public ObjectSpawnData objectSpawnData;
}
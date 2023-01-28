using UnityEngine;

public class ChunklessMapObject : MonoBehaviour
{
    private void Start()
    {
        MapObject.CreateChunklessObject(GetComponent<MapObject>());
    }
}

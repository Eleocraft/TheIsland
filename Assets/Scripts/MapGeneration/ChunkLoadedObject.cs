using UnityEngine;
using System.Collections.Generic;

public class ChunkLoadedObject : MonoBehaviour
{
    private static List<ChunkLoadedObject> chunkLoadedObjects = new();
    void Start()
    {
        chunkLoadedObjects.Add(this);
    }
    void OnDestroy()
    {
        chunkLoadedObjects.Remove(this);
    }
    void UpdateVisibility(List<Bounds> visibleChunkBounds, int chunkSize)
    {
        foreach(Bounds bound in visibleChunkBounds)
        {
            if (bound.Contains(transform.position.XZ()))
            {
                gameObject.SetActive(true);
                return;
            }
        }
        gameObject.SetActive(false);
    }
    public static void UpdateVisibleChunks(List<Bounds> visibleChunkBounds, int chunkSize)
    {
        foreach(ChunkLoadedObject obj in chunkLoadedObjects)
            obj.UpdateVisibility(visibleChunkBounds, chunkSize);
    }
}

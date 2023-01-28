using UnityEngine;

public class StaticObjectResetter : MonoBehaviour
{
    void OnDestroy()
    {
        MapObject.ResetMapObjectLists();
    }
}

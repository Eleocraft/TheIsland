using UnityEngine;

public class StaticObjectResetter : MonoBehaviour
{
    void OnDestroy()
    {
        MapObject.ResetMapObjectLists();
        Building.ResetBuildingLists();
        Chest.ResetChestLists();
    }
}

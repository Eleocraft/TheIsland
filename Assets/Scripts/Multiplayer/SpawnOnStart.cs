using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class SpawnOnStart : MonoBehaviour
{
    void Start()
    {
        NetworkObject obj = GetComponent<NetworkObject>();
        if (obj.IsOwner)
            obj.Spawn(true);
    }
}

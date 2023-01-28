using UnityEngine;
using Unity.Netcode;

public class PlayerModelMover : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) return;
    }
    void Update()
    {
        if (!IsOwner) return;

        transform.position = PlayerData.Position;
        transform.rotation = PlayerData.Roatation;
    }
}

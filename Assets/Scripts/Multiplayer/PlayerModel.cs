using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour
{
    public static PlayerModel LocalPlayer;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        LocalPlayer = this;
    }
    void Update()
    {
        if (!IsOwner) return;

        transform.position = PlayerData.Position;
        transform.rotation = PlayerData.Roatation;
    }
}
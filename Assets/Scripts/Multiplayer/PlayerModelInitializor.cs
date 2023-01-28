using UnityEngine;
using Unity.Netcode;

public class PlayerModelInitializor : NetworkBehaviour
{
    [SerializeField] NetworkObject playerModelPrefab;
    private void Awake()
    {
        SpawnPlayermodelServerRpc();
    }

    [ServerRpc]
    private void SpawnPlayermodelServerRpc()
    {
        NetworkObject networkObject = Instantiate(playerModelPrefab);
        networkObject.SpawnAsPlayerObject(OwnerClientId);
    }
}

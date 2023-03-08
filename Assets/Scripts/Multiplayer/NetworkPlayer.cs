using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner)
            return;
        
        transform.GetChild(0).gameObject.SetActive(false);
        PlayerAnimationManager.SetPlayerNetworkAnimator(GetComponent<Animator>());
    }
    private void Update()
    {
        if (!IsOwner)
            return;
        
        transform.position = PlayerData.Position;
        transform.rotation = PlayerData.Roatation;
    }
}
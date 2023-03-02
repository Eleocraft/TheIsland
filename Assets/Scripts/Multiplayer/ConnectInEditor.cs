using UnityEngine;
using Unity.Netcode;

public class ConnectInEditor : MonoBehaviour
{
    public enum ConnectionMode { Host, Client }
    [SerializeField] private ConnectionMode connectionMode;
    private void Awake()
    {
        if (connectionMode == ConnectionMode.Host)
            GetComponent<NetworkManager>().StartHost();
        else
            GetComponent<NetworkManager>().StartClient();
    }
}

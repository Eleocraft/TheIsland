using UnityEngine;
using System;

public class CarryPlayer : MonoBehaviour
{
    private GameObject player;
    private bool active;
    private Rigidbody rb;

    private GameObject playerDummy;
    [SerializeField] private GameObject shipDummy;
    private Rigidbody playerDummyRb;
    public bool CarryingPlayer => active;
    public Action<bool> ChangeActivation;

    private static bool Carrying;

    void Start()
    {
        player = PlayerData.GameObject;
        rb = GetComponent<Rigidbody>();
        GetComponentInChildren<CarryTrigger>().PlayerEntered += StartCarrying;
        GetComponentInChildren<CarryTrigger>().PlayerExited += StopCarrying;
        shipDummy = Instantiate(shipDummy, Vector3.zero, Quaternion.identity);
        shipDummy.SetActive(false);
    }
    void OnDestroy()
    {
        Destroy(shipDummy);
    }
    void StartCarrying()
    {
        if (Carrying)
            return;
        Carrying = true;

        active = true;
        ChangeActivation?.Invoke(true);
        playerDummy = PlayerMovement.ActivateDummyObject();
        playerDummyRb = playerDummy.GetComponent<Rigidbody>();
        playerDummy.transform.position = transform.InverseTransformPoint(player.transform.position);
        playerDummy.transform.rotation = Quaternion.Euler(0, player.transform.eulerAngles.y - transform.eulerAngles.y, 0);
        CameraControl.SetYRotation(Camera.main.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
        shipDummy.SetActive(true);
        Update();
    }
    void StopCarrying()
    {
        if (!Carrying)
            return;
        Carrying = false;

        active = false;
        ChangeActivation?.Invoke(false);
        PlayerMovement.DeactivateDummyObject(playerDummy);
        CameraControl.SetYRotation(Camera.main.transform.rotation.eulerAngles.y);
        CameraControl.SetYRotOffset(0);
        shipDummy.SetActive(false);
    }
    void Update()
    {
        if (active)
        {
            player.transform.position = transform.position + (rb.rotation * playerDummy.transform.position);
            player.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + playerDummyRb.rotation.eulerAngles.y, 0);
            CameraControl.SetYRotOffset(transform.rotation.eulerAngles.y);
        }
    }
}

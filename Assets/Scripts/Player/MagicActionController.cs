using UnityEngine;

public class MagicActionController : MonoBehaviour
{
    void Start()
    {
        GlobalData.controls.Player.CastMagic.performed += Cast;
    }

    void Update()
    {
        GlobalData.controls.Player.CastMagic.performed -= Cast;
    }
    private void Cast(UnityEngine.InputSystem.InputAction.CallbackContext ctx = default)
    {
        Debug.Log("cast");
    }
}

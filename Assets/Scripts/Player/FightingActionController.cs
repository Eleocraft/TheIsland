using UnityEngine;

public class FightingActionController : MonoBehaviour
{
    [SerializeField] private ProjectileInfo Info;
    [SerializeField] private Transform CameraTransform;
    private void Start()
    {
        GlobalData.controls.Player.CastMagic.performed += Shoot;
    }
    private void OnDestroy()
    {
        GlobalData.controls.Player.CastMagic.performed -= Shoot;
    }
    private void Shoot(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Projectile.SpawnProjectile(Info, CameraTransform.position, CameraTransform.rotation * Vector3.forward);
    }
}

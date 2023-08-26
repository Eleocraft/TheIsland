using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 _spawnPosition;
    private Vector3 _lastPosition;
    private Vector3 _velocity;
    private ProjectileInfo _info;
    private float _sqrMaxDistance;
    private GameObject _displayObject;

    public static void SpawnProjectile(ProjectileInfo info, Vector3 position, Vector3 velocity)
    {
        GameObject projectileObj = Instantiate(PrefabHolder.Prefabs[PrefabTypes.Projectile], position, Quaternion.identity);
        projectileObj.GetComponent<Projectile>().Initialize(info, velocity);
    }
    private void Initialize(ProjectileInfo info, Vector3 velocity)
    {
        // Graphics
        _displayObject = Instantiate(info.Prefab, transform.position, Quaternion.identity);
        // Physics
        _velocity = velocity;
        _lastPosition = transform.position;
        _spawnPosition = transform.position;
        _info = info;
        _sqrMaxDistance = info.MaxDistance * info.MaxDistance;
    }
    private void Update()
    {
        Vector3 movement = _velocity * Time.deltaTime;
        transform.position += movement;
        _displayObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, _velocity);
        
        if (Physics.Raycast(_lastPosition, movement, out RaycastHit hitData, movement.magnitude, ProjectileHitLayer.CanHit))
        {
            if (hitData.collider.TryGetComponent(out IDamagable damagable))
                damagable.OnHit(_info, hitData.point, hitData.normal);

            _displayObject.transform.position = hitData.point;
            Destroy(gameObject);
        }
        else
            _displayObject.transform.position = transform.position;
        
        // Lifetime
        if ((transform.position - _spawnPosition).sqrMagnitude > _sqrMaxDistance)
            Destroy(gameObject);
        
        // Physics
        _velocity -= _velocity * _info.Drag * Time.deltaTime; // Drag
        _velocity += Vector3.down * _info.Dropoff * Time.deltaTime; // Gravity
        
        _lastPosition = transform.position;
    }
}

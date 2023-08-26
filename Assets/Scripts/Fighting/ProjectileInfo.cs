using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "CustomObjects/Weapons/Projectile")]
public class ProjectileInfo : ScriptableObject
{
    [Header("Damage")]
    public float Damage;
    [Header("Physics")]
    public float Drag;
    public float Dropoff;
    [Header("DespawnChecks")]
    public float MaxDistance;
    [Header("Graphics")]
    public GameObject Prefab;
}
using System;
using UnityEngine;


public enum DamageType { Default, Headshot }

[CreateAssetMenu(fileName = "New Projectile", menuName = "CustomObjects/Weapons/Projectile")]
public class ProjectileInfo : ScriptableObject
{
    [Header("Damage")]
    public EnumDictionary<DamageType, float> Damages;
    [Header("Physics")]
    public float InitialVelocity;
    public float Drag;
    public float Dropoff;
    [Header("DespawnChecks")]
    public float MaxDistance;
    [Header("Graphics")]
    public GameObject Prefab;
    private void OnValidate()
    {
        Damages.Update();
    }
    public float GetDamage(DamageType damageType, float projectileVelocity)
    {
        return Damages[damageType] * (projectileVelocity / InitialVelocity);
    }
}
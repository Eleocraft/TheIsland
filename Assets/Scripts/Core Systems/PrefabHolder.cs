using UnityEngine;


public enum PrefabTypes { Projectile, TreeDestructionParticles, TrunkDestructionParticles, TreeHitParticles }
public class PrefabHolder : MonoSingleton<PrefabHolder>
{
    [SerializeField] private EnumDictionary<PrefabTypes, GameObject> prefabs = new();
    public static EnumDictionary<PrefabTypes, GameObject> Prefabs => Instance.prefabs;
    private void OnValidate()
    {
        prefabs.Update();
    }
    private void Start()
    {
        prefabs.Update();
    }
}
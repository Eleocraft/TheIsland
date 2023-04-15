using UnityEngine;

public class ObjectHolder : MonoSingleton<ObjectHolder>
{
    [Header("--General Objects")]
    [SerializeField] private ParticleSystem treeDestructionParticles;
    public static ParticleSystem TreeDestructionParticles => Instance.treeDestructionParticles;
    [SerializeField] private ParticleSystem trunkDestructionParticles;
    public static ParticleSystem TrunkDestructionParticles => Instance.trunkDestructionParticles;

    [Header("--Savable Object Prefabs")]
    [SerializeField] private GameObject[] savablePrefabs;
    public static GameObject[] SavablePrefabs => Instance.savablePrefabs;
}

using UnityEngine;

public class TrunkResource : MonoBehaviour, IInteractable, IHarvestable
{
    const float spawnForce = 50f;
    const float trunkHeight = 30f;
    const float totalMovingTime = 15f;
    const float itemSpawnForce = 4f;
    private TrunkObjectData objectData;
    private Rigidbody RB;
    private float Life;
    private float MovingTime;

    public string InteractionInfo => objectData.recourceName;
    public HarvestableType HarvestableType => HarvestableType.Trunk;
    public void OnInitialisation(TrunkObjectData objectData, Vector3 direction)
    {
        this.objectData = objectData;
        Life = objectData.Life;
        MovingTime = totalMovingTime;
        RB = gameObject.GetComponent<Rigidbody>();
        RB.AddForceAtPosition(direction * spawnForce, transform.position + Vector3.up * trunkHeight, ForceMode.Impulse);
        // Spawning Particle System
        ParticleSystem destructionParticleSystem = Instantiate(PrefabHolder.Prefabs[PrefabTypes.TreeDestructionParticles], transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = destructionParticleSystem.shape;
        shape.mesh = GetComponent<MeshFilter>().mesh;
    }
    public void OnHit(ToolItem itemInfo, Vector3 position, Vector3 direction)
    {
        Instantiate(PrefabHolder.Prefabs[PrefabTypes.TreeHitParticles], position, Quaternion.identity);

        ToolEfficiencyData data = objectData.harvestingInfo[itemInfo];
        foreach (ResourceDropInfo itemAmount in data.droppedItems)
            GroundItem.Create(itemAmount.resourceItem.CreateItem(), position, Vector3.up * itemSpawnForce + direction * -itemSpawnForce, itemAmount.Amount, false);
        
        Life -= data.DamagePerHit;
        if (Life <= 0)
        {
            ParticleSystem destructionParticleSystem = Instantiate(PrefabHolder.Prefabs[PrefabTypes.TrunkDestructionParticles], transform.position, transform.rotation).GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = destructionParticleSystem.shape;
            shape.mesh = GetComponent<MeshFilter>().mesh;
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (MovingTime > 0)
        {
            MovingTime -= Time.deltaTime;
            return;
        }
        if (RB.isKinematic)
            return;
        if (RB.velocity == Vector3.zero)
            RB.isKinematic = true;
    }
}

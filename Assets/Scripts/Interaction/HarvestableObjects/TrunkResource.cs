using UnityEngine;

public class TrunkResource : MonoBehaviour, IInteractable, IHarvestable
{
    const float spawnForce = 1000f;
    const float trunkHeight = 30f;
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
        MovingTime = objectData.MovingTime;
        RB = gameObject.GetComponent<Rigidbody>();
        RB.AddForceAtPosition(direction * spawnForce, transform.position + Vector3.up * trunkHeight, ForceMode.Impulse);
        Debug.DrawLine(transform.position + Vector3.up * trunkHeight, transform.position + Vector3.up * trunkHeight + direction * 10, Color.red, 10);
        ParticleSystem destructionParticleSystem = Instantiate(ObjectHolder.TreeDestructionParticles, transform.position, Quaternion.identity);
        ParticleSystem.ShapeModule shape = destructionParticleSystem.shape;
        shape.mesh = GetComponent<MeshFilter>().mesh;
    }
    public void OnHit(ToolItem itemInfo, Vector3 direction)
    {
        ToolEfficiencyData data = objectData.harvestingInfo[itemInfo];
        foreach (ResourceDropInfo itemAmount in data.droppedItems)
            PlayerInventory.AddItem(itemAmount.resourceItem.CreateItem(), itemAmount.Amount);
        
        Life -= data.DamagePerHit;
        if (Life <= 0)
            Destroy(this.gameObject);
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

using UnityEngine;

public class SimpleResource : MapObject, IInteractable, IHarvestable
{
    public SimpleResourceData objectData;
    private float Life;
    public string InteractionInfo => objectData.recourceName;
    public override float GetBaseLife => objectData.Life;
    public HarvestableType HarvestableType => HarvestableType.Ore;
    private void Start()
    {
        Life = objectData.Life;
    }
    public override void UpdateLocalState(float life)
    {
        Life = life;
        if (Life <= 0)
            Destroy(gameObject);
    }
    public void OnHit(ToolItem itemInfo, Vector3 position, Vector3 direction)
    {
        ToolEfficiencyData data = objectData.harvestingInfo[itemInfo];
        foreach (ResourceDropInfo itemAmount in data.droppedItems)
            PlayerInventory.AddItem(new(itemAmount.resourceItem, itemAmount.Amount));
        Life -= data.DamagePerHit;
        UpdateState(Id, Life);
        if (Life <= 0)
            Destroy(gameObject);
    }
}

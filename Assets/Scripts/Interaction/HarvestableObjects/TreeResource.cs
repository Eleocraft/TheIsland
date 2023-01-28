using UnityEngine;

public class TreeResource : MapObject, IInteractable, IHarvestable
{
    public TreeObjectData objectData;
    private float Life;
    public string InteractionInfo => objectData.treeName;
    public override float GetBaseLife => objectData.Life;
    public HarvestableType HarvestableType => HarvestableType.Tree;
    private void Awake()
    {
        Life = objectData.Life;
    }
    public override void UpdateLocalState(float life)
    {
        Life = life;
        if (Life <= 0)
        {
            // Instantiate stump
            Instantiate(objectData.stumpPrefab, transform.position, Quaternion.identity, transform.parent);
            Destroy(gameObject);
        }
    }
    public void OnHit(ToolItem itemInfo, Vector3 direction)
    {
        Life -= objectData.toolDamageInfo[itemInfo];
        UpdateState(Id, Life);
        if (Life <= 0)
        {
            // Instantiate trunk
            GameObject trunk = Instantiate(objectData.trunkPrefab, transform.position, Quaternion.identity, transform.parent);
            trunk.GetComponent<TrunkResource>().OnInitialisation(objectData.trunkData, direction);
            // Instantiate stump
            Instantiate(objectData.stumpPrefab, transform.position, Quaternion.identity, transform.parent);
            
            Destroy(gameObject);
        }
    }
}

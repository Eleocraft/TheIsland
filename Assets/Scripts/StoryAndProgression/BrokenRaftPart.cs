using UnityEngine;

public class BrokenRaftPart : MonoBehaviour, IInteractable
{
    public string PartName;
    public bool Repaired => _repaired;
    private bool _repaired;

    [SerializeField] private ItemAmountInfo RequiredItem;
    [SerializeField] private Material bluePrintMaterial;
    private Material[] materials;
    private MeshRenderer meshRenderer;
    
    public string InteractionInfo => _repaired ? controller.InteractionInfo : $"{RequiredItem.amount} {RequiredItem.itemObj.name} needed to repair {PartName}";

    private BrokenRaft controller;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        materials = meshRenderer.materials;
        gameObject.SetActive(false);
    }

    public void Activate(BrokenRaft controller)
    {
        gameObject.SetActive(true);
        this.controller = controller;
        // Creating BluePrintMaterial Array
        Material[] bluePrintMaterials = new Material[materials.Length];
        for (int i = 0; i < materials.Length; i++)
            bluePrintMaterials[i] = bluePrintMaterial;
        meshRenderer.materials = bluePrintMaterials;
    }

    public void Interact()
    {
        if (Repaired)
            controller.Interact();
        else if (PlayerInventory.UseItems(new(){ RequiredItem }))
            Repair();
    }
    public void Repair()
    {
        _repaired = true;
        meshRenderer.materials = materials;
        controller?.AddPart();
    }
}

public class PickupResource : MapObject, IInteractable
{
    public PickupResourceData objectData;
    public string InteractionInfo => objectData.recourceName + $"\nPress {GlobalData.controls.Interaction.MainInteraction.bindings[0].ToDisplayString()} to pick up {objectData.Item.name}";
    public override float GetBaseLife => 1f;
    public override void UpdateLocalState(float life)
    {
        if (life == 0)
            Destroy(gameObject);
    }
    public void Interact()
    {
        PlayerInventory.AddItem(new (objectData.Item, objectData.Amount));
        UpdateState(Id, 0f);
        Destroy(gameObject);
    }
}

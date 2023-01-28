using UnityEngine;

public class GiveItem : MonoBehaviour, IInteractable
{
    public string InteractionInfo => $"Gather {item.itemObj.name}";
    [SerializeField] private ItemAmountInfo item;

    public void Interact()
    {
        PlayerInventory.AddItem(item);
        Destroy(gameObject);
    }
}

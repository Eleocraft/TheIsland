using UnityEngine;

public class GiveItem : MonoBehaviour, IInteractable
{
    public string InteractionInfo => $"Gather {item.itemObj.name}";
    [SerializeField] private DisplayItemAmountInfo item;

    public void Interact()
    {
        PlayerInventory.AddItem(item);
        Destroy(gameObject);
    }
}

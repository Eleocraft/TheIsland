using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private Inventory chestInv;
    private InventoryInterface chestInterface;
    private Animator animator;
    private bool open;
    [SerializeField] private int slotCount;

    public string InteractionInfo => "chest";
    private void Start()
    {
        chestInv = new Inventory(slotCount, PlayerInventory.MaxStack);
        chestInterface = PlayerInventory.ChestInterface;
        animator = GetComponent<Animator>();
    }

    public void Interact() //Open
    {
        if (open)
            return;
        InputStateMachine.ChangeInputState(false, this);
        PlayerInventory.ToggleInventory(Close);
        chestInterface.ChangeInventory(chestInv);
        chestInterface.gameObject.SetActive(true);
        animator.Play("Open");
        open = true;
    }
    public void Close()
    {
        if (!open)
            return;
        InputStateMachine.ChangeInputState(true, this);
        chestInterface.gameObject.SetActive(false);
        animator.Play("Close");
        open = false;
    }
}

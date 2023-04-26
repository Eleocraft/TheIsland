using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Building))]
public class Chest : MonoBehaviour, IInteractable
{
    private Inventory chestInv;
    private InventoryInterface chestInterface;
    private Animator animator;
    private bool open;
    [SerializeField] private int slotCount;
    public string InteractionInfo => "chest";

    [ResetOnDestroy]
    private static Dictionary<int, Inventory> chests = new();

    [Save(SaveType.world)]
    public static object ChestsSaveData
    {
        get => chests;
        set => chests = (Dictionary<int, Inventory>)value;
    }
    
    private void Start()
    {
        int Id = GetComponent<Building>().Id;
        if (!chests.ContainsKey(Id))
        {
            chestInv = new Inventory(slotCount, PlayerInventory.MaxStack);
            chests.Add(Id, chestInv);
        }
        else
        {
            chestInv = chests[Id];
            chestInv.Load();
        }
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

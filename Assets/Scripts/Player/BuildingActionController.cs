using UnityEngine;

public class BuildingActionController : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;
    InputMaster controls;
    private void Start()
    {
        controls = GlobalData.controls;
        controls.MouseFP.SecondaryAction.started += OpenBuildMenu;
        controls.MouseFP.MainAction.started += PlaceBuilding;
        PlayerInventory.HotbarSlotChange += AbortAction;
    }
    void OnDestroy()
    {
        controls.MouseFP.SecondaryAction.started -= OpenBuildMenu;
        controls.MouseFP.MainAction.started -= PlaceBuilding;
        PlayerInventory.HotbarSlotChange -= AbortAction;
    }

    private void OpenBuildMenu(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!PlayerInventory.TryGetActiveItem(out Item equippedItem) || equippedItem.ItemObject.Type != ItemType.Hammer)
            return;

        if (CursorStateMachine.AlreadyLocked(this))
            return;

        EscQueue.Enqueue(CloseBuildMenu);
        CursorStateMachine.ChangeCursorState(false, this);
        
        buildingMenu.SetActive(true);
    }
    public void CloseBuildMenu()
    {
        if (!buildingMenu.activeSelf)
            return;
        
        EscQueue.Remove(CloseBuildMenu);
        CursorStateMachine.ChangeCursorState(true, this);
        buildingMenu.SetActive(false);
    }
    private void AbortAction()
    {

    }
    private void PlaceBuilding(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {

    }
}

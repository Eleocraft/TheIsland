using UnityEngine;

public class BuildingActionController : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float HitRange;
    [SerializeField] private LayerMask InteractableLayers;
    InputMaster controls;
    bool buildMode;
    GameObject activeBlueprint;
    private void Start()
    {
        controls = GlobalData.controls;
        controls.MouseFP.SecondaryAction.started += OpenBuildMenu;
        controls.MouseFP.MainAction.started += MainAction;
        PlayerInventory.HotbarSlotChange += EndBuildMode;
    }
    void OnDestroy()
    {
        controls.MouseFP.SecondaryAction.started -= OpenBuildMenu;
        controls.MouseFP.MainAction.started -= MainAction;
        PlayerInventory.HotbarSlotChange -= EndBuildMode;
    }

    private void OpenBuildMenu(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!PlayerInventory.TryGetActiveItem(out Item equippedItem) || equippedItem.ItemObject.Type != ItemType.Hammer)
            return;

        if (CursorStateMachine.AlreadyLocked(this))
            return;

        if (buildMode)
            EndBuildMode();

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
    public void StartBuildMode(BuildingObject buildingObject)
    {
        CloseBuildMenu();
        buildMode = true;
        activeBlueprint = Instantiate(buildingObject.blueprintPrefab);
    }
    private void EndBuildMode()
    {
        buildMode = false;
        Destroy(activeBlueprint);
    }
    private void MainAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (buildMode)
        {
            buildMode = false;
            activeBlueprint = null;
        }
        else
        {
            Debug.Log("build");
        }
    }
    private void FixedUpdate()
    {
        if (!buildMode)
            return;
        
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, InteractableLayers))
            activeBlueprint.transform.position = hitData.point;
        else
            activeBlueprint.transform.position = CameraTransform.position + CameraTransform.TransformDirection(Vector3.forward) * HitRange;
    }
}

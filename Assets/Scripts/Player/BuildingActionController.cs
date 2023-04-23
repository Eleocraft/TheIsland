using UnityEngine;

public class BuildingActionController : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float HitRange;
    [SerializeField] private LayerMask InteractableLayers;
    InputMaster controls;
    ActiveBuildingBlueprint activeBlueprint = new();
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

        if (activeBlueprint.BuildMode)
        {
            EndBuildMode();
            return;
        }

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
        activeBlueprint.ActivateBlueprint(Instantiate(buildingObject.blueprintPrefab), buildingObject);
    }
    private void EndBuildMode()
    {
        activeBlueprint.EndBlueprint();
    }
    private void MainAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (activeBlueprint.BuildMode)
        {
            Instantiate(activeBlueprint.Object.buildingPrefab, activeBlueprint.BuildingPos, Quaternion.identity);
        }
        else
        {
            Debug.Log("WIP Build");
        }
    }
    private void FixedUpdate()
    {
        if (!activeBlueprint.BuildMode)
            return;
        
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, InteractableLayers))
            activeBlueprint.SetPosition(hitData.point);
        else
            activeBlueprint.SetPosition(CameraTransform.position + CameraTransform.TransformDirection(Vector3.forward) * HitRange);
    }
    private class ActiveBuildingBlueprint
    {
        private GameObject Blueprint;
        public BuildingObject Object;
        public bool BuildMode => Blueprint != null;
        public Vector3 BuildingPos => Blueprint.transform.position;
        public void ActivateBlueprint(GameObject Blueprint, BuildingObject Object)
        {
            this.Blueprint = Blueprint;
            this.Object = Object;
        }
        public void EndBlueprint() => Destroy(Blueprint);
        public void SetPosition(Vector3 Pos) => Blueprint.transform.position = Pos;
    }
}

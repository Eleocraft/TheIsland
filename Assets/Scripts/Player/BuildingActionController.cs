using UnityEngine;

public class BuildingActionController : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float HitRange;
    [SerializeField] private LayerMask FreePlaceLayers;
    [SerializeField] private LayerMask BuildingSortingLayers;
    [SerializeField] private Color BlockedColor;
    [SerializeField] private Color BuildableColor;

    private LayerMask snappingPointLayer;
    ActiveBuildingBlueprint activeBlueprint;
    float angleOffset;
    private void Start()
    {
        activeBlueprint = new(this);
        
        ItemActionController itemActionController = GetComponent<ItemActionController>();

        itemActionController.AddAction(ItemActionController.ActionType.MainActionStarted, ItemType.Hammer, MainAction);
        itemActionController.AddAction(ItemActionController.ActionType.SecondaryActionStarted, ItemType.Hammer, OpenBuildMenu);
        itemActionController.AddAction(ItemActionController.ActionType.TertiaryActionStarted, ItemType.Hammer, Delete);

        GlobalData.controls.Interaction.SecondaryInteraction.performed += Rotate;
        PlayerInventory.HotbarSlotChange += EndBuildMode;
    }
    void OnDestroy()
    {
        GlobalData.controls.Interaction.SecondaryInteraction.performed -= Rotate;
        PlayerInventory.HotbarSlotChange -= EndBuildMode;
    }

    private void OpenBuildMenu(Item equippedItem)
    {
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
        snappingPointLayer = BuildingDatabase.BuildingTypeToSnappingLayer(buildingObject.buildingType);
    }
    private void EndBuildMode()
    {
        if (activeBlueprint.BuildMode)
            activeBlueprint.EndBlueprint();
    }
    private void MainAction(Item equippedItem)
    {
        if (activeBlueprint.BuildMode)
        {
            PlayerInventory.TryGetActiveItem(out Item item);
            if (activeBlueprint.PlacingPossible && PlayerInventory.UseItems(activeBlueprint.Object.Cost) && ((HammerItem)item.ItemObject).HammerLevel >= activeBlueprint.Object.BuildingLevel)
                Instantiate(activeBlueprint.Object.buildingPrefab, activeBlueprint.BuildingPos, activeBlueprint.BuildingRot).Initialize();
        }
        else
        {
            //Debug.Log("WIP Build");
        }
    }
    private void Delete(Item equippedItem)
    {
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, BuildingSortingLayers))
            Destroy(hitData.collider.gameObject);
    }
    private void Rotate(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        angleOffset = (angleOffset + 90f).ClampToAngle();
    }
    private void Update()
    {
        if (!activeBlueprint.BuildMode)
            return;
        // Snapping
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit snapHitData, HitRange, snappingPointLayer) &&
            snapHitData.collider.TryGetComponent(out BuildingSnappingPoint snappingPoint) && 
            snappingPoint.TryGetSnappingInfo(activeBlueprint.Object.buildingType, out Vector3 position, out float yangle))
        {
            activeBlueprint.PlacingPossible = !activeBlueprint.Blueprint.Blocked;
            activeBlueprint.SetPosition(position);
            activeBlueprint.SetRotation(Vector3.up, activeBlueprint.Object.allowRotation ? GetAngle(yangle) : yangle);
        }
        //FreePlacing
        else if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, FreePlaceLayers))
        {
            activeBlueprint.PlacingPossible = activeBlueprint.Object.FreePlacingPossible(hitData)
                && !activeBlueprint.Blueprint.Blocked;
            
            Vector3 buildingNormal = activeBlueprint.Object.GetNormal(hitData);
            
            activeBlueprint.SetPosition(hitData.point);
            activeBlueprint.SetRotation(buildingNormal, (transform.rotation.eulerAngles.y + angleOffset).ClampToAngle());
        }
        else
        {
            activeBlueprint.PlacingPossible = false;
            activeBlueprint.SetPosition(CameraTransform.position + CameraTransform.TransformDirection(Vector3.forward) * HitRange);
            activeBlueprint.SetRotation(Vector3.up, transform.rotation.eulerAngles.y);
        }
        float GetAngle(float YAngle)
        {
            float smallestDiff = float.MaxValue;
            int smallestDiffIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                float diff = Mathf.Abs(AngleFromIndex(i) - transform.eulerAngles.y);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    smallestDiffIndex = i;
                }
            }
                
            return (AngleFromIndex(smallestDiffIndex) + angleOffset).ClampToAngle();

            float AngleFromIndex(int i) => (YAngle + i * 90f).ClampToAngle();
        }
    }
    private class ActiveBuildingBlueprint
    {
        private readonly BuildingActionController BAC;
        public Blueprint Blueprint { get; private set; }
        private Rigidbody blueprintRB;
        private MeshRenderer blueprintRenderer;
        public BuildingObject Object { get; private set; }
        private bool newBuilding = false;
        private bool placingPossible;
        public bool PlacingPossible
        {
            get => placingPossible;
            set
            {
                if (placingPossible != value || newBuilding)
                {
                    SetColor(value ? BAC.BuildableColor : BAC.BlockedColor);
                    newBuilding = false;
                }
                placingPossible = value;
            }
        }
        public bool BuildMode => blueprintRB != null;
        public Vector3 BuildingPos => blueprintRB.transform.position;
        public Quaternion BuildingRot => blueprintRB.transform.rotation;
        public ActiveBuildingBlueprint(BuildingActionController BAC)
        {
            this.BAC = BAC;
        }
        public void ActivateBlueprint(Blueprint Blueprint, BuildingObject Object)
        {
            this.Blueprint = Blueprint;
            blueprintRB = Blueprint.GetComponent<Rigidbody>();
            blueprintRenderer = Blueprint.GetComponent<MeshRenderer>();
            this.Object = Object;
            newBuilding = true;
        }
        public void EndBlueprint() => Destroy(blueprintRB.gameObject);
        public void SetPosition(Vector3 Pos) => blueprintRB.MovePosition(Pos);
        public void SetRotation(Vector3 axis, float angle) => blueprintRB.MoveRotation(Quaternion.FromToRotation(Vector3.up, axis) * Quaternion.Euler(0, angle, 0));
        public void SetRotation(Quaternion rotation) => blueprintRB.MoveRotation(rotation);
        private void SetColor(Color color)
        {
            foreach (Material mat in blueprintRenderer.materials)
                mat.color = color;
        }
    }
}

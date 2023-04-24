using UnityEngine;
using System.Linq;

public class BuildingActionController : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float HitRange;
    [SerializeField] private LayerMask FreePlaceLayers;
    [SerializeField] private LayerMask SnappingPointLayer;
    [SerializeField] private LayerMask BuildingSortingLayers;
    [SerializeField] private Color BlockedColor;
    [SerializeField] private Color BuildableColor;
    InputMaster controls;
    ActiveBuildingBlueprint activeBlueprint;
    private void Start()
    {
        activeBlueprint = new(this);

        controls = GlobalData.controls;
        controls.MouseFP.SecondaryAction.started += OpenBuildMenu;
        controls.MouseFP.MainAction.started += MainAction;
        controls.MouseFP.TertiaryAction.started += Delete;
        PlayerInventory.HotbarSlotChange += EndBuildMode;
    }
    void OnDestroy()
    {
        controls.MouseFP.SecondaryAction.started -= OpenBuildMenu;
        controls.MouseFP.MainAction.started -= MainAction;
        controls.MouseFP.TertiaryAction.started -= Delete;
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
        if (activeBlueprint.BuildMode)
            activeBlueprint.EndBlueprint();
    }
    private void MainAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (activeBlueprint.BuildMode)
        {
            if (activeBlueprint.PlacingPossible)
                Instantiate(activeBlueprint.Object.buildingPrefab, activeBlueprint.BuildingPos, activeBlueprint.BuildingRot);
        }
        else if (PlayerInventory.TryGetActiveItem(out Item equippedItem) && equippedItem.ItemObject.Type == ItemType.Hammer)
        {
            Debug.Log("WIP Build");
        }
    }
    private void Delete(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (PlayerInventory.TryGetActiveItem(out Item equippedItem) && equippedItem.ItemObject.Type == ItemType.Hammer
            && Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, BuildingSortingLayers))
        {
            Destroy(hitData.collider.gameObject);
        }
    }
    private void Update()
    {
        if (!activeBlueprint.BuildMode)
            return;
        // Snapping
        if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit snapHitData, HitRange, SnappingPointLayer) &&
            snapHitData.collider.TryGetComponent(out BuildingSnappingPoint snappingPoint) && 
            snappingPoint.SnappingPointActive && snappingPoint.Type == activeBlueprint.Object.buildingType)
        {
            activeBlueprint.PlacingPossible = !activeBlueprint.Blueprint.Blocked;
            activeBlueprint.SetPosition(snapHitData.collider.transform.position);
            activeBlueprint.SetRotation(snappingPoint.upAxis, GetSmallestAngleDifference(snappingPoint.allowedAngles));
        }
        //FreePlacing
        else if (Physics.Raycast(CameraTransform.position, CameraTransform.TransformDirection(Vector3.forward), out RaycastHit hitData, HitRange, FreePlaceLayers))
        {
            activeBlueprint.PlacingPossible = 
                activeBlueprint.Object.FreePlacingLayer == (activeBlueprint.Object.FreePlacingLayer | 1 << hitData.collider.gameObject.layer)
                && (!activeBlueprint.Object.UseMaxSteepness || Utility.CalcualteSteepnessRadientFromNormal(hitData.normal) < activeBlueprint.Object.MaxSteepnessRadiants)
                && !activeBlueprint.Blueprint.Blocked;
            
            Vector3 buildingNormal = Vector3.up;
            if (activeBlueprint.Object.AjustToNormals && activeBlueprint.Object.FreePlacingLayer == (activeBlueprint.Object.FreePlacingLayer | 1 << hitData.collider.gameObject.layer))
                buildingNormal = hitData.normal;
            
            activeBlueprint.SetPosition(hitData.point);
            activeBlueprint.SetRotation(buildingNormal, transform.rotation.eulerAngles.y);
        }
        else
        {
            activeBlueprint.PlacingPossible = false;
            activeBlueprint.SetPosition(CameraTransform.position + CameraTransform.TransformDirection(Vector3.forward) * HitRange);
            activeBlueprint.SetRotation(Vector3.up, transform.rotation.eulerAngles.y);
        }
        float GetSmallestAngleDifference(float[] angles)
        {
            float smallestDiff = float.MaxValue;
            int smallestDiffIndex = 0;
            for (int i = 0; i < angles.Length; i++)
            {
                float diff = Mathf.Abs(angles[i] - transform.eulerAngles.y);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    smallestDiffIndex = i;
                }
            }
                
            return angles[smallestDiffIndex];
        }
    }
    private class ActiveBuildingBlueprint
    {
        private readonly BuildingActionController BAC;
        public Blueprint Blueprint { get; private set; }
        private Rigidbody blueprintRB;
        private MeshRenderer blueprintRenderer;
        public BuildingObject Object { get; private set; }
        private bool placingPossible;
        public bool PlacingPossible
        {
            get => placingPossible;
            set
            {
                if (placingPossible != value)
                    SetColor(value ? BAC.BuildableColor : BAC.BlockedColor);
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
            SetColor(BAC.BlockedColor);
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

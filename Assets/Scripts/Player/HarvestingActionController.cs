using UnityEngine;

public enum HarvestableType { None, Tree, Trunk, Ore }
public class HarvestingActionController : MonoSingleton<HarvestingActionController>
{
    [Header("--Visuals")]
    [SerializeField] private GameObject Particles;
    [SerializeField] private Animator armAnimator;
    [SerializeField] private AnimationEventHandler armAnimationEventHandler;

    private HarvestableType harvestableType;
    private InputMaster controls;
    
    [SerializeField] [ReadOnly] private bool hitting;
    [SerializeField] [ReadOnly] private bool locked;
    private float critTimer;
    void Start()
    {
        controls = GlobalData.controls;
        controls.MouseFP.MainAction.started += StartAction;
        // If Hotbarslot is changed the current action should be stopped
        PlayerInventory.HotbarSlotChange += AbortAction;
        armAnimationEventHandler.Events["HitObject"] += HitObject;
        armAnimationEventHandler.Events["CritChance"] += CritChance;
    }
    void OnDestroy()
    {
        controls.MouseFP.MainAction.started -= StartAction;
        PlayerInventory.HotbarSlotChange -= AbortAction;
        armAnimationEventHandler.Events["HitObject"] -= HitObject;
        armAnimationEventHandler.Events["CritChance"] -= CritChance;
    }
    void StartAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!PlayerInventory.TryGetActiveItem(out Item equippedItem) || equippedItem.ItemObject.Type != ItemType.Tool)
            return;

        if (InteractionController.TryGetInteraction(out InteractionData interaction) && interaction.interactableObject.TryGetComponent(out IHarvestable harvestableObject))
        {
            // check if an animation is already playing
            if (hitting)
            {
                if (!locked && critTimer > 0)
                {
                    armAnimator.SetTrigger("CritHit");
                    critTimer = 0;
                }
                else
                    locked = true;
                return;
            }
            // no animation playing
            harvestableType = harvestableObject.HarvestableType;
            armAnimator.Play("Hit");
        }
        else
            armAnimator.Play("NoHit");
        
        armAnimator.speed = ((ToolItem)equippedItem.ItemObject).Speed;
        hitting = true;
        locked = false;
        critTimer = 0;
    }
    void Update()
    {
        if (critTimer > 0)
            critTimer -= Time.deltaTime;
    }
    void CritChance()
    {
        if (PlayerInventory.TryGetActiveItemGO(out GameObject GO))
        {
            float critTime = ((ToolItem)PlayerInventory.activeItem.ItemObject).CritTime;
            GO.GetComponentInChildren<ToolCriticalHitIndicator>().CritChance(critTime);
            critTimer = critTime;
        }
    }
    void AbortAction()
    {
        if (hitting)
        {
            hitting = false;
            if (armAnimator.isActiveAndEnabled)
                armAnimator.CrossFade("DefaultPos", 3f);
            armAnimator.speed = 1;
        }
    }
    public static void EndPlaying()
    {
        Instance.hitting = false;
        Instance.armAnimator.speed = 1;
    }
    public void HitObject()
    {
        if (PlayerInventory.TryGetActiveItem(out Item equippedItem) && InteractionController.TryGetInteraction(out InteractionData interaction) && interaction.interactableObject.TryGetComponent(out IHarvestable harvestableObject))
        {
            harvestableObject.OnHit(equippedItem.ItemObject as ToolItem, (interaction.hitData.point - transform.position).XZ().AddHeight(0).normalized);
            CameraShakeController.Singleton.Shake(1.5f, 0.4f);
            Instantiate(Particles, interaction.hitData.point, Quaternion.identity);
        }
    }
}
public interface IHarvestable
{
    void OnHit(ToolItem information, Vector3 direction);
    public HarvestableType HarvestableType { get; }
}
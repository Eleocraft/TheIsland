using UnityEngine;

public enum HarvestableType { None, Tree, Trunk, Ore }
public class HarvestingActionController : MonoBehaviour
{
    [Header("--Visuals")]
    [SerializeField] private Animator armAnimator;
    [SerializeField] private AnimationEventHandler armAnimationEventHandler;
    
    private bool hitting;
    private bool locked;
    private float critTimer;
    private bool startingCrit;
    void Start()
    {
        GetComponent<ItemActionController>().AddAction(ItemActionController.ActionType.MainActionStarted, ItemType.Tool, StartAction);
        // If Hotbarslot is changed the current action should be stopped
        PlayerInventory.HotbarSlotChange += AbortAction;
        armAnimationEventHandler.Events["HitObject"] += HitObject;
        armAnimationEventHandler.Events["CritChance"] += CritChance;
    }
    void OnDestroy()
    {
        PlayerInventory.HotbarSlotChange -= AbortAction;
        armAnimationEventHandler.Events["HitObject"] -= HitObject;
        armAnimationEventHandler.Events["CritChance"] -= CritChance;
    }
    void StartAction(Item equippedItem)
    {
        if (InteractionController.TryGetInteraction(out InteractionData interaction) && interaction.interactableObject.TryGetComponent(out IHarvestable harvestableObject))
        {
            // check if an animation is already playing
            if (hitting)
            {
                if (!locked && critTimer > 0)
                {
                    armAnimator.SetTrigger("CritHit");
                    startingCrit = true;
                    critTimer = 0;
                }
                else
                    locked = true;
                return;
            }
            // no animation playing
            armAnimator.SetTrigger("ToolHit");
        }
        else
            armAnimator.Play("SecondHit");
        
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
        if (!hitting) return;
        float critTime = ((ToolItem)PlayerInventory.activeItem.ItemObject).CritTime;
        PlayerInventory.GetActiveItemGO().GetComponentInChildren<ToolCriticalHitIndicator>().CritChance(critTime);
        critTimer = critTime;
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
    public void EndPlaying()
    {
        if (startingCrit)
        {
            startingCrit = false;
            return;
        }
        hitting = false;
        armAnimator.speed = 1;
    }
    public void HitObject()
    {
        if (PlayerInventory.TryGetActiveItem(out Item equippedItem) && InteractionController.TryGetInteraction(out InteractionData interaction) && interaction.interactableObject.TryGetComponent(out IHarvestable harvestableObject))
        {
            harvestableObject.OnHit(equippedItem.ItemObject as ToolItem, interaction.hitData.point, (interaction.hitData.point - transform.position).XZ().AddHeight(0).normalized);
            CameraShakeController.Singleton.Shake(1.5f, 0.4f);
        }
    }
}
public interface IHarvestable
{
    void OnHit(ToolItem information, Vector3 position, Vector3 direction);
    public HarvestableType HarvestableType { get; }
}
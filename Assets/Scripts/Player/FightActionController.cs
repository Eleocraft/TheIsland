using UnityEngine;

public class FightActionController : MonoBehaviour
{
    [SerializeField] private float MaximumCharge = 3;
    [SerializeField] private GameObject WeaponHolder;
    [SerializeField] private AnimationEventHandler eventHandler;
    [SerializeField] private Animator CameraStateController;

    private InputMaster controls;
    private Animator animator;
    private float HeavyAttackTimer;
    private float ComboTimer;
    private bool HeavyAttack;
    private static bool attacking;
    private static bool drawing;
    public static bool Drawing => drawing;
    public static bool Attacking => attacking;

    private enum FightingState { Sword, Dagger, Spear, Bow }
    private FightingState state;
    
    void Start()
    {
        controls = GlobalData.controls;
        animator = transform.GetChild(0).GetComponent<Animator>();

        controls.PlayerTP.Attack.performed += Attack;
        controls.PlayerTP.DrawBow.started += StartDraw;
        controls.PlayerTP.DrawBow.canceled += StopDraw;
        controls.PlayerTP.HeavyAttack.started += StartHeavyAttack;
        controls.PlayerTP.HeavyAttack.canceled += ExecuteHeavyAttack;

        eventHandler.Events["ComboOpp"] += ComboOpp;
        eventHandler.Events["EndAttack"] += EndAttack;
        PlayerStateManager.GamemodeChange += ChangeGamemode;
    }
    void OnDestroy()
    {
        controls.PlayerTP.Attack.performed -= Attack;
        controls.PlayerTP.DrawBow.started -= StartDraw;
        controls.PlayerTP.DrawBow.canceled -= StopDraw;
        controls.PlayerTP.HeavyAttack.started -= StartHeavyAttack;
        controls.PlayerTP.HeavyAttack.canceled -= ExecuteHeavyAttack;

        eventHandler.Events["ComboOpp"] -= ComboOpp;
        eventHandler.Events["EndAttack"] -= EndAttack;
        PlayerStateManager.GamemodeChange -= ChangeGamemode;
    }
    void Update()
    {
        if (HeavyAttack && HeavyAttackTimer < MaximumCharge)
            HeavyAttackTimer += Time.deltaTime;
        if (ComboTimer > 0)
            ComboTimer -= Time.deltaTime;
    }
    private void ChangeGamemode(PlayerState newState)
    {
        WeaponHolder.SetActive(newState == PlayerState.ThirdPerson);
    }
    private void StartDraw(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (PlayerInventory.bow == null)
            return;
        
        state = FightingState.Bow;
        drawing = true;
        CameraStateController.Play("BowCam");
    }
    private void StopDraw(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        drawing = false;
        CameraStateController.Play("TPCam");
    }
    private void Attack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ComboTimer > 0)
        {
            animator.SetTrigger("Combo");
            return;
        }
        if (attacking || drawing)
            return;

        ItemObject WeaponItem = PlayerInventory.mainWeapon?.ItemObject;
        if (WeaponItem == null)
            return;
        state = GetFightingState(WeaponItem.Type);
        animator.SetTrigger("Slash");
        attacking = true;
    }
    private void ComboOpp()
    {
        ComboTimer = 0.2f;
    }
    private void EndAttack()
    {
        attacking = false;
    }
    private void StartHeavyAttack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        HeavyAttack = true;
    }
    private void ExecuteHeavyAttack(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        Debug.Log(HeavyAttackTimer);
        HeavyAttack = false;
        HeavyAttackTimer = 0;
    }
    private FightingState GetFightingState(ItemType type)
    {
        if (type == ItemType.Sword)
            return FightingState.Sword;
        else if (type == ItemType.Spear)
            return FightingState.Spear;
        else
            return FightingState.Dagger;
    }
}
public interface IDamagable
{
    void OnHit();
}
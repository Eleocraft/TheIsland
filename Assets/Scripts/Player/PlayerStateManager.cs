using UnityEngine;
using UnityEngine.Rendering;
using System;

public enum PlayerState { FirstPerson, ThirdPerson }
public class PlayerStateManager : MonoBehaviour
{
    private InputMaster controls;
    private static PlayerState state = PlayerState.FirstPerson;
    public static PlayerState State => state;
    // temp
    private static bool fightMode => state == PlayerState.ThirdPerson;
    public static bool FightMode => fightMode;

    public static event Action<bool> GamemodeChange;

    private Animator animator;
    [SerializeField] private Renderer[] thirdPersonModelRenderers;
    private float weight;
    private int TPLayer;
    private int FPLayer;
    [SerializeField] private GameObject FPHUD;
    [SerializeField] private GameObject TPHUD;

    void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        TPLayer = animator.GetLayerIndex("ThirdPerson");
        FPLayer = animator.GetLayerIndex("FirstPerson");
        controls = GlobalData.controls;
        controls.Player.ChangeMode.performed += ManualSwitch;
    }
    void OnDestroy()
    {
        controls.Player.ChangeMode.performed -= ManualSwitch;
    }

    void Update()
    {
        if (weight > 0)
        {
            weight -= Time.deltaTime;
            animator.SetLayerWeight(fightMode? TPLayer : FPLayer, 1f - weight);
            if (weight <= 0)
                animator.SetLayerWeight(fightMode? FPLayer : TPLayer, 0);
        }
    }
    void ManualSwitch(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeGamemode(fightMode? PlayerState.FirstPerson : PlayerState.ThirdPerson);

    void ChangeGamemode(PlayerState newState)
    {
        // no GamemodeSwitch when Attacking
        if (FightActionController.Attacking)
            return;

        state = newState;
        GamemodeChange(fightMode);
        weight = 1f;

        if (fightMode) // Switch to Third Person
        {
            foreach (SkinnedMeshRenderer modelRenderer in thirdPersonModelRenderers)
                modelRenderer.shadowCastingMode = ShadowCastingMode.On;
            FPHUD.SetActive(false);
            TPHUD.SetActive(true);
            controls.PlayerFP.Disable();
            controls.PlayerTP.Enable();
            animator.SetLayerWeight(FPLayer, 1);
        }
        else // Switch to First Person
        {
            foreach (SkinnedMeshRenderer modelRenderer in thirdPersonModelRenderers)
                modelRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            FPHUD.SetActive(true);
            TPHUD.SetActive(false);
            controls.PlayerFP.Enable();
            controls.PlayerTP.Disable();
            animator.SetLayerWeight(TPLayer, 1);
        }
    }
}

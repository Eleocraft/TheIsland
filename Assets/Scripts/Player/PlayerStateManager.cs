using UnityEngine;
using UnityEngine.Rendering;
using System;

public enum PlayerState { FirstPerson, ThirdPerson }
public class PlayerStateManager : MonoBehaviour
{
    private InputMaster controls;
    private static PlayerState state = PlayerState.FirstPerson;
    public static PlayerState State => state;
    public static event Action<PlayerState> GamemodeChange;

    [SerializeField] private Animator playermodelAnimator;
    [SerializeField] private SkinnedMeshRenderer[] thirdPersonModelRenderers;
    private float weight;
    private int TPLayer;
    private int FPLayer;
    [SerializeField] private GameObject FPHUD;
    [SerializeField] private GameObject TPHUD;

    void Start()
    {
        TPLayer = playermodelAnimator.GetLayerIndex("ThirdPerson");
        FPLayer = playermodelAnimator.GetLayerIndex("FirstPerson");
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
            playermodelAnimator.SetLayerWeight((state == PlayerState.FirstPerson)? TPLayer : FPLayer, 1f - weight);
            if (weight <= 0)
                playermodelAnimator.SetLayerWeight((state == PlayerState.ThirdPerson)? FPLayer : TPLayer, 0);
        }
    }
    void ManualSwitch(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => ChangeGamemode((state == PlayerState.ThirdPerson) ? PlayerState.FirstPerson : PlayerState.ThirdPerson);

    void ChangeGamemode(PlayerState newState)
    {
        // no GamemodeSwitch when Attacking
        if (FightActionController.Attacking)
            return;

        state = newState;
        GamemodeChange(state);
        weight = 1f;

        if (state == PlayerState.ThirdPerson) // Switch to Third Person
        {
            foreach (SkinnedMeshRenderer modelRenderer in thirdPersonModelRenderers)
                modelRenderer.shadowCastingMode = ShadowCastingMode.On;
            FPHUD.SetActive(false);
            TPHUD.SetActive(true);
            controls.PlayerFP.Disable();
            controls.PlayerTP.Enable();
            playermodelAnimator.SetLayerWeight(FPLayer, 1);
        }
        else if (state == PlayerState.FirstPerson) // Switch to First Person
        {
            foreach (SkinnedMeshRenderer modelRenderer in thirdPersonModelRenderers)
                modelRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            FPHUD.SetActive(true);
            TPHUD.SetActive(false);
            controls.PlayerFP.Enable();
            controls.PlayerTP.Disable();
            playermodelAnimator.SetLayerWeight(TPLayer, 1);
        }
    }
}

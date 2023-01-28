using UnityEngine;

public class DisableHUD : MonoBehaviour
{
    [SerializeField] private GameObject HUD;
    private void Start()
    {
        GlobalData.controls.Debug.HideHUD.performed += ToggleHUD;
    }
    private void OnDestroy()
    {
        GlobalData.controls.Debug.HideHUD.performed -= ToggleHUD;
    }
    private void ToggleHUD(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        HUD.SetActive(!HUD.activeSelf);
    }
}

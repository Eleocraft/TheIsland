using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChestSign : MonoBehaviour, IInteractable
{
    TMP_InputField signInput;
    bool active;
    bool closeSign;
    public string InteractionInfo => "Change name";
    void Start()
    {
        signInput = GetComponentInChildren<TMP_InputField>();
    }
    public void Interact()
    {
        if (active)
            return;
        
        GlobalData.controls.Menus.ExitSign.performed += CloseSignInput;
        InputStateMachine.ChangeInputState(false, this);
        signInput.ActivateInputField();
        active = true;
    }
    void CloseSignInput(UnityEngine.InputSystem.InputAction.CallbackContext ctx) => closeSign = true;
    void Update()
    {
        if (closeSign)
            CloseSign();
    }
    void CloseSign()
    {
        InputStateMachine.ChangeInputState(true, this);
        signInput.DeactivateInputField();
        GlobalData.controls.Menus.ExitSign.performed -= CloseSignInput;
        active = false;
        closeSign = false;
    }
    void OnDestroy() {
        if (active)
            GlobalData.controls.Menus.ExitSign.performed -= CloseSignInput;
    }
}

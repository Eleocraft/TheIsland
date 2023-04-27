using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChestSign : MonoBehaviour, IInteractable
{
    TMP_InputField signInput;
    bool active;
    bool closeSign;
    public string InteractionInfo => "Change name";
    [ResetOnDestroy]
    private static Dictionary<int, string> chestsSigns = new();

    [Save(SaveType.world)]
    public static object ChestSignsSaveData
    {
        get => chestsSigns;
        set => chestsSigns = (Dictionary<int, string>)value;
    }
    void Start()
    {
        signInput = GetComponentInChildren<TMP_InputField>();
        int Id = GetComponentInParent<Building>().Id;
        if (chestsSigns.ContainsKey(Id))
            signInput.text = chestsSigns[Id];
        else
            chestsSigns.Add(Id, "");
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
        chestsSigns[GetComponentInParent<Building>().Id] = signInput.text;
        GlobalData.controls.Menus.ExitSign.performed -= CloseSignInput;
        active = false;
        closeSign = false;
    }
    void OnDestroy() {
        if (active)
            GlobalData.controls.Menus.ExitSign.performed -= CloseSignInput;
    }
}

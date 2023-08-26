using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ItemActionController : MonoBehaviour
{
    public enum ActionType { MainActionStarted, MainActionCanceled, SecondaryActionStarted, SecondaryActionCanceled, TertiaryActionStarted, TertiaryActionCanceled }
    private InputMaster _controls;
    private Dictionary<ActionType, Dictionary<ItemType, Action<Item>>> _itemActions = new();
    private void Start()
    {
        foreach (ActionType actionType in Utility.GetEnumValues<ActionType>())
            _itemActions.Add(actionType, new());

        _controls = GlobalData.controls;
        _controls.Mouse.MainAction.started += Action;
        _controls.Mouse.SecondaryAction.started += Action;
        _controls.Mouse.TertiaryAction.started += Action;
        
        _controls.Mouse.MainAction.canceled += Action;
        _controls.Mouse.SecondaryAction.canceled += Action;
        _controls.Mouse.TertiaryAction.canceled += Action;
    }
    private void OnDestroy()
    {
        _controls.Mouse.MainAction.started -= Action;
        _controls.Mouse.SecondaryAction.started -= Action;
        _controls.Mouse.TertiaryAction.started -= Action;
        
        _controls.Mouse.MainAction.canceled -= Action;
        _controls.Mouse.SecondaryAction.canceled -= Action;
        _controls.Mouse.TertiaryAction.canceled -= Action;
    }
    public void AddAction(ActionType actionType, ItemType itemType, Action<Item> action)
    {
        _itemActions[actionType].Add(itemType, action);
    }
    private void Action(InputAction.CallbackContext ctx)
    {
        if (!PlayerInventory.TryGetActiveItem(out Item equippedItem))
            return;

        Dictionary<ItemType, Action<Item>> actionDictionary = _itemActions[GetActionType()];
        if (actionDictionary.ContainsKey(equippedItem.ItemObject.Type))
            actionDictionary[equippedItem.ItemObject.Type]?.Invoke(equippedItem);

        ActionType GetActionType()
        {
            return (ActionType)Enum.Parse(typeof(ActionType), ctx.action.name + ctx.action.phase);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildingSystemInterface : MonoBehaviour
{
    [SerializeField] private GameObject buildingField;
    [SerializeField] private BuildingActionController BAC;
    List<GameObject> slotsOnInterface;

    private void Start()
    {
        CreateSlots();
    }
    private void CreateSlots()
    {
        slotsOnInterface = new();
        foreach (BuildingObject buildingObj in BuildingDatabase.BuildingObjects.Values)
        {
            GameObject obj = Instantiate(buildingField, Vector3.zero, Quaternion.identity, transform);
            obj.transform.GetChild(0).GetComponent<Image>().sprite = buildingObj.Image;

            AddEvent(obj, EventTriggerType.PointerEnter, (BaseEventData data) => { OnEnter(buildingObj); });
            AddEvent(obj, EventTriggerType.PointerExit, (BaseEventData data) => { OnExit(buildingObj); });
            AddEvent(obj, EventTriggerType.PointerClick, (BaseEventData data) => { OnClick(buildingObj, data as PointerEventData); });

            slotsOnInterface.Add(obj);
        }
    }
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        EventTrigger.Entry eventTrigger = new() { eventID = type };
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    private void OnEnter(BuildingObject obj)
    {
        TooltipPanelManager.CreateTooltips(obj.GetTooltips());
    }
    private void OnExit(BuildingObject obj)
    {
        TooltipPanelManager.Deactivate();
    }
    private void OnDisable()
    {
        TooltipPanelManager.Deactivate();
    }
    private void OnClick(BuildingObject obj, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            BAC.StartBuildMode(obj);
    }

}

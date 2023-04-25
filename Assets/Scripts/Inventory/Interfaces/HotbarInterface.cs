using UnityEngine;
using UnityEngine.UI;

public class HotbarInterface : StaticInventoryInterface
{
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color selectedColor;

    private int activeSlot;
    protected override void OnEnable() { }
    public void ChangeHotbarSlot(int newSlot)
    {
        slots[activeSlot].GetComponent<Image>().color = defaultColor;
        slots[newSlot].GetComponent<Image>().color = selectedColor;
        activeSlot = newSlot;
    }
}

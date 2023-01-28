using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarInterface : StaticInventoryInterface
{
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color selectedColor;

    private int activeSlot;
    protected override void OnEnable() { }
    protected override void Start()
    {
        base.Start();
        activeSlot = 0;
        slots[activeSlot].GetComponent<Image>().color = selectedColor;
    }
    public void ChangeHotbarSlot(int newSlot)
    {
        slots[activeSlot].GetComponent<Image>().color = defaultColor;
        slots[newSlot].GetComponent<Image>().color = selectedColor;
        activeSlot = newSlot;
    }
}

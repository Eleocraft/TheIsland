using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

[AddComponentMenu("UI/SliderSetting", 33)]
[RequireComponent(typeof(Slider))]
public class SliderSetting : SettingPanelObject
{
    [SerializeField] private TMP_Text value;
    [SerializeField] private TMP_Text nameField;
    [SerializeField] private string format;
    private string _Id;
    public override string Id => _Id;
    public override int Size => 80;

    private Slider slider;
    public override event Action<string> SettingChanged;
    private bool invokedChange;

    public override void OnInitialisation(string name, string Id, SettingAttribute objectSetting)
    {
        SliderSettingAttribute sliderObjectSettings = objectSetting as SliderSettingAttribute;
        _Id = Id;
        nameField.text = name;
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener((var) => ValueChanged());
        slider.minValue = sliderObjectSettings.minValue;
        slider.maxValue = sliderObjectSettings.maxValue;
        value.text = slider.value.ToString(format);
    }

    public void ValueChanged()
    {
        value.text = slider.value.ToString(format);
        if (!invokedChange)
            SettingChanged?.Invoke(value.text);
        else
            invokedChange = false;
    }
    public override void ChangeValue(string value)
    {
        invokedChange = true;
        slider.value = float.Parse(value);
    }
}
public class SliderSettingAttribute : SettingAttribute
{
    public override SettingObjectType objectType => SettingObjectType.Slider;
    readonly public int minValue;
    readonly public int maxValue;
    public SliderSettingAttribute (SettingCategory settingCategory, SettingLoadTime loadTime, string settingsName, string defaultValue, int minValue, int maxValue) : base(settingCategory, loadTime, settingsName, defaultValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
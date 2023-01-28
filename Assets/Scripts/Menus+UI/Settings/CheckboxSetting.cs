using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

[AddComponentMenu("UI/CheckboxSetting", 33)]
[RequireComponent(typeof(Toggle))]
public class CheckboxSetting : SettingPanelObject
{
    [SerializeField] private TMP_Text nameField;
    private string _Id;
    public override string Id => _Id;
    public override int Size => 80;

    private Toggle checkbox;
    public override event Action<string> SettingChanged;
    private bool invokedChange;

    public override void OnInitialisation(string name, string Id, SettingAttribute objectSetting)
    {
        _Id = Id;
        nameField.text = name;
        checkbox = GetComponent<Toggle>();
        checkbox.onValueChanged.AddListener((var) => ValueChanged(var));
    }

    public void ValueChanged(bool value)
    {
        if (!invokedChange)
            SettingChanged?.Invoke(checkbox.isOn.ToString());
        else
            invokedChange = false;
    }
    public override void ChangeValue(string value)
    {
        invokedChange = true;
        checkbox.isOn = bool.Parse(value);
    }
}
public class CheckboxSettingAttribute : SettingAttribute
{
    public override SettingObjectType objectType => SettingObjectType.Checkbox;
    public CheckboxSettingAttribute (SettingCategory settingCategory, SettingLoadTime loadTime, string settingsName, string defaultValue) : base(settingCategory, loadTime, settingsName, defaultValue) { }
}
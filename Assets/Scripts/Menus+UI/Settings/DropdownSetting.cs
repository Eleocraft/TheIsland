using UnityEngine;
using System.Linq;
using System;
using TMPro;

[AddComponentMenu("UI/DropdownSetting", 33)]
[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownSetting : SettingPanelObject
{
    [SerializeField] private TMP_Text nameField;
    private string _Id;
    public override string Id => _Id;
    public override int Size => 90;

    private TMP_Dropdown dropdown;
    public override event Action<string> SettingChanged;
    private bool invokedChange;

    public override void OnInitialisation(string name, string Id, SettingAttribute objectSetting)
    {
        DropdownSettingAttribute dropdownObjectSettings = objectSetting as DropdownSettingAttribute;
        _Id = Id;
        nameField.text = name;
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener((var) => ValueChanged());
        dropdown.AddOptions(dropdownObjectSettings.options.ToList());
    }
    public void ValueChanged()
    {
        if (!invokedChange)
            SettingChanged?.Invoke(dropdown.captionText.text);
        else
            invokedChange = false;
    }
    public override void ChangeValue(string value)
    {
        invokedChange = true;
        dropdown.value = dropdown.options.FindIndex(option => option.text == value);
    }
}
public class DropdownSettingAttribute : SettingAttribute
{
    public override SettingObjectType objectType => SettingObjectType.Dropdown;
    readonly public string[] options;
    public DropdownSettingAttribute (SettingCategory settingCategory, SettingLoadTime loadTime, string settingsName, string defaultValue, string[] options) : base(settingCategory, loadTime, settingsName, defaultValue)
    {
        this.options = options;
    }
}
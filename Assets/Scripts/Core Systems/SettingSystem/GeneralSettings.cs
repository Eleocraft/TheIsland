using UnityEngine;
public static class GeneralSettings
{
    [DropdownSetting(SettingCategory.Display, SettingLoadTime.Start, "Target Framerate", "60", new string[] { "No Limit", "30", "60", "90", "120", "144", "270", "360"})]
    public static void ChangeTargetFrameRate(string settingValue)
    {
        if (settingValue == "No Limit")
            Application.targetFrameRate = -1;
        else
            Application.targetFrameRate = int.Parse(settingValue);
    }
    [CheckboxSetting(SettingCategory.Display, SettingLoadTime.Start, "Fullscreen", "true")]
    public static void ToggleFullscreen(string settingValue)
    {
        Screen.fullScreen = bool.Parse(settingValue);
    }
    [DropdownSetting(SettingCategory.Display, SettingLoadTime.Start, "Resolution", "1920x1080", new string[] { "1280x720", "1920x1080", "2560x1440", "3840x2160"})]
    public static void ChangeResolution(string settingValue)
    {
        string[] resolution = settingValue.Split("x");
        Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), Screen.fullScreenMode, Application.targetFrameRate);
    }
}

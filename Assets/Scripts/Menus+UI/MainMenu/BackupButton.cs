using UnityEngine;
using TMPro;

public class BackupButton : MonoBehaviour
{
    [Header("--Objects")]
    [SerializeField] private TMP_Text NameObj;

    private int slot;
    private const string loadMessage = "Are you sure that you want to load this backup?\nThe original savefile will be overwritten!";

    public void OnInitialisation(string name, int slot)
    {
        this.slot = slot;
        NameObj.text = name;
    }

    public void Load() => MainMenuController.LoadBackup(slot);
}

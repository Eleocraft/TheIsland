using UnityEngine;
using System;
using TMPro;

public class BackupButton : MonoBehaviour
{
    [Header("--Objects")]
    [SerializeField] private TMP_Text NameObj;

    private int backupID;
    private int slot;
    private const string loadMessage = "Are you sure that you want to load this backup?\nThe original savefile will be overwritten!";
    private const string backupButtonText = "Backup";

    public void OnInitialisation(DateTime creationTime, int backupID, int slot)
    {
        this.backupID = backupID;
        this.slot = slot;
        NameObj.text = $"{backupButtonText} {creationTime:g}";
    }

    public void Load() => MainMenuController.LoadBackup(backupID, slot);
}

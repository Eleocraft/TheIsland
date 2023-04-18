using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveButton : MonoBehaviour
{
    [Header("--Objects")]
    [SerializeField] private TMP_Text NameObj;
    [SerializeField] private TMP_Text DateObj;
    [SerializeField] private Image Screenshot;

    private int slot;
    private const string deleteMessage = "Are you sure that you want to delete that savefile?\nThis action cannot be undone!";

    public void OnInitialisation(string name, Savegame savegame, int slot, Texture2D tex)
    {
        this.slot = slot;
        NameObj.text = name;
        DateObj.text = savegame.date.ToString("d");
        Screenshot.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public void Load() => MainMenuController.Load(slot);
    public void Delete() => Popup.Create(PopupType.YesNo, deleteMessage, () => MainMenuController.DeleteSaveFile(slot));
}

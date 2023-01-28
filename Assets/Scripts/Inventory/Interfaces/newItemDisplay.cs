using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class newItemDisplay : MonoBehaviour
{
    private float decayTime;
    private float totalDecayTime;
    private float fadeTime;
    private float totalFadeTime;
    private int count;
    private TMP_Text ItemName;
    private TMP_Text Count;
    private Image ItemImage;
    private ItemObject itemObject;
    public void Initialize(float decayTime, float fadeTime, ItemObject itemObject, int count)
    {
        this.decayTime = decayTime;
        this.fadeTime = fadeTime;
        this.itemObject = itemObject;
        this.count = count;
        totalFadeTime = fadeTime;
        totalDecayTime = decayTime;

        ItemName = transform.GetChild(0).GetComponent<TMP_Text>();
        Count = transform.GetChild(1).GetComponent<TMP_Text>();
        ItemImage = transform.GetChild(2).GetComponent<Image>();

        ItemName.text = itemObject.name;
        Count.text = string.Concat("x", count);
        ItemImage.sprite = itemObject.Image;
    }
    public void newCount(int newCount)
    {
        count += newCount;
        Count.text = string.Concat("x", count);
        ItemName.color = new Color(ItemName.color.r, ItemName.color.g, ItemName.color.b, 1f);
        Count.color = new Color(Count.color.r, Count.color.g, Count.color.b, 1f);
        ItemImage.color = new Color(ItemImage.color.r, ItemImage.color.g, ItemImage.color.b, 1f);
        decayTime = totalDecayTime;
        fadeTime = totalFadeTime;
    }
    void Update()
    {
        if (decayTime > 0f)
            decayTime -= Time.deltaTime;
        else if (fadeTime > 0f)
        {
            fadeTime -= Time.deltaTime;
            ItemName.color = new Color(ItemName.color.r, ItemName.color.g, ItemName.color.b, (fadeTime / totalFadeTime));
            Count.color = new Color(Count.color.r, Count.color.g, Count.color.b, (fadeTime / totalFadeTime));
            ItemImage.color = new Color(ItemImage.color.r, ItemImage.color.g, ItemImage.color.b, (fadeTime / totalFadeTime));
        }
        else
        {
            PlayerInventory.Instance.RemoveItemDisplayObject(itemObject);
            Destroy(this.gameObject);
        }
    }
}

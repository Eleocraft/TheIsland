using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialTooltip : MonoBehaviour, ITooltipObject
{
    [SerializeField] private int spacing = 5;
    public Vector2 Initialize(TooltipAttributeData Information)
    {
        MaterialTooltipAttributeData data = (MaterialTooltipAttributeData)Information;
        Image image = GetComponentInChildren<Image>();
        image.sprite = data.material.itemObj.Image;
        image.color = PlayerInventory.ContainsItems(data.material.itemObj.CreateItem(), data.material.amount) ? Color.white : Color.red;
        TMP_Text textMesh = GetComponent<TMP_Text>();
        textMesh.text = data.material.amount.ToString();
        textMesh.ForceMeshUpdate();
        return (Vector2)textMesh.textBounds.extents * 2 + new Vector2(0, spacing);
    }
}
public class MaterialTooltipAttributeData : TooltipAttributeData
{
    public readonly ItemAmountInfo material;
    public MaterialTooltipAttributeData(ItemAmountInfo material) : base(TooltipAttributeType.MaterialCost)
    {
        this.material = material;
    }
}
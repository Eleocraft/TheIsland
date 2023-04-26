using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextTooltip : MonoBehaviour, ITooltipObject
{
    [SerializeField] private int spacing;
    public Vector2 Initialize(TooltipAttributeData Information)
    {
        TMP_Text textMesh = GetComponent<TMP_Text>();
        textMesh.text = ((TextTooltipAttributeData)Information).text;
        textMesh.ForceMeshUpdate();
        return (Vector2)textMesh.textBounds.extents * 2 + new Vector2(0, spacing);
    }
}
public class TextTooltipAttributeData : TooltipAttributeData
{
    public readonly string text;
    public TextTooltipAttributeData(TooltipAttributeType type, string text) : base(type)
    {
        this.text = text;
    }
}
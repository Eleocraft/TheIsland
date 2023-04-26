using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HammerLevelTooltip : MonoBehaviour, ITooltipObject
{
    [SerializeField] private int spacing = 5;
    public Vector2 Initialize(TooltipAttributeData Information)
    {
        GetComponentInChildren<Image>().color = ((HammerLevelTooltipAttributeData)Information).levelHighEnough ? Color.white : Color.red;
        TMP_Text textMesh = GetComponent<TMP_Text>();
        textMesh.text = ((HammerLevelTooltipAttributeData)Information).level.ToString();
        textMesh.ForceMeshUpdate();
        return (Vector2)textMesh.textBounds.extents * 2 + new Vector2(0, spacing);
    }
}
public class HammerLevelTooltipAttributeData : TooltipAttributeData
{
    public readonly int level;
    public readonly bool levelHighEnough;
    public HammerLevelTooltipAttributeData(int level, bool levelHighEnough) : base(TooltipAttributeType.BuildingLevel)
    {
        this.level = level;
        this.levelHighEnough = levelHighEnough;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class TooltipPanelManager : MonoSingleton<TooltipPanelManager>
{
    [SerializeField] private Vector2 TooltipSpacing;
    private Vector2 DefaultScale;
    private InputMaster controls;
    protected override void SingletonAwake()
    {
        DefaultScale = GetComponent<RectTransform>().sizeDelta;
    }
    void Start()
    {
        controls = GlobalData.controls;
        Deactivate();
    }
    void Update()
    {
        transform.position = controls.Menus.MousePosition.ReadValue<Vector2>();
    }
    public static void Deactivate()
    {
        Instance.gameObject.SetActive(false);
    }
    public static void CreateTooltips(List<TooltipAttributeData> data) => Instance.createTooltips(data);
    private void createTooltips(List<TooltipAttributeData> data)
    {
        gameObject.SetActive(true);
        transform.position = controls.Menus.MousePosition.ReadValue<Vector2>();
        gameObject.ClearChilds();
        float CurrentPlacement = 0;
        RectTransform tooltipTransform = GetComponent<RectTransform>();
        tooltipTransform.sizeDelta = DefaultScale;
        foreach (TooltipAttributeData Tooltip in data)
        {
            GameObject TooltipObject = Instantiate(TooltipPrefabLister.TooltipPrefabs[Tooltip.type]);
            RectTransform TooltipObjectTransform = TooltipObject.GetComponent<RectTransform>();
            TooltipObjectTransform.SetParent(tooltipTransform, false);
            TooltipObjectTransform.anchoredPosition = new Vector2(0, CurrentPlacement) + TooltipSpacing;
            Vector2 bounds = TooltipObject.GetComponent<ITooltipObject>().Initialize(Tooltip);
            CurrentPlacement -= bounds.y;
            if (tooltipTransform.sizeDelta.x + TooltipSpacing.x < bounds.x + TooltipSpacing.x * 2)
                tooltipTransform.sizeDelta = new Vector2(bounds.x + TooltipSpacing.x * 2, tooltipTransform.sizeDelta.y);

            if (tooltipTransform.sizeDelta.y < CurrentPlacement * -1 + TooltipSpacing.y * 2)
                tooltipTransform.sizeDelta = new Vector2(tooltipTransform.sizeDelta.x, CurrentPlacement * -1 + TooltipSpacing.y * 2);
        }
    }
}
public enum TooltipAttributeType
{
    Title,
    description,
    HealValue,
    AttackValue,
    DefenseValue,
    BuildingLevel,
    BuildingCost,
    HarvestingSpeed
}
public class TooltipAttributeData
{
    public readonly TooltipAttributeType type;
    public TooltipAttributeData(TooltipAttributeType type)
    {
        this.type = type;
    }
}
public interface ITooltipObject
{
    Vector2 Initialize(TooltipAttributeData Information);
}
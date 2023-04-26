using UnityEngine;

public class TooltipPrefabLister : MonoSingleton<TooltipPrefabLister>
{
    [SerializeField] private EnumDictionary<TooltipAttributeType, GameObject> tooltipPrefabs;
    public static EnumDictionary<TooltipAttributeType, GameObject> TooltipPrefabs => Instance.tooltipPrefabs;
    private void OnValidate()
    {
        tooltipPrefabs.Update();
    }
}

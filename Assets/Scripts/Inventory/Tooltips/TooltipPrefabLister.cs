using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipPrefabLister : MonoBehaviour
{
    [SerializeField] private TooltipPrefab[] TooltipPrefabs;
    public static Dictionary<TooltipAttributeType, GameObject> tooltipPrefabs;
    void Awake()
    {
        tooltipPrefabs = new Dictionary<TooltipAttributeType, GameObject>();
        foreach (TooltipPrefab data in TooltipPrefabs)
        {
            tooltipPrefabs.Add(data.type, data.prefab);
        }
    }
    [System.Serializable]
    private struct TooltipPrefab
    {
        public TooltipAttributeType type;
        public GameObject prefab;
    }
}

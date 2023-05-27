using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GridBuilding : Building
{
    private BuildingSnappingPoint[] snappingPoints;
    public bool checkingForStability { get; private set; }
    private bool recalculatingStability;
    private List<StandaloneGridBuilding> connectedStandaloneBuildings = new();
    [ResetOnDestroy]
    private static Dictionary<int, int> stabilities = new();
    [Save(SaveType.world)]
    public static object StabilitySaveData
    {
        get => stabilities;
        set => stabilities = (Dictionary<int, int>)value;
    }
    [SerializeField] private TextMeshPro StabilityDisplayPrefab;
    private TextMeshPro stabilityDisplay;
    void Awake()
    {
        snappingPoints = GetComponentsInChildren<BuildingSnappingPoint>();
        stabilityDisplay = Instantiate(StabilityDisplayPrefab, transform.position, Quaternion.identity, transform);
    }
    public override void Initialize()
    {
        base.Initialize();
        stabilities.Add(Id, -1);
        CalculateStabilityAfterPhysicsUpdate();
    }
    public void AddStandaloneBuilding(StandaloneGridBuilding building) => connectedStandaloneBuildings.Add(building);
    public virtual int GetStability(bool recalculateStability = false)
    {
        if (checkingForStability)
            return 0;
        
        if (stabilities[Id] != -1 && !recalculateStability)
            return stabilities[Id];

        int tempStability = 0;
        checkingForStability = true;
        foreach (BuildingSnappingPoint point in snappingPoints)
        {
            int pointStability = point.GetStability(recalculateStability);
            if (tempStability < pointStability)
                tempStability = pointStability;
        }
        checkingForStability = false;
        return tempStability;
    }
    public void CalculateStabilityAfterPhysicsUpdate() => StartCoroutine(Utility.WaitForPhysicsUpdate(() => CalculateStability(false)));
    public void RecalculateCalculateStabilityAfterPhysicsUpdate() => StartCoroutine(Utility.WaitForPhysicsUpdate(() => CalculateStability(true)));
    public void CalculateStability(bool recursive)
    {
        if (recalculatingStability)
            return;
        
        int oldStability = stabilities[Id];
        stabilities[Id] = -1;
        stabilities[Id] = GetStability(recursive);
        stabilityDisplay.text = stabilities[Id].ToString();
        recalculatingStability = true;
        if (oldStability != stabilities[Id])
        {
            foreach (BuildingSnappingPoint point in snappingPoints)
                point.RecalculateConnectedBuildingStability();
        }
        recalculatingStability = false;
        if (stabilities[Id] <= 0)
        {
            foreach(StandaloneGridBuilding building in connectedStandaloneBuildings)
                Destroy(building.gameObject);
            Destroy(gameObject);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        stabilities.Remove(Id);
        foreach (BuildingSnappingPoint point in snappingPoints)
            point.RecalculateConnectedBuildingStabilityAfterPhysicsUpdate();
    }
}

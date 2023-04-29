using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BrokenRaft : MonoSingleton<BrokenRaft>, IInteractable
{
    private enum BrokenRaftState { Inactive, Repairing, Ready }
    private BrokenRaftState _state;
    private BrokenRaftState state
    {
        get => _state;
        set
        {
            _state = value;
            if (value == BrokenRaftState.Repairing)
                foreach (BrokenRaftPart part in raftParts)
                    part.Activate(this);
        }
    }

    public string InteractionInfo
    {
        get
        {
            if (state == BrokenRaftState.Inactive)
                return $"press {GlobalData.controls.Interaction.MainInteraction.bindings[0].ToDisplayString()} to Inspekt the Raft";
            else if (state == BrokenRaftState.Repairing)
            {
                string parts = "";
                foreach (BrokenRaftPart raftPart in raftParts)
                    parts += $"{(raftPart.Repaired ? "" : raftPart.PartName)} ";
                return $"repair {parts} to proceed";
            }
            else
                return $"press {GlobalData.controls.Interaction.MainInteraction.bindings[0].ToDisplayString()} to Repair the Raft";;
        }
    }

    [Save(SaveType.world)]
    public static object RaftStateSaveData
    {
        get => Instantiated() ? (int)Instance.state : 3;
        set
        {
            int state = (int)value;
            if (state == 3)
                Destroy(Instance.gameObject);
            else
                Instance.state = (BrokenRaftState)state;
        }
    }
    [Save(SaveType.world)]
    public static object RaftPartSaveData
    {
        get => Instance.raftParts.Select(raftPart => raftPart.Repaired).ToList();
        set
        {
            List<bool> partStates = (List<bool>)value;
            for (int i = 0; i < Instance.raftParts.Count; i++)
                if (partStates[i] == true)
                    Instance.raftParts[i].Repair();
        }
    }


    [SerializeField] private List<BrokenRaftPart> raftParts;
    [SerializeField] private GameObject raftPrefab;
    [SerializeField] private Transform raftSpawnPoint;

    public void Interact()
    {
        if (state == BrokenRaftState.Inactive)
            state = BrokenRaftState.Repairing;
        else if (state == BrokenRaftState.Ready)
            SpawnRaft();
    }
    void SpawnRaft()
    {
        Instantiate(raftPrefab, raftSpawnPoint.position, raftSpawnPoint.rotation);
        Destroy(gameObject);
    }
    public void AddPart()
    {
        foreach (BrokenRaftPart raftPart in raftParts)
            if (!raftPart.Repaired)
                return;
        state = BrokenRaftState.Ready;
    }

    // Temp
    // void Start()
    // {
    //     GlobalData.controls.PlayerFP.Drop.performed += NextPart;
    // }
    // void OnDestroy()
    // {
    //     GlobalData.controls.PlayerFP.Drop.performed -= NextPart;
    // }
    // void NextPart(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    // {
    //     Debug.Log("nice");
    //     foreach (BrokenRaftPart raftPart in raftParts)
    //     {
    //         if (!raftPart.Repaired)
    //         {
    //             raftPart.Repair();
    //             return;
    //         }
    //     }
    // }
}
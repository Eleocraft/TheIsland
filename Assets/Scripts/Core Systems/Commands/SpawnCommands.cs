using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnCommands : MonoSingleton<SpawnCommands>
{
    [SerializeField] private CommandSpawnObj[] spawnableObjects;
    [SerializeField] private Transform player;

    [Command]
    public static void Spawn(List<string> args)
    {
        if (args.Count <= 0)
            return;

        GameObject obj = Instance.spawnableObjects.Where(c => c.name == args[0]).Select(c => c.prefab).Single();
        Instantiate(obj, Instance.player.position, Quaternion.identity);
    }
    
}
[System.Serializable]
public class CommandSpawnObj
{
    public string name;
    public GameObject prefab;
}
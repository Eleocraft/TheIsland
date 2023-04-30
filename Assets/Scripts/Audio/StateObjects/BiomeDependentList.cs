using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Biome Dependent List", menuName = "CustomObjects/Audio/BiomeDependentList")]
public class BiomeDependentList : SoundStateObject
{
    [SerializeField] private TerrainSettings referenceTerrain;
    [SerializeField] private SerializableDictionary<string, SoundStateObject> music;
    private string biome;

    public void OnValidate()
    {
        music.Update();
        string[] biomes = referenceTerrain?.Biomes.Select(b => b.name).ToArray();
        if (referenceTerrain && music.Count != biomes.Length)
        {
            foreach (string biome in music.Keys)
                if (!biomes.Contains(biome))
                    music.Remove(biome);
            foreach (string biome in biomes)
                if (!music.Keys.Contains(biome))
                    music.Add(biome);
            music.RemoveDublicates();
        }
    }
    private void OnEnable()
    {
        music.Update();
    }
    public override AudioClip GetClip()
    {
        biome = MapRuntimeHandler.GetViewerBiome().name;
        return music[biome].GetClip();
    }
    public override float GetPauseTime()
    {
        biome = MapRuntimeHandler.GetViewerBiome().name;
        return music[biome].GetPauseTime();
    }
    public override bool StateChanged()
    {
        if (music[MapRuntimeHandler.GetViewerBiome().name] != music[biome])
            return true;
        return false;
    }
}
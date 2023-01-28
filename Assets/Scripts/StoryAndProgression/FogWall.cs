using UnityEngine;
using System.Collections.Generic;

public class FogWall : MonoSingleton<FogWall>
{
    [SerializeField] private float MaxDistance;
    [SerializeField] private Transform player;
    [SerializeField] private float fadeTime;
    [SerializeField] private GameObject raftBlocker;
    private float sqrMaxDist;
    private MeshRenderer meshRenderer;
    float fadeTimer;
    [Save(SaveType.world)]
    public static object FogWallSaveData
    {
        get => Instantiated();
        set
        {
            if (!(bool)value)
            {
                Instance.EndFogWall();
                Destroy(Instance.gameObject);
            }
        }
    }
    void Start()
    {
        sqrMaxDist = MaxDistance * MaxDistance;
        meshRenderer = GetComponent<MeshRenderer>();
    }
    void Update()
    {
        if (player.position.sqrMagnitude > sqrMaxDist && fadeTimer <= 0)
            EndFogWall();
        if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            meshRenderer.material.SetFloat("_Transparency", fadeTimer / fadeTime);
            if (fadeTimer <= 0)
            {
                Destroy(gameObject);
                PostProcessingManager.UpdateVolumeStack();
            }
        }
    }
    void EndFogWall()
    {
        Wind.StartTimer();
        fadeTimer = fadeTime;
        Destroy(raftBlocker);
    }
    [Command]
    public static void ClearSpawnFog(List<string> args) => Instance.EndFogWall();
}

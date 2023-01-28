using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class StartBombExplosion : MonoBehaviour
{
    [SerializeField] private VisualEffect sparkParticles;
    
    private void Awake()
    {
        sparkParticles.Stop();
    }
    
    private void StartParticles()
    {
        sparkParticles.Play();
    }
}

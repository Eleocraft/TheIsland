using UnityEngine;
using System.Collections.Generic;

public class PlayerHealth : MonoSingleton<PlayerHealth>, IDamagable
{
    [SerializeField] private float MaxLife;
    [SerializeField] private IndicatorBar lifeBar;
    private float Life;
    void Start()
    {
        Life = MaxLife;
    }
    [Command]
    public static void Hit(List<string> args) => Instance.OnHit();
    public void OnHit()
    {
        Life--;
        lifeBar.AnimateProgress(Life / MaxLife);
    }
}

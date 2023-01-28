using UnityEngine;

public class Dummy : IDamagable
{
    public void OnHit()
    {
        Debug.Log("Hit");
    }
}

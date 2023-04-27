using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Blueprint : MonoBehaviour
{
    private List<Collider> collisions = new();
    public bool Blocked
    {
        get
        {
            collisions.RemoveAll(col => col == null);
            return collisions.Count > 0;
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        collisions.Add(col);
    }
    private void OnTriggerExit(Collider col)
    {
        collisions.Remove(col);
    }
}

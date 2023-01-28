using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField] private Transform clouds;

    void Update()
    {
        clouds.position = PlayerData.Position.WithHeight(clouds.position.y);
    }
}

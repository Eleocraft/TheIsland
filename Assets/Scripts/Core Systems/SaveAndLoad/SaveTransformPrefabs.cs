using UnityEngine;

public class SaveTransformPrefabs : MonoSingleton<SaveTransformPrefabs>
{
    [SerializeField] private GameObject[] savablePrefabs;
    public static GameObject[] SavablePrefabs => Instance?.savablePrefabs;
}

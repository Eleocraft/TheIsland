using UnityEngine;

public class SetShaderValue : MonoBehaviour
{
    [SerializeField] private Material Grass;
    void Start()
    {
    }

    void changeSetting(string setting, float value)
    {
        if (setting == "Grass Density")
        {
            Grass.SetFloat("_TessellationGrassFalloff", value / 10);
        }
    }
}

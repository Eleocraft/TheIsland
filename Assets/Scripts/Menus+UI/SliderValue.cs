using UnityEngine;
using UnityEngine.UI;
using TMPro;

[AddComponentMenu("UI/SliderValue", 33)]
[RequireComponent(typeof(Slider))]
public class SliderValue : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private string format;

    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(delegate { SetValue(); });
        text.text = slider.value.ToString(format);
    }

    public void SetValue()
    {
        text.text = slider.value.ToString(format);
    }
}

using UnityEngine;

public class MenuSounds : MonoBehaviour
{
    [SerializeField] private AudioClip click;
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip fail;
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void Click() => audioSource.PlayOneShot(click);
    public void Select() => audioSource.PlayOneShot(select);
    public void Fail() => audioSource.PlayOneShot(fail);
}

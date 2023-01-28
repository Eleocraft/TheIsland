using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private float Time;
    private void Start()
    {           
        this.Invoke(() => SceneManager.LoadScene(GlobalData.menuSceneName), Time);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
        SceneManager.LoadScene(2);
        SceneManager.LoadScene(3);
    }
}

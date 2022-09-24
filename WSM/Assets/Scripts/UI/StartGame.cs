using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public int startSceneToLoad;

    public void startGame()
    {
        SceneManager.LoadScene(startSceneToLoad);
    }
}

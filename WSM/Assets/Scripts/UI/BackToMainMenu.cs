using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    public int startSceneToLoad;

    public void backToMainMenu()
    {
        SceneManager.LoadScene(startSceneToLoad);
    }
}

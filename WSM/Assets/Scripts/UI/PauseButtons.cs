using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseButtons : MonoBehaviour
{
    public int mainMenuScene;

    private PauseController pauseCon;

    private void Start()
    {
        pauseCon = transform.parent.parent.GetComponent<PauseController>();
    }

    public void resume()
    {
        pauseCon.unPause();
    }

    public void backToMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}

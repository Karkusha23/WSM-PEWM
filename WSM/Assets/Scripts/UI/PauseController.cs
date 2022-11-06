using UnityEngine;

public class PauseController : MonoBehaviour
{
    private GameObject pauseScreen;
    private bool onPause;

    private void Start()
    {
        pauseScreen = transform.GetChild(0).gameObject;
        pauseScreen.SetActive(false);
        onPause = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (onPause)
            {
                unPause();
            }
            else
            {
                pause();
            }
        }
    }

    public void pause()
    {
        onPause = true;
        Time.timeScale = 0f;
        pauseScreen.SetActive(true);
    }

    public void unPause()
    {
        onPause = false;
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
    }
}

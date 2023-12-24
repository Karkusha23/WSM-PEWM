using UnityEngine;

public class PauseController : MonoBehaviour
{
    private GameObject pauseScreen;
    private CameraController camcon;
    private Player player;
    private bool onPause;

    private void Start()
    {
        pauseScreen = transform.GetChild(0).gameObject;
        pauseScreen.SetActive(false);
        camcon = Camera.main.GetComponent<CameraController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
        camcon.isAllowedToMove = false;
        player.lockWeapon();
    }

    public void unPause()
    {
        onPause = false;
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
        camcon.isAllowedToMove = true;
        player.unlockWeapon();
    }
}

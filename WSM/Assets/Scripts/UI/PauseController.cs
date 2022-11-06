using UnityEngine;

public class PauseController : MonoBehaviour
{
    private GameObject pauseScreen;
    private CameraController camcon;
    private PlayerController playerCon;
    private bool onPause;

    private void Start()
    {
        pauseScreen = transform.GetChild(0).gameObject;
        pauseScreen.SetActive(false);
        camcon = Camera.main.GetComponent<CameraController>();
        playerCon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
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
        playerCon.lockWeapon();
    }

    public void unPause()
    {
        onPause = false;
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
        camcon.isAllowedToMove = true;
        playerCon.unlockWeapon();
    }
}

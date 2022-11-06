using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public int sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        checkButton();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        checkButton();
    }

    private void checkButton()
    {
        if (Input.GetKey("e"))
        {
            PlayerData.health = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().health; 
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public int sceneToLoad;

    private PlayerController playerCon;

    private void Start()
    {
        playerCon = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

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
        if (Input.GetKey(KeyCode.E))
        {
            PlayerData.health = playerCon.health;
            PlayerData.hasWeapon = playerCon.hasWeapon;
            PlayerData.weaponID = playerCon.weaponID;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

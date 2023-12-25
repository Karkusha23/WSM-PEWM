using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public int sceneToLoad;

    private Player player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
            loadNextLevel();
        }
    }

    private void loadNextLevel()
    {
        PlayerData.health = player.health;
        PlayerData.weaponSample = player.weapon.GetComponent<PlayerWeapon>().weaponDroppedPrefab.GetComponent<WeaponDropped>().weaponPrefab;
        PlayerData.damage = player.damage;
        PlayerData.reloadTime = player.reloadTime;
        PlayerData.moveSpeed = player.moveSpeed;
        PlayerData.itemCounts = player.itemCounts;
        SceneManager.LoadScene(sceneToLoad);
    }
}

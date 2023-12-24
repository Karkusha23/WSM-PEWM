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
        PlayerData.hasWeapon = player.hasWeapon;
        PlayerData.weaponID = player.weaponID;
        PlayerData.damage = player.damage;
        PlayerData.reloadTime = player.reloadTime;
        PlayerData.moveSpeed = player.moveSpeed;
        PlayerData.damageItemCount = player.itemCounts[ItemController.Item.Damage];
        PlayerData.tearsItemCount = player.itemCounts[ItemController.Item.Tears];
        PlayerData.speedItemCount = player.itemCounts[ItemController.Item.Speed];
        SceneManager.LoadScene(sceneToLoad);
    }
}

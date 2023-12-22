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
            loadNextLevel();
        }
    }

    private void loadNextLevel()
    {
        PlayerData.health = playerCon.health;
        PlayerData.hasWeapon = playerCon.hasWeapon;
        PlayerData.weaponID = playerCon.weaponID;
        PlayerData.damage = playerCon.damage;
        PlayerData.reloadTime = playerCon.reloadTime;
        PlayerData.moveSpeed = playerCon.moveSpeed;
        PlayerData.damageItemCount = playerCon.itemCounts[ItemController.Item.Damage];
        PlayerData.tearsItemCount = playerCon.itemCounts[ItemController.Item.Tears];
        PlayerData.speedItemCount = playerCon.itemCounts[ItemController.Item.Speed];
        SceneManager.LoadScene(sceneToLoad);
    }
}

using UnityEngine;

public class RoomController : MonoBehaviour
{
    public EnemyLoadout loadout;
    public GameObject[] roomDrops;
    public GameObject[] doors;

    private int enemyCount;
    private bool isCleaned;
    private CameraController camcon;
    private bool isBig;

    private void Start()
    {
        isCleaned = false;
        camcon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        openDoors();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (CompareTag("SmallRoom"))
            {
                camcon.goToPos(transform.position);
            }
            else if (CompareTag("BigRoom"))
            {
                if (other.GetComponent<PlayerController>().hasWeapon)
                {
                    camcon.followMousePos();
                }
                else
                {
                    camcon.follow(other.gameObject);
                }
            }
            if (loadout != null && !isCleaned)
            {
                lockDoors();
                enemyCount = loadout.enemyPoss.Length;
                for (int i = enemyCount - 1; i >= 0; --i)
                {
                    Instantiate(loadout.enemyTypes[i], loadout.enemyPoss[i] + transform.position, Quaternion.identity, transform);
                }
            }
        }
    }

    public void checkEnemyKilled()
    {
        enemyCount -= 1;
        if (enemyCount <= 0)
        {
            openDoors();
            Instantiate(roomDrops[Random.Range(0, roomDrops.Length)], transform.position, Quaternion.identity);
            isCleaned = true;
        }
    }

    private void lockDoors()
    {
        foreach (var door in doors)
        {
            if (door != null)
            {
                door.SetActive(true);
            }
        }
    }

    private void openDoors()
    {
        foreach (var door in doors)
        {
            if (door != null)
            {
                door.SetActive(false);
            }
        }
    }
}

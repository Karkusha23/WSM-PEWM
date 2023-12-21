using UnityEngine;

public class RoomController : MonoBehaviour
{
    public EnemyLoadout loadout;
    public GameObject[] roomDrops;
    public GameObject[] doors;
    public float invincibleTime;

    private int enemyCount;
    private bool isActivated;
    private CameraController camcon;
    private Vector3 bigRoomOffset;
    private int roomType;

    private void Start()
    {
        bigRoomOffset = new Vector3(8.8f, -5.5f, 0f);
        if (CompareTag("SmallRoom"))
        {
            roomType = 0;
        }
        else if (CompareTag("BigRoom"))
        {
            roomType = 1;
        }
        else
        {
            roomType = 2;
        }
        isActivated = false;
        camcon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        openDoors();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomType == 0)
            {
                camcon.goToPos(transform.position);
            }
            else if (roomType == 1)
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
            if (loadout != null && !isActivated)
            {
                other.GetComponent<PlayerController>().setInvincible(invincibleTime);
                lockDoors();
                isActivated = true;
                enemyCount = loadout.enemyPoss.Length;
                if (roomType == 0)
                {
                    for (int i = enemyCount - 1; i >= 0; --i)
                    {
                        Instantiate(loadout.enemyTypes[i], loadout.enemyPoss[i] + transform.position, Quaternion.identity, transform);
                    }
                }
                else if (roomType == 1)
                {
                    for (int i = enemyCount - 1; i >= 0; --i)
                    {
                        Instantiate(loadout.enemyTypes[i], loadout.enemyPoss[i] + transform.position + bigRoomOffset, Quaternion.identity, transform);
                    }
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
            Instantiate(roomDrops[Random.Range(0, roomDrops.Length)], roomType == 1 ? transform.position + bigRoomOffset : transform.position, Quaternion.identity);
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

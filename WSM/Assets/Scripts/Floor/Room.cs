using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomLoadout loadout;
    public List<GameObject> roomDrops;
    public List<GameObject> doors;
    public float invincibleTime;
    public RoomPath.RoomType roomType;

    // Navigation grid for room
    public RoomPath.RoomGrid roomGrid;

    // Time between enemy spawning when entering room
    public const float timeBetweenEnemySpawn = 0.2f;

    private int enemyCount;
    private bool isActivated;
    private CameraController camcon;
    private Vector3 bigRoomOffset;

    private List<RoomLoadout.LoadoutUnit> enemyLoadout;

    private void Start()
    {
        bigRoomOffset = new Vector3(Floor.roomWidth / 2.0f, -Floor.roomHeight, 0f);
        if (CompareTag("SmallRoom"))
        {
            roomType = RoomPath.RoomType.SmallRoom;
        }
        else if (CompareTag("BigRoom"))
        {
            roomType = RoomPath.RoomType.BigRoom;
        }
        else
        {
            roomType = RoomPath.RoomType.None;
        }
        isActivated = false;
        camcon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        openDoors();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (roomType)
            {
                case RoomPath.RoomType.SmallRoom:
                    camcon.goToPos(transform.position);
                    break;
                case RoomPath.RoomType.BigRoom:
                    if (other.GetComponent<Player>().hasWeapon)
                    {
                        camcon.followMousePos();
                    }
                    else
                    {
                        camcon.follow(other.gameObject);
                    }
                    break;
                case RoomPath.RoomType.None:
                    break;
            }
            if (loadout != null && !isActivated)
            {
                activateEnemyRoom(other.gameObject);
            }
        }
    }

    // Get traveling cost for tile from prefab
    public static byte getTravelingCost(GameObject prefab)
    {
        if (prefab.CompareTag("Wall"))
        {
            return 0;
        }
        return RoomPath.defaultTravelCost;
    }

    public void spawnProps()
    {
        if (loadout == null || enemyLoadout != null)
        {
            return;
        }
        enemyLoadout = new List<RoomLoadout.LoadoutUnit>();
        roomGrid = new RoomPath.RoomGrid(roomType);
        foreach (var loadoutUnit in loadout.loadout)
        {
            if (loadoutUnit.prefab.CompareTag("Enemy"))
            {
                enemyLoadout.Add(loadoutUnit);
            }
            else
            {
                Instantiate(loadoutUnit.prefab, transform.position + RoomPath.RoomPointToLocal(loadoutUnit.row, loadoutUnit.col), Quaternion.identity, transform);
                roomGrid[loadoutUnit.row, loadoutUnit.col] = getTravelingCost(loadoutUnit.prefab);
            }
        }
    }

    public void activateEnemyRoom(GameObject player)
    {
        if (enemyLoadout == null)
        {
            spawnProps();
        }
        player.GetComponent<Player>().setInvincible(invincibleTime);
        lockDoors();
        isActivated = true;
        enemyCount = enemyLoadout.Count;
        StartCoroutine("spawnEnemies");
    }


    public void checkEnemyKilled()
    {
        enemyCount -= 1;
        if (enemyCount <= 0)
        {
            openDoors();
            Instantiate(roomDrops[Random.Range(0, roomDrops.Count)], roomType == RoomPath.RoomType.BigRoom ? transform.position + bigRoomOffset : transform.position, Quaternion.identity);
        }
    }

    private IEnumerator spawnEnemies()
    {
        foreach (var enemyLoadoutUnit in enemyLoadout)
        {
            Instantiate(enemyLoadoutUnit.prefab, transform.position + RoomPath.RoomPointToLocal(enemyLoadoutUnit.row, enemyLoadoutUnit.col), Quaternion.identity, transform);
            yield return new WaitForSeconds(timeBetweenEnemySpawn);
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

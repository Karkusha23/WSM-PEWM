using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public RoomLayout layout;
    public List<GameObject> roomDrops;
    public List<GameObject> doors;
    public float invincibleTime;
    public RoomPath.RoomType roomType;

    // Navigation grid for room
    public RoomPath.RoomGrid roomGrid;

    // Time between enemy spawning when entering room
    public const float timeBetweenEnemySpawn = 0.2f;

    public int tileHeight { get => roomType == RoomPath.RoomType.BigRoom ? RoomPath.bigRoomTileHeightCount : RoomPath.roomTileHeightCount; }

    public int tileWidth { get => roomType == RoomPath.RoomType.BigRoom ? RoomPath.bigRoomTileWidthCount : RoomPath.roomTileWidthCount; }

    private int enemyCount;
    private bool isPropsSpawned;
    private bool isActivated;
    private CameraController camcon;
    private Vector3 bigRoomOffset;

    private List<RoomLayout.ObjectPoint> enemyLayout;

    private void Start()
    {
        bigRoomOffset = new Vector3(Floor.roomWidth / 2.0f, -Floor.roomHeight, 0f);
        isPropsSpawned = false;
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
            if (layout != null && !isActivated)
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
        if (layout == null)
        {
            return;
        }
        roomGrid = new RoomPath.RoomGrid(roomType);
        foreach (var layoutUnit in layout.getProps())
        {
            Instantiate(layoutUnit.obj, transform.position + RoomPath.RoomPointToLocal(layoutUnit.point.row, layoutUnit.point.col), Quaternion.identity, transform);
            roomGrid[layoutUnit.point.row, layoutUnit.point.col] = getTravelingCost(layoutUnit.obj);
        }
        isPropsSpawned = true;
    }

    public void activateEnemyRoom(GameObject player)
    {
        if (!isPropsSpawned)
        {
            spawnProps();
        }
        player.GetComponent<Player>().setInvincible(invincibleTime);
        lockDoors();
        isActivated = true;
        enemyLayout = layout.getEnemies();
        enemyCount = enemyLayout.Count;
        StartCoroutine("spawnEnemies");
    }


    public void checkEnemyKilled()
    {
        enemyCount -= 1;
        if (enemyCount <= 0)
        {
            openDoors();
            spawnRoomReward();
        }
    }

    public void spawnRoomReward()
    {
        Instantiate(roomDrops[Random.Range(0, roomDrops.Count)], RoomPath.RoomPointToLocal(RoomPath.GetFreeCenterPoint(roomGrid)) + transform.position, Quaternion.identity);
    }

    private IEnumerator spawnEnemies()
    {
        foreach (var layoutUnit in enemyLayout)
        {
            Instantiate(layoutUnit.obj, transform.position + RoomPath.RoomPointToLocal(layoutUnit.point.row, layoutUnit.point.col), Quaternion.identity, transform);
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

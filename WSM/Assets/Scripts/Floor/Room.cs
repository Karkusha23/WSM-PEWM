using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomType { None, SmallRoom, BigRoom }

    public RoomLoadout loadout;
    public List<GameObject> roomDrops;
    public List<GameObject> doors;
    public float invincibleTime;

    // Tiling consts
    public const int roomTileWidthCount = 15;
    public const int roomTileHeightCount = 9;
    public const float tileSize = 1.2f;

    // Tile grid for enemy navigation. 0 if can not go through tile, otherwise value is traveling cost for tile
    public byte[,] roomGrid;

    // Deafult travel cost for normal tile
    public const int defaultTravelCost = 1;

    // Time between enemy spawning when entering room
    public const float timeBetweenEnemySpawn = 0.2f;

    private int enemyCount;
    private bool isActivated;
    private CameraController camcon;
    private Vector3 bigRoomOffset;
    private RoomType roomType;

    private List<RoomLoadout.LoadoutUnit> enemyLoadout;

    private void Start()
    {
        bigRoomOffset = new Vector3(Floor.roomWidth / 2.0f, -Floor.roomHeight, 0f);
        if (CompareTag("SmallRoom"))
        {
            roomType = RoomType.SmallRoom;
        }
        else if (CompareTag("BigRoom"))
        {
            roomType = RoomType.BigRoom;
        }
        else
        {
            roomType = RoomType.None;
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
                case RoomType.SmallRoom:
                    camcon.goToPos(transform.position);
                    break;
                case RoomType.BigRoom:
                    if (other.GetComponent<Player>().hasWeapon)
                    {
                        camcon.followMousePos();
                    }
                    else
                    {
                        camcon.follow(other.gameObject);
                    }
                    break;
                case RoomType.None:
                    break;
            }
            if (loadout != null && !isActivated)
            {
                activateEnemyRoom(other.gameObject);
                Debug.Log(pathToString(RoomPath.BuildPath(new RoomPath.RoomPoint(0, 3), new RoomPath.RoomPoint(5, 3), roomGrid)));
                Debug.Log(pathToString(RoomPath.BuildPath(new RoomPath.RoomPoint(8, 11), new RoomPath.RoomPoint(4, 11), roomGrid)));
            }
        }
    }

    // Return local coordinates of point on room grid
    public static Vector3 RoomPointToLocal(RoomPath.RoomPoint point)
    {
        return tileSize * new Vector3((point.j - roomTileWidthCount / 2), (roomTileHeightCount / 2 - point.i), 0.0f);
    }

    public static Vector3 RoomPointToLocal(int row, int col)
    {
        return tileSize * new Vector3((col - roomTileWidthCount / 2), (roomTileHeightCount / 2 - row), 0.0f);
    }

    // Returns room grid point from room local coordinates
    public static RoomPath.RoomPoint LocalToRoomPoint(Vector3 pos)
    {
        return new RoomPath.RoomPoint(roomTileHeightCount / 2 - Mathf.RoundToInt(pos.y / tileSize), roomTileWidthCount / 2 + Mathf.RoundToInt(pos.x / tileSize));
    }

    // Get traveling cost for tile from prefab
    public static byte getTravelingCost(GameObject prefab)
    {
        if (prefab.CompareTag("Wall"))
        {
            return 0;
        }
        return defaultTravelCost;
    }

    public void spawnProps()
    {
        if (loadout == null || enemyLoadout != null)
        {
            return;
        }
        enemyLoadout = new List<RoomLoadout.LoadoutUnit>();
        initRoomGrid();
        foreach (var loadoutUnit in loadout.loadout)
        {
            if (loadoutUnit.prefab.CompareTag("Enemy"))
            {
                enemyLoadout.Add(loadoutUnit);
            }
            else
            {
                Instantiate(loadoutUnit.prefab, transform.position + RoomPointToLocal(loadoutUnit.row, loadoutUnit.col), Quaternion.identity, transform);
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
            Instantiate(roomDrops[Random.Range(0, roomDrops.Count)], roomType == RoomType.BigRoom ? transform.position + bigRoomOffset : transform.position, Quaternion.identity);
        }
    }

    public string pathToString(RoomPath.Path path)
    {
        var tmpGrid = new byte[roomTileHeightCount, roomTileWidthCount];
        for (int i = 0; i < roomTileHeightCount; ++i)
        {
            for (int j = 0; j < roomTileWidthCount; ++j)
            {
                tmpGrid[i, j] = roomGrid[i, j];
            }
        }
        foreach (var point in path)
        {
            tmpGrid[point.i, point.j] = 2;
        }
        string result = "";
        for (int i = 0; i < roomTileHeightCount; ++i)
        {
            for (int j = 0; j < roomTileWidthCount; ++j)
            {
                result += tmpGrid[i, j];
            }
            result += '\n';
        }
        return result;
    }

    private void initRoomGrid()
    {
        roomGrid = new byte[roomTileHeightCount, roomTileWidthCount];
        for (int i = 0; i < roomTileHeightCount; ++i)
        {
            for (int j = 0; j < roomTileWidthCount; ++j)
            {
                roomGrid[i, j] = defaultTravelCost;
            }
        }
    }

    private IEnumerator spawnEnemies()
    {
        foreach (var enemyLoadoutUnit in enemyLoadout)
        {
            Instantiate(enemyLoadoutUnit.prefab, transform.position + RoomPointToLocal(enemyLoadoutUnit.row, enemyLoadoutUnit.col), Quaternion.identity, transform);
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

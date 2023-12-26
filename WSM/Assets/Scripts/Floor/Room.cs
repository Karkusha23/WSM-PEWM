using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static RoomLoadout;

public class Room : MonoBehaviour
{
    public enum RoomType { None, SmallRoom, BigRoom }

    public struct RoomPoint
    {
        public int i { get; set; }
        public int j { get; set; }

        public RoomPoint(int row, int col)
        {
            i = row;
            j = col;
        }
    }

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
            }
        }
    }

    // Return local coordinates of point on room grid
    public static Vector3 roomPointToLocal(int row, int col)
    {
        return tileSize * new Vector3((col - roomTileWidthCount / 2), (roomTileHeightCount / 2 - row), 0.0f);
    }

    // Returns room grid point from room local coordinates
    public static RoomPoint localToRoomPoint(Vector3 pos)
    {
        return new RoomPoint(roomTileHeightCount / 2 - Mathf.RoundToInt(pos.y / tileSize), roomTileWidthCount / 2 + Mathf.RoundToInt(pos.x / tileSize));
    }

    // Get traveling cost for tile from prefav
    public static byte getTravelingCost(GameObject prefab)
    {
        if (prefab.CompareTag("Wall"))
        {
            return 0;
        }
        return 1;
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
                Instantiate(loadoutUnit.prefab, transform.position + roomPointToLocal(loadoutUnit.row, loadoutUnit.col), Quaternion.identity, transform);
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
        foreach (var enemyLoadoutUnit in enemyLoadout)
        {
            Instantiate(enemyLoadoutUnit.prefab, transform.position + roomPointToLocal(enemyLoadoutUnit.row, enemyLoadoutUnit.col), Quaternion.identity, transform);
        }
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

    private void initRoomGrid()
    {
        roomGrid = new byte[roomTileHeightCount, roomTileWidthCount];
        for (int i = 0; i < roomTileHeightCount; ++i)
        {
            for (int j = 0; j < roomTileWidthCount; ++j)
            {
                roomGrid[i, j] = 1;
            }
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

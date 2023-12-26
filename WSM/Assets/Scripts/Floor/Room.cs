using System.Collections.Generic;
using UnityEngine;

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

    public const int roomTileWidthCount = 15;
    public const int roomTileHeightCount = 9;
    public const float tileWidth = (15.0f * 1.2f) / roomTileWidthCount;
    public const float tileHeight = (9.0f * 1.2f) / roomTileHeightCount;

    private int enemyCount;
    private bool isActivated;
    private CameraController camcon;
    private Vector3 bigRoomOffset;
    private RoomType roomType;

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
        return new Vector3((col - roomTileWidthCount / 2) * tileWidth, (row - roomTileHeightCount / 2) * tileHeight, 0.0f);
    }

    // Returns room grid point from room local coordinates
    public static RoomPoint localToRoomPoint(Vector3 pos)
    {
        return new RoomPoint(roomTileHeightCount / 2 + Mathf.RoundToInt(pos.y / tileHeight), roomTileWidthCount / 2 + Mathf.RoundToInt(pos.x / tileWidth));
    }

    public void activateEnemyRoom(GameObject player)
    {
        player.GetComponent<Player>().setInvincible(invincibleTime);
        lockDoors();
        isActivated = true;
        enemyCount = 0;
        for (int i = 0; i < loadout.prefabs.Length; ++i)
        {
            GameObject roomObject = Instantiate(loadout.prefabs[i], transform.position + roomPointToLocal(loadout.roomPointYs[i], loadout.roomPointXs[i]), Quaternion.identity, transform);
            if (roomObject.CompareTag("Enemy"))
            {
                ++enemyCount;
            }
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

using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    // Room count limitations
    public int minRoomCount = 10;
    public int maxRoomCount = 15;

    // Floor size limitations
    public int floorHeight = 10;
    public int floorWidth = 10;

    // Probability of big room creating (between [0, 1])
    public float bigRoomProbability = 0.15f;

    // Item room count limitation
    public int minItemRoomCount = 1;
    public int maxItemRoomCount = 3;

    // Prefabs of floor elements
    public GameObject smallRoomPrefab;
    public GameObject bigRoomPrefab;
    public GameObject doorPrefab;
    public GameObject wallPlugPrefab;

    // Room consts
    public const float roomHeight = 12.0f;
    public const float roomWidth = 19.2f;

    public RoomLoadout[] smallLoadouts;
    public RoomLoadout[] bigLoadouts;
    public RoomLoadout[] bossLoadouts;
    public List<GameObject> pickups;
    public List<GameObject> bossDrops;
    public ItemList itemList;

    private FloorGenerator.FloorGrid floorGrid;

    public FloorGenerator.FloorGrid FloorGrid { get => floorGrid; }

    private GameObject room;
    private bool[] smallDoors;
    private bool[] bigDoors;
    private Vector3[] doorPoss;
    private Vector3[] bigRoomDoorOffsets;

    private void Awake()
    {
        transform.position = Vector3.zero;
        initVectors();

        floorGrid = FloorGenerator.GenerateFloor(floorHeight, floorWidth, Random.Range(minRoomCount, maxRoomCount + 1), Random.Range(minItemRoomCount, maxItemRoomCount + 1), bigRoomProbability);

        for (int i = 0; i < floorGrid.rows; ++i)
        {
            for (int j = 0; j < floorGrid.cols; ++j)
            {
                switch (floorGrid[i, j])
                {
                    case FloorGenerator.RoomType.Small:
                        createSmallRoom(i, j, true);
                        break;
                    case FloorGenerator.RoomType.BigCore:
                        createBigRoom(i, j, false);
                        break;
                    case FloorGenerator.RoomType.Boss:
                        createBigRoom(i, j, true);
                        break;
                    case FloorGenerator.RoomType.PreBoss:
                        createSmallRoom(i, j, false);
                        break;
                    case FloorGenerator.RoomType.Item:
                        createItemRoom(i, j);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void initVectors()
    {
        smallDoors = new bool[4];
        bigDoors = new bool[8];
        doorPoss = new Vector3[4];
        doorPoss[0] = new Vector3(0f, roomHeight / 2.0f, 0.0f);
        doorPoss[1] = new Vector3(-roomWidth / 2.0f, 0.0f, 0.0f);
        doorPoss[2] = new Vector3(roomWidth / 2.0f, 0f, 0f);
        doorPoss[3] = new Vector3(0f, -roomHeight / 2.0f, 0f);
        bigRoomDoorOffsets = new Vector3[4];
        bigRoomDoorOffsets[0] = Vector3.zero;
        bigRoomDoorOffsets[1] = new Vector3(roomWidth, 0.0f, 0.0f);
        bigRoomDoorOffsets[2] = new Vector3(0.0f, -roomHeight, 0.0f);
        bigRoomDoorOffsets[3] = new Vector3(roomWidth, -roomHeight, 0.0f);
    }

    private void createSmallRoom(int row, int col, bool withEnemies)
    {
        Vector3 pos = floorMatrixToWorld(row, col);
        room = Instantiate(smallRoomPrefab, pos, Quaternion.identity, transform);
        getSmallRoomType(row, col);
        createSmallDoors();
        if (row == floorGrid.rows / 2 && col == floorGrid.cols / 2)
        {
            return;
        }
        if (withEnemies)
        {
            room.GetComponent<Room>().loadout = smallLoadouts[Random.Range(0, smallLoadouts.Length)];
            room.GetComponent<Room>().roomDrops = pickups;
            room.GetComponent<Room>().roomType = RoomPath.RoomType.SmallRoom;
            room.GetComponent<Room>().spawnProps();
        }
    }

    private void createItemRoom(int row, int col)
    {
        createSmallRoom(row, col, false);
        GameObject item = itemList.items[Random.Range(0, itemList.items.Length)];
        Instantiate(item, room.transform.position, Quaternion.identity);
        room.transform.Find("Floor").GetComponent<SpriteRenderer>().color = new Color(217.0f / 256.0f, 212.0f / 256.0f, 105.0f / 256.0f, 1.0f);
    }

    private void createBigRoom(int row, int col, bool isBoss)
    {
        Vector3 pos = floorMatrixToWorld(row, col);
        room = Instantiate(bigRoomPrefab, pos, Quaternion.identity, transform);
        getBigRoomType(row, col);
        createBigDoors();
        if (isBoss)
        {
            room.GetComponent<Room>().loadout = bossLoadouts[Random.Range(0, bossLoadouts.Length)];
            room.GetComponent<Room>().roomDrops = bossDrops;
        }
        else
        {
            room.GetComponent<Room>().loadout = bigLoadouts[Random.Range(0, bigLoadouts.Length)];
            room.GetComponent<Room>().roomDrops = pickups;
        }
        room.GetComponent<Room>().roomType = RoomPath.RoomType.BigRoom;
        room.GetComponent<Room>().spawnProps();
    }

    private void getSmallRoomType(int row, int col)
    {
        smallDoors[0] = floorGrid.isRoomOccupied(row - 1, col);
        smallDoors[1] = floorGrid.isRoomOccupied(row, col - 1);
        smallDoors[2] = floorGrid.isRoomOccupied(row, col + 1);
        smallDoors[3] = floorGrid.isRoomOccupied(row + 1, col);
    }

    private void getBigRoomType(int row, int col)
    {
        bigDoors[0] = floorGrid.isRoomOccupied(row - 1, col);
        bigDoors[1] = floorGrid.isRoomOccupied(row - 1, col + 1);
        bigDoors[2] = floorGrid.isRoomOccupied(row, col - 1);
        bigDoors[3] = floorGrid.isRoomOccupied(row, col + 2);
        bigDoors[4] = floorGrid.isRoomOccupied(row + 1, col - 1);
        bigDoors[5] = floorGrid.isRoomOccupied(row + 1, col + 2);
        bigDoors[6] = floorGrid.isRoomOccupied(row + 2, col);
        bigDoors[7] = floorGrid.isRoomOccupied(row + 2, col + 1);
    }

    private void createSmallDoors()
    {
        GameObject tmp;
        Room roomcon = room.GetComponent<Room>();
        for (int i = 0; i < 4; ++i)
        {
            tmp = Instantiate(smallDoors[i] ? doorPrefab : wallPlugPrefab, room.transform.position + doorPoss[i], i == 0 || i == 3 ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f), room.transform);
            if (smallDoors[i])
            {
                roomcon.doors.Add(tmp);
            }
        }
    }

    private void createBigDoors()
    {
        GameObject tmp;
        Room roomcon = room.GetComponent<Room>();
        for (int i = 0; i < 8; ++i)
        {
            tmp = Instantiate(bigDoors[i] ? doorPrefab : wallPlugPrefab, room.transform);
            tmp.transform.position = room.transform.position;
            if (i < 4)
            {
                tmp.transform.position += bigRoomDoorOffsets[i % 2];
            }
            else
            {
                tmp.transform.position += bigRoomDoorOffsets[(i % 2) + 2];
            }
            if (i < 2)
            {
                tmp.transform.position += doorPoss[0];
            }
            else if (i < 6 && i % 2 == 0)
            {
                tmp.transform.position += doorPoss[1];
            }
            else if (i < 6 && i % 2 == 1)
            {
                tmp.transform.position += doorPoss[2];
            }
            else
            {
                tmp.transform.position += doorPoss[3];
            }
            if (i >= 2 && i <= 5)
            {
                tmp.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            if (bigDoors[i])
            {
                roomcon.doors.Add(tmp);
            }
        }
    }

    public Vector3 floorMatrixToWorld(int row, int col)
    {
        return new Vector3((col - floorGrid.cols / 2) * roomWidth, (floorGrid.rows / 2 - row) * roomHeight, 0.0f);
    }
}

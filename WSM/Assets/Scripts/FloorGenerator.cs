using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public int minRoomCount;
    public int maxRoomCount;
    public int floorHeight;
    public int floorWidth;

    public GameObject smallRoom;
    public GameObject bigRoom;
    public GameObject door;
    public GameObject wallPlug;

    public EnemyLoadout[] loadouts;
    public WeaponsList weapons;
    public GameObject[] pickups;

    private int[,] floorMatrix;
    private int[] freeRoomHeights;
    private int[] freeRoomWidths;
    private int freeRoomCount;
    private GameObject room;
    private bool[] smallDoors;
    private Vector3[] doorPoss;
    private Vector3[] bigRoomDoorOffsets;

    private void Awake()
    {
        smallDoors = new bool[4];
        doorPoss = new Vector3[4];
        doorPoss[0] = new Vector3(0f, 5f, 0f);
        doorPoss[1] = new Vector3(-8f, 0f, 0f);
        doorPoss[2] = new Vector3(8f, 0f, 0f);
        doorPoss[3] = new Vector3(0f, -5f, 0f);
        for (int i = 0; i < 4; ++i)
        {
            doorPoss[i] *= 1.1f;
        }

        bigRoomDoorOffsets = new Vector3[3];
        bigRoomDoorOffsets[0] = new Vector3(17.6f, 0f, 0f);
        bigRoomDoorOffsets[1] = new Vector3(0, -11f, 0f);
        bigRoomDoorOffsets[2] = new Vector3(17.6f, -11f, 0f);

        buildMatrix();
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorMatrix[i, j] == 2)
                {
                    createRoom(i, j);
                }
            }
        }
    }

    private void buildMatrix()
    {
        init();
        int count = Random.Range(minRoomCount - 1, maxRoomCount);
        for (int i = 0; i < count; ++i)
        {
            buildRandomRoom();
        }
    }

    private void init()
    {
        floorMatrix = new int[floorHeight, floorWidth];
        freeRoomHeights = new int[floorHeight * floorWidth];
        freeRoomWidths = new int[floorWidth * floorWidth];
        freeRoomCount = 0;
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                floorMatrix[i, j] = 0;
            }
        }
        floorMatrix[floorHeight / 2, floorWidth / 2] = 2;
        checkFreeRoom(floorHeight / 2, floorWidth / 2);
    }

    private void buildRandomRoom()
    {
        int index = Random.Range(0, freeRoomCount);
        floorMatrix[freeRoomHeights[index], freeRoomWidths[index]] = 2;
        checkFreeRoom(freeRoomHeights[index], freeRoomWidths[index]);
        removeFreeRoom(index);
    }

    private void checkFreeRoom(int row, int col)
    {
        if (row > 0)
        {
            checkCell(row - 1, col);
        }
        if (col > 0)
        {
            checkCell(row, col - 1);
        }
        if (row < floorHeight - 1)
        {
            checkCell(row + 1, col);
        }
        if (col < floorWidth - 1)
        {
            checkCell(row, col + 1);
        }
    }

    private void checkCell(int row, int col)
    {
        if (floorMatrix[row, col] == 0)
        {
            floorMatrix[row, col] = 1;
            freeRoomHeights[freeRoomCount] = row;
            freeRoomWidths[freeRoomCount] = col;
            ++freeRoomCount;
        }
    }

    private void removeFreeRoom(int index)
    {
        freeRoomHeights[index] = freeRoomHeights[freeRoomCount - 1];
        freeRoomWidths[index] = freeRoomWidths[freeRoomCount - 1];
        --freeRoomCount;
    }

    private void createRoom(int row, int col)
    {
        Vector3 pos = new Vector3((col - floorWidth / 2) * 17.6f, (floorHeight / 2 - row) * 11f, 0f);
        room = Instantiate(smallRoom, pos, Quaternion.identity);
        getRoomType(row, col);
        createDoors();
        if (row == floorHeight / 2 && col == floorWidth / 2)
        {
            Instantiate(weapons.weapons_dropped[0], room.transform.position + new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        }
        else
        {
            room.GetComponent<RoomController>().loadout = loadouts[Random.Range(0, loadouts.Length)];
            room.GetComponent<RoomController>().roomDrops = pickups;
        }
    }

    private void getRoomType(int row, int col)
    {
        if (row > 0)
        {
            smallDoors[0] = floorMatrix[row - 1, col] == 2;
        }
        else
        {
            smallDoors[0] = false;
        }
        if (col > 0)
        {
            smallDoors[1] = floorMatrix[row, col - 1] == 2;
        }
        else
        {
            smallDoors[1] = false;
        }
        if (row < floorHeight - 1)
        {
            smallDoors[3] = floorMatrix[row + 1, col] == 2;
        }
        else
        {
            smallDoors[3] = false;
        }
        if (col < floorWidth - 1)
        {
            smallDoors[2] = floorMatrix[row, col + 1] == 2;
        }
        else
        {
            smallDoors[0] = false;
        }
    }

    private void createDoors()
    {
        GameObject tmp;
        RoomController roomcon = room.GetComponent<RoomController>();
        roomcon.doors = new GameObject[4];
        int counter = 0;
        for (int i = 0; i < 4; ++i)
        {
            tmp = Instantiate(smallDoors[i] ? door : wallPlug, room.transform.position + doorPoss[i], i == 0 || i == 3 ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f), room.transform);
            if (smallDoors[i])
            {
                roomcon.doors[counter] = tmp;
                ++counter;
            }
        }
    }
}

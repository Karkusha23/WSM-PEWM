using UnityEditor;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public int minRoomCount;
    public int maxRoomCount;
    public int floorHeight;
    public int floorWidth;
    public float bigRoomProbability;

    public GameObject smallRoom;
    public GameObject bigRoom;
    public GameObject door;
    public GameObject wallPlug;

    public EnemyLoadout[] smallLoadouts;
    public EnemyLoadout[] bigLoadouts;
    public WeaponsList weapons;
    public GameObject[] pickups;

    private int[,] floorMatrix;
    // 0 - no room, can not build there
    // 1 - no room, free only for small room
    // 2 - no room, free only for big room
    // 3 - no room, free for small and big room
    // 4 - small room
    // 5 - big room core
    // 6 - big room subunits
    private int[] freeSmallRoomHeights;
    private int[] freeSmallRoomWidths;
    private int freeSmallRoomCount;
    private int[] freeBigRoomHeights;
    private int[] freeBigRoomWidths;
    private int freeBigRoomCount;
    private GameObject room;
    private bool[] smallDoors;
    private bool[] bigDoors;
    private Vector3[] doorPoss;
    private Vector3[] bigRoomDoorOffsets;

    private void Awake()
    {
        initVectors();
        buildMatrix();
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorMatrix[i, j] == 4)
                {
                    createSmallRoom(i, j);
                }
                if (floorMatrix[i, j] == 5)
                {
                    createBigRoom(i, j);
                }
            }
        }
        createBossRoom(); // TODO
    }

    private void initVectors()
    {
        smallDoors = new bool[4];
        bigDoors = new bool[8];
        doorPoss = new Vector3[4];
        doorPoss[0] = new Vector3(0f, 5f, 0f);
        doorPoss[1] = new Vector3(-8f, 0f, 0f);
        doorPoss[2] = new Vector3(8f, 0f, 0f);
        doorPoss[3] = new Vector3(0f, -5f, 0f);
        for (int i = 0; i < 4; ++i)
        {
            doorPoss[i] *= 1.1f;
        }
        bigRoomDoorOffsets = new Vector3[4];
        bigRoomDoorOffsets[0] = Vector3.zero;
        bigRoomDoorOffsets[1] = new Vector3(17.6f, 0f, 0f);
        bigRoomDoorOffsets[2] = new Vector3(0, -11f, 0f);
        bigRoomDoorOffsets[3] = new Vector3(17.6f, -11f, 0f);
    }

    private void buildMatrix()
    {
        initMatrix();
        int count = Random.Range(minRoomCount - 1, maxRoomCount);
        bool indicator = true;
        for (int i = 0; i < count; ++i)
        {
            indicator = buildRandomRoom();
            if (!indicator)
            {
                break;
            }
        }
    }

    private void initMatrix()
    {
        floorMatrix = new int[floorHeight, floorWidth];
        freeSmallRoomHeights = new int[floorHeight * floorWidth];
        freeSmallRoomWidths = new int[floorWidth * floorWidth];
        freeSmallRoomCount = 0;
        freeBigRoomHeights = new int[floorHeight * floorWidth];
        freeBigRoomWidths = new int[floorWidth * floorWidth];
        freeBigRoomCount = 0;
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                floorMatrix[i, j] = 0;
            }
        }
        floorMatrix[floorHeight / 2, floorWidth / 2] = 4;
        checkSmallFreeRoom(floorHeight / 2, floorWidth / 2);
    }

    private bool buildRandomRoom()
    {
        if (Random.value <= bigRoomProbability && freeBigRoomCount > 0)
        {
            int index = Random.Range(0, freeBigRoomCount);
            checkBigFreeRoom(freeBigRoomHeights[index], freeBigRoomWidths[index]);
            removeFreeBigAround(freeBigRoomHeights[index], freeBigRoomWidths[index]);
            floorMatrix[freeBigRoomHeights[index], freeBigRoomWidths[index]] = 5;
            for (int i = 1; i < 4; ++i)
            {
                floorMatrix[freeBigRoomHeights[index] + i / 2, freeBigRoomWidths[index] + i % 2] = 6;
            }
            removeFreeBigRoom(index);
        }
        else
        {
            if (freeSmallRoomCount == 0)
            {
                return false;
            }
            int index = Random.Range(0, freeSmallRoomCount);
            checkSmallFreeRoom(freeSmallRoomHeights[index], freeSmallRoomWidths[index]);
            removeFreeSmallAround(freeSmallRoomHeights[index], freeSmallRoomWidths[index]);
            floorMatrix[freeSmallRoomHeights[index], freeSmallRoomWidths[index]] = 4;
            removeFreeSmallRoom(index);
        }
        return true;
    }

    private void checkSmallFreeRoom(int row, int col)
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
        if (row > 1)
        {
            if (col > 0)
            {
                checkBigCell(row - 2, col - 1);
            }
            if (col < floorWidth - 1)
            {
                checkBigCell(row - 2, col);
            }
        }
        if (col > 1)
        {
            if (row > 0)
            {
                checkBigCell(row - 1, col - 2);
            }
            if (row < floorHeight - 1)
            {
                checkBigCell(row, col - 2);
            }
        }
        if (row < floorHeight - 2)
        {
            if (col > 0)
            {
                checkBigCell(row + 1, col - 1);
            }
            if (col < floorWidth - 1)
            {
                checkBigCell(row + 1, col);
            }
        }
        if (col < floorWidth - 2)
        {
            if (row > 0)
            {
                checkBigCell(row - 1, col + 1);
            }
            if (row < floorHeight - 1)
            {
                checkBigCell(row, col + 1);
            }
        }
    }

    private void checkBigFreeRoom(int row, int col)
    {
        if (row > 0)
        {
            checkCell(row - 1, col);
            checkCell(row - 1, col + 1);
        }
        if (col > 0)
        {
            checkCell(row, col - 1);
            checkCell(row + 1, col - 1);
        }
        if (row < floorHeight - 2)
        {
            checkCell(row + 2, col);
            checkCell(row + 2, col + 1);
        }
        if (col < floorWidth - 2)
        {
            checkCell(row, col + 2);
            checkCell(row + 1, col + 2);
        }
        if (row > 1)
        {
            checkBigCell(row - 2, col);
            if (col > 0)
            {
                checkBigCell(row - 2, col - 1);
            }
            if (col < floorWidth - 1)
            {
                checkBigCell(row - 2, col + 1);
            }
        }
        if (col > 1)
        {
            checkBigCell(row, col - 2);
            if (row > 0)
            {
                checkBigCell(row - 1, col - 2);
            }
            if (row < floorHeight - 1)
            {
                checkBigCell(row + 1, col - 2);
            }
        }
        if (row < floorHeight - 2)
        {
            checkBigCell(row + 2, col);
            if (col > 0)
            {
                checkBigCell(row + 2, col - 1);
            }
            if (col < floorWidth - 1)
            {
                checkBigCell(row + 2, col + 1);
            }
        }
        if (col < floorWidth - 2)
        {
            checkBigCell(row, col + 2);
            if (row > 0)
            {
                checkBigCell(row - 1, col + 2);
            }
            if (row < floorHeight - 1)
            {
                checkBigCell(row + 1, col + 2);
            }
        }
    }

    private void checkCell(int row, int col)
    {
        if (floorMatrix[row, col] == 0 || floorMatrix[row, col] == 2)
        {
            floorMatrix[row, col] = floorMatrix[row, col] == 0 ? 1 : 3;
            freeSmallRoomHeights[freeSmallRoomCount] = row;
            freeSmallRoomWidths[freeSmallRoomCount] = col;
            ++freeSmallRoomCount;
        }
    }

    private void checkBigCell(int row, int col)
    {
        if (floorMatrix[row, col] <= 1 && row < floorHeight - 2 && col < floorWidth - 2)
        {
            bool tmp = true;
            for (int i = 1; i < 4; ++i)
            {
                if (floorMatrix[row + i / 2, col + i % 2] > 3)
                {
                    tmp = false;
                    break;
                }
            }
            if (tmp)
            {
                floorMatrix[row, col] = floorMatrix[row, col] == 0 ? 2 : 3;
                freeBigRoomHeights[freeBigRoomCount] = row;
                freeBigRoomWidths[freeBigRoomCount] = col;
                ++freeBigRoomCount;
            }
        }
    }

    private int findSmallFreeRoom(int row, int col)
    {
        for (int i = 0; i < freeSmallRoomCount; ++i)
        {
            if (freeSmallRoomHeights[i] == row && freeSmallRoomWidths[i] == col)
            {
                return i;
            }
        }
        return -1;
    }

    private int findBigFreeRoom(int row, int col)
    {
        for (int i = 0; i < freeBigRoomCount; ++i)
        {
            if (freeBigRoomHeights[i] == row && freeBigRoomWidths[i] == col)
            {
                return i;
            }
        }
        return -1;
    }

    private void removeFreeSmallAround(int row, int col)
    {
        if (floorMatrix[row, col] == 3)
        {
            removeFreeBigRoom(findBigFreeRoom(row, col));
            floorMatrix[row, col] = 1;
        }
        if (row > 0 && col > 0)
        {
            if (floorMatrix[row - 1, col - 1] == 2 || floorMatrix[row - 1, col - 1] == 3)
            {
                removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
                floorMatrix[row - 1, col - 1] = floorMatrix[row - 1, col - 1] == 2 ? 0 : 1;
            }
        }
        if (row > 0)
        {
            if (floorMatrix[row - 1, col] == 2 || floorMatrix[row - 1, col] == 3)
            {
                removeFreeBigRoom(findBigFreeRoom(row - 1, col));
                floorMatrix[row - 1, col] = floorMatrix[row - 1, col] == 2 ? 0 : 1;
            }
        }
        if (col > 0)
        {
            if (floorMatrix[row, col - 1] == 2 || floorMatrix[row, col - 1] == 3)
            {
                removeFreeBigRoom(findBigFreeRoom(row, col - 1));
                floorMatrix[row, col - 1] = floorMatrix[row, col - 1] == 2 ? 0 : 1;
            }
        }
    }

    private void removeFreeBigAround(int row, int col)
    {
        if (floorMatrix[row, col] == 3)
        {
            removeFreeSmallRoom(findSmallFreeRoom(row, col));
            floorMatrix[row, col] = 2;
        }
        for (int i = 1; i < 4; ++i)
        {
            int curRow = row + i / 2;
            int curCol = col + i % 2;
            switch(floorMatrix[curRow, curCol])
            {
                case 1:
                    removeFreeSmallRoom(findSmallFreeRoom(curRow, curCol));
                    break;
                case 2:
                    removeFreeBigRoom(findBigFreeRoom(curRow, curCol));
                    break;
                case 3:
                    removeFreeSmallRoom(findSmallFreeRoom(curRow, curCol));
                    removeFreeBigRoom(findBigFreeRoom(curRow, curCol));
                    break;
            }
        }
        if (col > 0)
        {
            for (int curRow = row; curRow <= row + 1; ++curRow)
            {
                switch(floorMatrix[curRow, col - 1])
                {
                    case 2:
                        removeFreeBigRoom(findBigFreeRoom(curRow, col - 1));
                        floorMatrix[curRow, col - 1] = 0;
                        break;
                    case 3:
                        removeFreeBigRoom(findBigFreeRoom(curRow, col - 1));
                        floorMatrix[curRow, col - 1] = 1;
                        break;
                }
            }
            if (row > 0)
            {
                switch(floorMatrix[row - 1, col - 1])
                {
                    case 2:
                        removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
                        floorMatrix[row - 1, col - 1] = 0;
                        break;
                    case 3:
                        removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
                        floorMatrix[row - 1, col - 1] = 1;
                        break;
                }
            }
        }
        if (row > 0)
        {
            for (int curCol = col; curCol <= col + 1; ++curCol)
            {
                switch (floorMatrix[row - 1, curCol])
                {
                    case 2:
                        removeFreeBigRoom(findBigFreeRoom(row - 1, curCol));
                        floorMatrix[row - 1, curCol] = 0;
                        break;
                    case 3:
                        removeFreeBigRoom(findBigFreeRoom(row - 1, curCol));
                        floorMatrix[row - 1, curCol] = 1;
                        break;
                }
            }
        }
    }

    private void removeFreeSmallRoom(int index)
    {
        freeSmallRoomHeights[index] = freeSmallRoomHeights[freeSmallRoomCount - 1];
        freeSmallRoomWidths[index] = freeSmallRoomWidths[freeSmallRoomCount - 1];
        --freeSmallRoomCount;
    }

    private void removeFreeBigRoom(int index)
    {
        if (index < 0)
        {
            return;
        }
        freeBigRoomHeights[index] = freeBigRoomHeights[freeBigRoomCount - 1];
        freeBigRoomWidths[index] = freeBigRoomWidths[freeBigRoomCount - 1];
        --freeBigRoomCount;
    }

    private void createSmallRoom(int row, int col)
    {
        Vector3 pos = new Vector3((col - floorWidth / 2) * 17.6f, (floorHeight / 2 - row) * 11f, 0f);
        room = Instantiate(smallRoom, pos, Quaternion.identity);
        getSmallRoomType(row, col);
        createSmallDoors();
        if (row == floorHeight / 2 && col == floorWidth / 2)
        {
            Instantiate(weapons.weapons_dropped[0], room.transform.position + new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        }
        else
        {
            room.GetComponent<RoomController>().loadout = smallLoadouts[Random.Range(0, smallLoadouts.Length)];
            room.GetComponent<RoomController>().roomDrops = pickups;
        }
    }

    private void createBigRoom(int row, int col)
    {
        Vector3 pos = new Vector3((col - floorWidth / 2) * 17.6f, (floorHeight / 2 - row) * 11f, 0f);
        room = Instantiate(bigRoom, pos, Quaternion.identity);
        getBigRoomType(row, col);
        createBigDoors();
        room.GetComponent<RoomController>().loadout = bigLoadouts[Random.Range(0, bigLoadouts.Length)];
        room.GetComponent<RoomController>().roomDrops = pickups;
    }

    private void getSmallRoomType(int row, int col)
    {
        if (row > 0)
        {
            smallDoors[0] = floorMatrix[row - 1, col] >= 4;
        }
        else
        {
            smallDoors[0] = false;
        }
        if (col > 0)
        {
            smallDoors[1] = floorMatrix[row, col - 1] >= 4;
        }
        else
        {
            smallDoors[1] = false;
        }
        if (row < floorHeight - 1)
        {
            smallDoors[3] = floorMatrix[row + 1, col] >= 4;
        }
        else
        {
            smallDoors[3] = false;
        }
        if (col < floorWidth - 1)
        {
            smallDoors[2] = floorMatrix[row, col + 1] >= 4;
        }
        else
        {
            smallDoors[2] = false;
        }
    }

    private void getBigRoomType(int row, int col)
    {
        if (row > 0)
        {
            bigDoors[0] = floorMatrix[row - 1, col] >= 4;
            bigDoors[1] = floorMatrix[row - 1, col + 1] >= 4;
        }
        else
        {
            bigDoors[0] = false;
            bigDoors[1] = false;
        }
        if (col > 0)
        {
            bigDoors[2] = floorMatrix[row, col - 1] >= 4;
            bigDoors[4] = floorMatrix[row + 1, col - 1] >= 4;
        }
        else
        {
            bigDoors[2] = false;
            bigDoors[4] = false;
        }
        if (row < floorHeight - 2)
        {
            bigDoors[6] = floorMatrix[row + 2, col] >= 4;
            bigDoors[7] = floorMatrix[row + 2, col + 1] >= 4;
        }
        else
        {
            bigDoors[6] = false;
            bigDoors[7] = false;
        }
        if (col < floorWidth - 2)
        {
            bigDoors[3] = floorMatrix[row, col + 2] >= 4;
            bigDoors[5] = floorMatrix[row + 1, col + 2] >= 4;
        }
        else
        {
            bigDoors[3] = false;
            bigDoors[5] = false;
        }
    }

    private void createSmallDoors()
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
                roomcon.doors[counter++] = tmp;
            }
        }
    }

    private void createBigDoors()
    {
        GameObject tmp;
        RoomController roomcon = room.GetComponent<RoomController>();
        roomcon.doors = new GameObject[8];
        int counter = 0;
        for (int i = 0; i < 8; ++i)
        {
            tmp = Instantiate(bigDoors[i] ? door : wallPlug, room.transform);
            tmp.transform.position = room.transform.position;
            tmp.transform.localScale = new Vector3(4.4f, 1.1f, 1f);
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
                roomcon.doors[counter++] = tmp;
            }
        }
    }

    private void createBossRoom()
    {
        //TODO
    }
}

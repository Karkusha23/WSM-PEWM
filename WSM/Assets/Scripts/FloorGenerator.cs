using System.Collections.Generic;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    // Struct for storing points on floor matrix
    public struct FloorPoint
    {
        public int i { get; set; }
        public int j { get; set; }

        public FloorPoint(int row, int col)
        {
            i = row;
            j = col;
        }
    };

    public class PointList : List<FloorPoint>
    {
        // Remove point by coordinates
        public bool RemoveByPoint(int row, int col)
        {
            int index = IndexOf(new FloorPoint(row, col));
            if (index < 0)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }
    };

    // Room count limitations
    public int minRoomCount;
    public int maxRoomCount;

    // Floor size limitations
    public int floorHeight;
    public int floorWidth;

    // Probability of big room creating (between [0, 1])
    public float bigRoomProbability;

    // Item room count limitation
    public int minItemRoomCount;
    public int maxItemRoomCount;

    // Prefabs of floor elements
    public GameObject smallRoomPrefab;
    public GameObject bigRoomPrefab;
    public GameObject doorPrefab;
    public GameObject wallPlugPrefab;

    public EnemyLoadout[] smallLoadouts;
    public EnemyLoadout[] bigLoadouts;
    public EnemyLoadout[] bossLoadouts;
    public GameObject[] pickups;
    public GameObject[] bossDrops;
    public ItemList itemList;

    private int[,] floorMatrix;
    // 0 - no room, can not build there
    // 1 - no room, free only for small room
    // 2 - no room, free only for big room
    // 3 - no room, free for small and big room
    // 4 - small room
    // 5 - big room core
    // 6 - big room subunits
    // 7 - boss room core
    // 8 - pre-boss room
    // 9 - item room

    // Lists of coordinates of possible rooms
    private PointList freeSmallRoom;
    private PointList freeBigRoom;

    private GameObject room;
    private bool[] smallDoors;
    private bool[] bigDoors;
    private Vector3[] doorPoss;
    private Vector3[] bigRoomDoorOffsets;
    private Minimap minimap;

    private void Awake()
    {
        initVectors();
        buildMatrix();
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                switch (floorMatrix[i, j])
                {
                    case 4:
                        createSmallRoom(i, j, true);
                        break;
                    case 5:
                        createBigRoom(i, j, false);
                        break;
                    case 7:
                        createBigRoom(i, j, true);
                        break;
                    case 8:
                        createSmallRoom(i, j, false);
                        break;
                    case 9:
                        createItemRoom(i, j);
                        break;
                }
            }
        }
        minimap = GameObject.FindGameObjectWithTag("Minimap").GetComponent<Minimap>();
        minimap.floorMatrix = floorMatrix;
        minimap.floorHeight = floorHeight;
        minimap.floorWidth = floorWidth;
        minimap.init();
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
        //Debug.Log(FloorMatrixStr());
        buildItemRooms();
        buildBossRoom();
    }

    private void initMatrix()
    {
        floorMatrix = new int[floorHeight, floorWidth];
        freeSmallRoom = new PointList();
        freeBigRoom = new PointList();
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
        if (Random.value <= bigRoomProbability && freeBigRoom.Count > 0)
        {
            int index = Random.Range(0, freeBigRoom.Count);
            int row = freeBigRoom[index].i;
            int col = freeBigRoom[index].j;
            checkBigFreeRoom(row, col);
            removeFreeBigAround(row, col);
            floorMatrix[row, col] = 5;
            for (int i = 1; i < 4; ++i)
            {
                floorMatrix[row + i / 2, col + i % 2] = 6;
            }
            freeBigRoom.RemoveByPoint(row, col);
            //removeFreeBigRoom(index);
        }
        else
        {
            if (freeSmallRoom.Count == 0)
            {
                return false;
            }
            int index = Random.Range(0, freeSmallRoom.Count);
            int row = freeSmallRoom[index].i;
            int col = freeSmallRoom[index].j;
            checkSmallFreeRoom(row, col);
            removeFreeSmallAround(row, col);
            floorMatrix[row, col] = 4;
            freeSmallRoom.RemoveByPoint(row, col);
            //removeFreeSmallRoom(index);
        }
        Debug.Log(FloorMatrixStr());
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
        Debug.Log(row.ToString() + " " + col);
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
            freeSmallRoom.Add(new FloorPoint(row, col));
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
                freeBigRoom.Add(new FloorPoint(row, col));
            }
        }
    }

    private void removeFreeSmallAround(int row, int col)
    {
        if (floorMatrix[row, col] == 3)
        {
            freeBigRoom.RemoveByPoint(row, col);
            floorMatrix[row, col] = 1;
        }
        if (row > 0 && col > 0)
        {
            if (floorMatrix[row - 1, col - 1] == 2 || floorMatrix[row - 1, col - 1] == 3)
            {
                freeBigRoom.RemoveByPoint(row - 1, col - 1);
                //removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
                floorMatrix[row - 1, col - 1] = floorMatrix[row - 1, col - 1] == 2 ? 0 : 1;
            }
        }
        if (row > 0)
        {
            if (floorMatrix[row - 1, col] == 2 || floorMatrix[row - 1, col] == 3)
            {
                freeBigRoom.RemoveByPoint(row - 1, col);
                //removeFreeBigRoom(findBigFreeRoom(row - 1, col));
                floorMatrix[row - 1, col] = floorMatrix[row - 1, col] == 2 ? 0 : 1;
            }
        }
        if (col > 0)
        {
            if (floorMatrix[row, col - 1] == 2 || floorMatrix[row, col - 1] == 3)
            {
                freeBigRoom.RemoveByPoint(row, col - 1);
                //removeFreeBigRoom(findBigFreeRoom(row, col - 1));
                floorMatrix[row, col - 1] = floorMatrix[row, col - 1] == 2 ? 0 : 1;
            }
        }
    }

    private void removeFreeBigAround(int row, int col)
    {
        if (floorMatrix[row, col] == 3)
        {
            freeSmallRoom.RemoveByPoint(row, col);
            floorMatrix[row, col] = 2;
        }
        for (int i = 1; i < 4; ++i)
        {
            int curRow = row + i / 2;
            int curCol = col + i % 2;
            switch(floorMatrix[curRow, curCol])
            {
                case 1:
                    freeSmallRoom.RemoveByPoint(curRow, curCol);
                    //removeFreeSmallRoom(findSmallFreeRoom(curRow, curCol));
                    break;
                case 2:
                    freeBigRoom.RemoveByPoint(curRow, curCol);
                    break;
                case 3:
                    freeSmallRoom.RemoveByPoint(curRow, curCol);
                    freeBigRoom.RemoveByPoint(curRow, curCol);
                    //removeFreeSmallRoom(findSmallFreeRoom(curRow, curCol));
                    //removeFreeBigRoom(findBigFreeRoom(curRow, curCol));
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
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        //removeFreeBigRoom(findBigFreeRoom(curRow, col - 1));
                        floorMatrix[curRow, col - 1] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        //removeFreeBigRoom(findBigFreeRoom(curRow, col - 1));
                        floorMatrix[curRow, col - 1] = 1;
                        break;
                }
            }
            if (row > 0)
            {
                switch(floorMatrix[row - 1, col - 1])
                {
                    case 2:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        //removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
                        floorMatrix[row - 1, col - 1] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        //removeFreeBigRoom(findBigFreeRoom(row - 1, col - 1));
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
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        //removeFreeBigRoom(findBigFreeRoom(row - 1, curCol));
                        floorMatrix[row - 1, curCol] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        //removeFreeBigRoom(findBigFreeRoom(row - 1, curCol));
                        floorMatrix[row - 1, curCol] = 1;
                        break;
                }
            }
        }
    }

    private void createSmallRoom(int row, int col, bool withEnemies)
    {
        Vector3 pos = new Vector3((col - floorWidth / 2) * 17.6f, (floorHeight / 2 - row) * 11f, 0f);
        room = Instantiate(smallRoomPrefab, pos, Quaternion.identity);
        getSmallRoomType(row, col);
        createSmallDoors();
        if (row == floorHeight / 2 && col == floorWidth / 2)
        {
            return;
        }
        if (withEnemies)
        {
            room.GetComponent<RoomController>().loadout = smallLoadouts[Random.Range(0, smallLoadouts.Length)];
            room.GetComponent<RoomController>().roomDrops = pickups;
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
        Vector3 pos = new Vector3((col - floorWidth / 2) * 17.6f, (floorHeight / 2 - row) * 11f, 0f);
        room = Instantiate(bigRoomPrefab, pos, Quaternion.identity);
        getBigRoomType(row, col);
        createBigDoors();
        if (isBoss)
        {
            room.GetComponent<RoomController>().loadout = bossLoadouts[Random.Range(0, bossLoadouts.Length)];
            room.GetComponent<RoomController>().roomDrops = bossDrops;
        }
        else
        {
            room.GetComponent<RoomController>().loadout = bigLoadouts[Random.Range(0, bigLoadouts.Length)];
            room.GetComponent<RoomController>().roomDrops = pickups;
        }
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
            tmp = Instantiate(smallDoors[i] ? doorPrefab : wallPlugPrefab, room.transform.position + doorPoss[i], i == 0 || i == 3 ? Quaternion.identity : Quaternion.Euler(0f, 0f, 90f), room.transform);
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
            tmp = Instantiate(bigDoors[i] ? doorPrefab : wallPlugPrefab, room.transform);
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

    private void buildItemRooms()
    {
        PointList freeItemRoom = new PointList();
        foreach (FloorPoint point in freeSmallRoom)
        {
            if (!(isInMiddle(point.i, point.j) || getDoorCount(point.i, point.j) != 1))
            {
                freeItemRoom.Add(point);
            }
        }
        int itemRoomCount = Random.Range(minItemRoomCount, maxItemRoomCount + 1);
        for (int i = 0; i < itemRoomCount; ++i)
        {
            if (freeItemRoom.Count == 0)
            {
                return;
            }
            int index = Random.Range(0, freeItemRoom.Count);
            int row = freeItemRoom[index].i;
            int col = freeItemRoom[index].j;
            for (int curRow = row - 1; curRow <= row + 1; ++curRow)
            {
                for (int curCol = col - 1; curCol <= col + 1; ++curCol)
                {
                    freeItemRoom.RemoveByPoint(curRow, curCol);
                }
            }
            floorMatrix[row, col] = 9;
        }

        /*int itemRoomCount = Random.Range(minItemRoomCount, maxItemRoomCount + 1);
        int[] smallRoomHeights = new int[floorHeight * floorWidth];
        int[] smallRoomWidths = new int[floorHeight * floorWidth];
        int smallRoomsCount = 0;
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorMatrix[i, j] == 4 && !(i >= floorHeight / 2 - 1 && i <= floorHeight / 2 + 1 && j >= floorWidth / 2 - 1 && j <= floorWidth / 2 + 1))
                {
                    smallRoomHeights[smallRoomsCount] = i;
                    smallRoomWidths[smallRoomsCount] = j;
                    ++smallRoomsCount;
                }
            }
        }
        Debug.Log(itemRoomCount);
        Debug.Log(smallRoomsCount);
        for (int i = 0; i < itemRoomCount; ++i)
        {
            int room = 0, row = 0, col = 0, iter = 0;
            do
            {
                room = Random.Range(0, smallRoomsCount);
                row = smallRoomHeights[room];
                col = smallRoomWidths[room];
                ++iter;
            } while (!checkItemRoomPlacement(row, col) && iter < 100);
            floorMatrix[row, col] = 9;
        }*/
    }

    private void buildBossRoom()
    {
        int[,] tmpMatrix = new int[floorHeight + 6, floorWidth + 6];
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                tmpMatrix[i + 3, j + 3] = floorMatrix[i, j];
            }
        }
        floorMatrix = tmpMatrix;
        floorHeight += 6;
        floorWidth += 6;
        int tmpType = Random.Range(0, 4);
        int[] freeRoom = new int[Mathf.Max(floorHeight, floorWidth)];
        int freeRoomCount = 0;
        if (tmpType == 0)
        {
            int row = 0, col = 0;
            bool indicator = false;
            for (; row < floorHeight; ++row)
            {
                for (; col < floorWidth; ++col)
                {
                    if (floorMatrix[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                col = 0;
            }
            for (; col < floorWidth; ++col)
            {
                if (floorMatrix[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = col;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorMatrix[row - 1, tmpPos] = 8;
            floorMatrix[row - 3, tmpPos] = 7;
            floorMatrix[row - 3, tmpPos + 1] = floorMatrix[row - 2, tmpPos] = floorMatrix[row - 2, tmpPos + 1] = 6;
        }
        else if (tmpType == 1)
        {
            int row = 0, col = 0;
            bool indicator = false;
            for (; col < floorWidth; ++col)
            {
                for (; row < floorHeight; ++row)
                {
                    if (floorMatrix[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                row = 0;
            }
            for (; row < floorHeight; ++row)
            {
                if (floorMatrix[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = row;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorMatrix[tmpPos, col - 1] = 8;
            floorMatrix[tmpPos, col - 3] = 7;
            floorMatrix[tmpPos, col - 2] = floorMatrix[tmpPos + 1, col - 3] = floorMatrix[tmpPos + 1, col - 2] = 6;
        }
        else if (tmpType == 2)
        {
            int row = 0, col = floorWidth - 1;
            bool indicator = false;
            for (; col >= 0; --col)
            {
                for (; row < floorHeight; ++row)
                {
                    if (floorMatrix[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                row = 0;
            }
            for (; row < floorHeight; ++row)
            {
                if (floorMatrix[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = row;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorMatrix[tmpPos, col + 1] = 8;
            floorMatrix[tmpPos, col + 2] = 7;
            floorMatrix[tmpPos, col + 3] = floorMatrix[tmpPos + 1, col + 2] = floorMatrix[tmpPos + 1, col + 3] = 6;
        }
        else
        {
            int row = floorHeight - 1, col = 0;
            bool indicator = false;
            for (; row >= 0; --row)
            {
                for (; col < floorWidth; ++col)
                {
                    if (floorMatrix[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                col = 0;
            }
            for (; col < floorWidth; ++col)
            {
                if (floorMatrix[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = col;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorMatrix[row + 1, tmpPos] = 8;
            floorMatrix[row + 2, tmpPos] = 7;
            floorMatrix[row + 2, tmpPos + 1] = floorMatrix[row + 3, tmpPos] = floorMatrix[row + 3, tmpPos + 1] = 6;
        }
    }

    private int getDoorCount(int row, int col)
    {
        int result = 0;
        if (row < floorHeight - 1 && floorMatrix[row + 1, col] >= 4)
        {
            ++result;
        }
        if (col < floorWidth - 1 && floorMatrix[row, col + 1] >= 4)
        {
            ++result;
        }
        if (row > 0 && floorMatrix[row - 1, col] >= 4)
        {
            ++result;
        }
        if (col > 0 && floorMatrix[row, col - 1] >= 4)
        {
            ++result;
        }
        return result;
    }

    private bool isInMiddle(int row, int col)
    {
        return row >= floorHeight / 2 - 1 && row <= floorHeight / 2 + 1 && col >= floorWidth / 2 - 1 && col <= floorWidth / 2 + 1;
    }

    public string FloorMatrixStr()
    {
        string str = "Free small count: " + freeSmallRoom.Count + "\nFree big count: " + freeBigRoom.Count + "\n";
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                str += floorMatrix[i, j].ToString();
            }
            str += "\n";
        }
        return str;
    }
}

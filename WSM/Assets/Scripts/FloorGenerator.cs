using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public int minRoomCount;
    public int maxRoomCount;
    public int floorHeight;
    public int floorWidth;

    public GameObject s_D;

    public GameObject s_N;
    public GameObject s_W;
    public GameObject s_E;
    public GameObject s_S;

    public GameObject s_NS;
    public GameObject s_WE;
    public GameObject s_NW;
    public GameObject s_NE;
    public GameObject s_WS;
    public GameObject s_ES;

    public GameObject s_NWE;
    public GameObject s_NWS;
    public GameObject s_NES;
    public GameObject s_WES;

    public GameObject s_NWES;

    public EnemyLoadout[] loadouts;
    public WeaponsList weapons;
    public GameObject[] pickups;

    private int[,] floorMatrix;
    private int[] freeRoomHeights;
    private int[] freeRoomWidths;
    private int freeRoomCount;

    private void Awake()
    {
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
        GameObject room = Instantiate(getRoomType(row, col), pos, Quaternion.identity);
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

    private GameObject getRoomType(int row, int col)
    {
        int exits = 0;
        if (row > 0)
        {
            exits |= (floorMatrix[row - 1, col] == 2) ? 1 : 0;
        }
        if (col > 0)
        {
            exits |= (floorMatrix[row, col - 1] == 2) ? 2 : 0;
        }
        if (row < floorHeight - 1)
        {
            exits |= (floorMatrix[row + 1, col] == 2) ? 8 : 0;
        }
        if (col < floorWidth - 1)
        {
            exits |= (floorMatrix[row, col + 1] == 2) ? 4 : 0;
        }
        switch (exits)
        {
            case 0:
                return s_D;
            case 1:
                return s_N;
            case 2:
                return s_W;
            case 3:
                return s_NW;
            case 4:
                return s_E;
            case 5:
                return s_NE;
            case 6:
                return s_WE;
            case 7:
                return s_NWE;
            case 8:
                return s_S;
            case 9:
                return s_NS;
            case 10:
                return s_WS;
            case 11:
                return s_NWS;
            case 12:
                return s_ES;
            case 13:
                return s_NES;
            case 14:
                return s_WES;
            case 15:
                return s_NWES;
        }
        return s_D;
    }
}

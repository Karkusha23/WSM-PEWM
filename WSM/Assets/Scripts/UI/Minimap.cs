using UnityEngine;

public class Minimap : MonoBehaviour
{
    public int pixHalfARoom = 10;
    public int minimapHeight = 10;
    public int minimapWidth = 10;

    public GameObject smallRoomNotExploredPrefab;
    public GameObject smallRoomExploredPrefab;
    public GameObject bigRoomNotExploredPrefab;
    public GameObject bigRoomExploredPrefab;
    public GameObject bossIconPrefab;

    [HideInInspector]
    public FloorGenerator.FloorGrid floorGrid;
    [HideInInspector]
    public int floorHeight;
    [HideInInspector]
    public int floorWidth;

    private byte[,] floorExplored;
    // 0 - not explored
    // 1 - explored small room, not visited
    // 2 - explored big room core, not visited
    // 3 - explored big room units, not visited
    // 4 - small room visited
    // 5 - big room core visited
    // 6 - big room subuints visited
    private int curRow;
    private int curCol;
    private Transform minimapBase;
    private GameObject[,] notExploredRooms;
    private GameObject[,] exploredRooms;
    private int pixARoom;

    private void Start()
    {
        var floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Floor>();

        floorGrid = floor.FloorGrid;
        floorHeight = floorGrid.rows;
        floorWidth = floorGrid.cols;

        curRow = floorHeight / 2;
        curCol = floorWidth / 2;
        pixARoom = pixHalfARoom * 2;
        floorExplored = new byte[floorHeight, floorWidth];
        minimapBase = transform.Find("MinimapBase");
        notExploredRooms = new GameObject[floorHeight, floorWidth];
        exploredRooms = new GameObject[floorHeight, floorWidth];
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorGrid.isRoomSmall(i, j))
                {
                    Vector2 newSmallRoomOffset = new Vector2((j - floorWidth / 2) * pixARoom, (floorHeight / 2 - i) * pixARoom);

                    notExploredRooms[i, j] = Instantiate(smallRoomNotExploredPrefab, minimapBase);
                    notExploredRooms[i, j].SetActive(false);
                    notExploredRooms[i, j].GetComponent<RectTransform>().anchoredPosition += newSmallRoomOffset;

                    exploredRooms[i, j] = Instantiate(smallRoomExploredPrefab, minimapBase);
                    exploredRooms[i, j].SetActive(false);
                    exploredRooms[i, j].GetComponent<RectTransform>().anchoredPosition += newSmallRoomOffset;
                }
                else if (floorGrid.isRoomBig(i, j))
                {
                    Vector2 newBigRoomOffset = new Vector2((j - floorWidth / 2) * pixARoom + pixHalfARoom, (floorHeight / 2 - i) * pixARoom - pixHalfARoom);

                    notExploredRooms[i, j] = notExploredRooms[i, j + 1] = notExploredRooms[i + 1, j] = notExploredRooms[i + 1, j + 1] = Instantiate(bigRoomNotExploredPrefab, minimapBase);
                    exploredRooms[i, j] = exploredRooms[i, j + 1] = exploredRooms[i + 1, j] = exploredRooms[i + 1, j + 1] = Instantiate(bigRoomExploredPrefab, minimapBase);

                    if (floorGrid[i, j] == FloorGenerator.RoomType.Boss)
                    {
                        Instantiate(bossIconPrefab, notExploredRooms[i, j].transform.position, Quaternion.identity, notExploredRooms[i, j].transform);
                        Instantiate(bossIconPrefab, exploredRooms[i, j].transform.position, Quaternion.identity, exploredRooms[i, j].transform);
                    }

                    notExploredRooms[i, j].GetComponent<RectTransform>().anchoredPosition += newBigRoomOffset;
                    exploredRooms[i, j].GetComponent<RectTransform>().anchoredPosition += newBigRoomOffset;

                    notExploredRooms[i, j].SetActive(false);
                    exploredRooms[i, j].SetActive(false);
                }

            }
        }
    }

    public void checkRoom(Vector3 roomPos)
    {
        int row = floorHeight / 2 - Mathf.RoundToInt(roomPos.y / Floor.roomHeight);
        int col = floorWidth / 2 + Mathf.RoundToInt(roomPos.x / Floor.roomWidth);
        if (floorExplored[row, col] < 4)
        {
            if (floorGrid.isRoomSmall(row, col))
            {
                floorExplored[row, col] = 4;

                checkSmallAround(row, col);
            }
            else if (floorGrid.isRoomBig(row, col))
            {
                floorExplored[row, col] = 5;
                floorExplored[row, col + 1] = floorExplored[row + 1, col] = floorExplored[row + 1, col + 1] = 6;

                checkBigAround(row, col);
            }

            exploredRooms[row, col].SetActive(true);
            notExploredRooms[row, col].SetActive(false);
        }

        minimapBase.GetComponent<RectTransform>().anchoredPosition += new Vector2((curCol - col) * pixARoom, (row - curRow) * pixARoom);
        curRow = row;
        curCol = col;

        checkToActivate();
    }

    private void checkSmallAround(int row, int col)
    {
        checkCell(row - 1, col);
        checkCell(row, col - 1);
        checkCell(row, col + 1);
        checkCell(row + 1, col);
    }

    private void checkBigAround(int row, int col)
    {
        checkCell(row - 1, col);
        checkCell(row - 1, col + 1);
        checkCell(row, col - 1);
        checkCell(row, col + 2);
        checkCell(row + 1, col - 1);
        checkCell(row + 1, col + 2);
        checkCell(row + 2, col);
        checkCell(row + 2, col + 1);
    }

    private void checkCell(int row, int col)
    {
        if (floorExplored[row, col] == 0 && floorGrid.isRoomOccupied(row, col))
        {
            notExploredRooms[row, col].SetActive(true);
            if (floorGrid.isRoomSmall(row, col))
            {
                floorExplored[row, col] = 1;
            }
            else if (floorGrid.isRoomSmall(row, col))
            {
                floorExplored[row, col] = 2;
                floorExplored[row, col + 1] = floorExplored[row + 1, col] = floorExplored[row + 1, col + 1] = 3;
            }
            else if (floorGrid[row, col] == FloorGenerator.RoomType.BigSubunit)
            {
                if (floorGrid.isRoomBig(row, col - 1))
                {
                    floorExplored[row, col - 1] = 2;
                    floorExplored[row, col] = floorExplored[row + 1, col - 1] = floorExplored[row + 1, col] = 3;
                }
                else if (floorGrid.isRoomBig(row - 1, col))
                {
                    floorExplored[row - 1, col] = 2;
                    floorExplored[row - 1, col + 1] = floorExplored[row, col] = floorExplored[row, col + 1] = 3;
                }
                else
                {
                    floorExplored[row - 1, col - 1] = 2;
                    floorExplored[row - 1, col] = floorExplored[row, col - 1] = floorExplored[row, col] = 3;
                }
            }
        }
    }

    private void checkToActivate()
    {
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorExplored[i, j] > 0)
                {
                    if (Mathf.Abs(i - curRow) <= minimapHeight / 2 - 1 && Mathf.Abs(j - curCol) <= minimapWidth / 2 - 1)
                    {
                        if (floorExplored[i, j] >= 1 && floorExplored[i, j] <= 3)
                        {
                            notExploredRooms[i, j].SetActive(true);
                        }
                        else if (floorExplored[i, j] >= 4 && floorExplored[i, j] <= 6)
                        {
                            exploredRooms[i, j].SetActive(true);
                        }
                    }
                    else
                    {
                        notExploredRooms[i, j].SetActive(false);
                        exploredRooms[i, j].SetActive(false);
                    }
                }
            }
        }
    }
}
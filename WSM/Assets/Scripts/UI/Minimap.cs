using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour
{
    public enum MinimapCell : byte
    {
        NotExplored, // Room is not explored and not shown in minimap
        SmallExplored, // Small room explored, but not visited. Shown in minimap as dark cell
        BigExplored, // Big room core explored, but not visited
        BigSubunitExplored, // Big room subunit explored, but not visited
        SmallVisited, // Small room visited. Shown in minimap as bright cell
        BigVisited, // Big room core visited
        BigSubunitVisited // Big room subunit visited
    }

    public static bool isRoomExplored(MinimapCell minimapCell)
    {
        switch (minimapCell)
        {
            case MinimapCell.SmallExplored:
            case MinimapCell.BigExplored:
            case MinimapCell.BigSubunitExplored:
                return true;
            default:
                break;
        }

        return false;
    }

    public static bool isRoomVisited(MinimapCell minimapCell)
    {
        switch (minimapCell)
        {
            case MinimapCell.SmallVisited:
            case MinimapCell.BigVisited:
            case MinimapCell.BigSubunitVisited:
                return true;
            default:
                break;
        }

        return false;
    }

    // Struct used in minimap grid
    public struct MinimapTuple
    {
        // Type of minimap cell
        public MinimapCell cell { get; set; }

        // Instance of not explored room icon
        public GameObject notExplored { get; set; }

        // Instance of explored room icon
        public GameObject explored { get; set; }
    }

    public class MinimapGrid : Grid<MinimapTuple>
    {
        public MinimapGrid(int floorHeight, int floorWidth)
        {
            grid = new MinimapTuple[floorHeight, floorWidth];

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    grid[i, j].cell = MinimapCell.NotExplored;
                    grid[i, j].notExplored = grid[i, j].explored = null;
                }
            }
        }

        public bool isRoomExplored(int row, int col)
        {
            return hasPoint(row, col) && Minimap.isRoomExplored(grid[row, col].cell);
        }

        public bool isRoomVisited(int row, int col)
        {
            return hasPoint(row, col) && Minimap.isRoomVisited(grid[row, col].cell);
        }

        public void setSmallRoomExplored(int row, int col)
        {
            grid[row, col].cell = MinimapCell.SmallExplored;
        }

        public void setBigRoomExplored(int row, int col)
        {
            grid[row, col].cell = MinimapCell.BigExplored;
            grid[row, col + 1].cell = grid[row + 1, col].cell = grid[row + 1, col + 1].cell = MinimapCell.BigSubunitExplored;
        }

        public void setSmallRoomVisited(int row, int col)
        {
            if (!hasPoint(row, col))
            {
                return;
            }

            grid[row, col].cell = MinimapCell.SmallVisited;

            grid[row, col].explored.SetActive(true);
            grid[row, col].notExplored.SetActive(false);
        }

        public void setBigRoomVisited(int row, int col)
        {
            if (!(row >= 0 && row < rows - 1 && col >= 0 && col < cols - 1))
            {
                return;
            }

            grid[row, col].cell = MinimapCell.BigVisited;
            grid[row, col + 1].cell = grid[row + 1, col].cell = grid[row + 1, col + 1].cell = MinimapCell.BigSubunitVisited;

            grid[row, col].explored.SetActive(true);
            grid[row, col].notExplored.SetActive(false);
        }

        public void createSmallRoom(int row, int col, GameObject notExploredPrefab, GameObject exploredPrefab, Transform parent, Vector2 offset)
        {
            grid[row, col].notExplored = Instantiate(notExploredPrefab, parent);
            grid[row, col].notExplored.SetActive(false);
            grid[row, col].notExplored.GetComponent<RectTransform>().anchoredPosition += offset;

            grid[row, col].explored = Instantiate(exploredPrefab, parent);
            grid[row, col].explored.SetActive(false);
            grid[row, col].explored.GetComponent<RectTransform>().anchoredPosition += offset;
        }

        public void createBigRoom(int row, int col, GameObject notExploredPrefab, GameObject exploredPrefab, Transform parent, Vector2 offset)
        {
            grid[row, col].notExplored = grid[row, col + 1].notExplored = grid[row + 1, col].notExplored = grid[row + 1, col + 1].notExplored = Instantiate(notExploredPrefab, parent);
            grid[row, col].notExplored.GetComponent<RectTransform>().anchoredPosition += offset;
            grid[row, col].notExplored.SetActive(false);

            grid[row, col].explored = grid[row, col + 1].explored = grid[row + 1, col].explored = grid[row + 1, col + 1].explored = Instantiate(exploredPrefab, parent);
            grid[row, col].explored.GetComponent<RectTransform>().anchoredPosition += offset;
            grid[row, col].explored.SetActive(false);
        }
    }

    public const int pixHalfARoom = 10;
    public const int pixARoom = pixHalfARoom * 2;

    public const int minimapHeight = 10;
    public const int minimapWidth = 10;

    public const float fullMapToggleReloadTime = 0.1f;

    public GameObject smallRoomNotExploredPrefab;
    public GameObject smallRoomExploredPrefab;
    public GameObject itemRoomNotExploredPrefab;
    public GameObject itemRoomExploredPrefab;
    public GameObject bigRoomNotExploredPrefab;
    public GameObject bigRoomExploredPrefab;
    public GameObject bossIconPrefab;

    public bool IsFullMapOn { get => isFullMapOn; }

    private bool isFullMapOn;

    private MinimapGrid minimapGrid;
    private FloorGenerator.FloorGrid floorGrid;

    private int floorHeight;
    private int floorWidth;

    private int curRow;
    private int curCol;
    private RectTransform playerIcon;
    private RectTransform minimapBase;
    private GameObject frame;

    private Vector2 minimapBasePosition;

    private bool canToggleMinimap;

    private void Start()
    {
        var floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<Floor>();

        floorGrid = floor.FloorGrid;
        floorHeight = floorGrid.rows;
        floorWidth = floorGrid.cols;

        curRow = floorHeight / 2;
        curCol = floorWidth / 2;

        isFullMapOn = false;
        canToggleMinimap = true;

        minimapGrid = new MinimapGrid(floorHeight, floorWidth);

        minimapBase = transform.Find("MinimapBase").GetComponent<RectTransform>();
        playerIcon = minimapBase.Find("Player").GetComponent<RectTransform>();
        frame = transform.Find("Frame").gameObject;

        minimapBasePosition = minimapBase.anchoredPosition;

        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorGrid.isRoomSmall(i, j))
                {
                    Vector2 newSmallRoomOffset = new Vector2((j - floorWidth / 2) * pixARoom, (floorHeight / 2 - i) * pixARoom);

                    if (floorGrid[i, j] == FloorGenerator.RoomType.Item)
                    {
                        minimapGrid.createSmallRoom(i, j, itemRoomNotExploredPrefab, itemRoomExploredPrefab, minimapBase, newSmallRoomOffset);
                    }
                    else
                    {
                        minimapGrid.createSmallRoom(i, j, smallRoomNotExploredPrefab, smallRoomExploredPrefab, minimapBase, newSmallRoomOffset);
                    }
                }
                else if (floorGrid.isRoomBig(i, j))
                {
                    Vector2 newBigRoomOffset = new Vector2((j - floorWidth / 2) * pixARoom + pixHalfARoom, (floorHeight / 2 - i) * pixARoom - pixHalfARoom);

                    minimapGrid.createBigRoom(i, j, bigRoomNotExploredPrefab, bigRoomExploredPrefab, minimapBase, newBigRoomOffset);

                    if (floorGrid[i, j] == FloorGenerator.RoomType.Boss)
                    {
                        Instantiate(bossIconPrefab, minimapGrid[i, j].notExplored.transform.position, Quaternion.identity, minimapGrid[i, j].notExplored.transform);
                        Instantiate(bossIconPrefab, minimapGrid[i, j].explored.transform.position, Quaternion.identity, minimapGrid[i, j].explored.transform);
                    }
                }

            }
        }
    }

    public void checkRoom(Vector3 roomPos)
    {
        int row = floorHeight / 2 - Mathf.RoundToInt(roomPos.y / Floor.roomHeight);
        int col = floorWidth / 2 + Mathf.RoundToInt(roomPos.x / Floor.roomWidth);
        if (!minimapGrid.isRoomVisited(row, col))
        {
            if (floorGrid.isRoomSmall(row, col))
            {
                minimapGrid.setSmallRoomVisited(row, col);

                checkSmallAround(row, col);
            }
            else if (floorGrid.isRoomBig(row, col))
            {
                minimapGrid.setBigRoomVisited(row, col);

                checkBigAround(row, col);
            }
        }

        Vector2 minimapMove = new Vector2((curCol - col) * pixARoom, (row - curRow) * pixARoom);

        if (!isFullMapOn)
        {
            minimapBase.anchoredPosition += minimapMove;
        }
        else
        {
            minimapBasePosition += minimapMove;
        }

        playerIcon.anchoredPosition -= minimapMove;

        Debug.Log(minimapMove);

        curRow = row;
        curCol = col;

        checkToActivate(false);
    }

    public void toggleFullMap()
    {
        if (!canToggleMinimap)
        {
            return;
        }

        canToggleMinimap = false;

        isFullMapOn = !isFullMapOn;

        if (isFullMapOn)
        {
            minimapBasePosition = minimapBase.anchoredPosition;
            minimapBase.transform.position = transform.parent.position;
        }
        else
        {
            minimapBase.anchoredPosition = minimapBasePosition;
        }

        frame.SetActive(!isFullMapOn);

        checkToActivate(isFullMapOn);

        StartCoroutine("fullMapToggleReload");
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
        if (!minimapGrid.hasPoint(row, col))
        {
            return;
        }

        if (minimapGrid[row, col].cell == MinimapCell.NotExplored && floorGrid.isRoomOccupied(row, col))
        {
            minimapGrid[row, col].notExplored.SetActive(true);
            if (floorGrid.isRoomSmall(row, col))
            {
                minimapGrid.setSmallRoomExplored(row, col);
            }
            else if (floorGrid.isRoomSmall(row, col))
            {
                minimapGrid.setBigRoomExplored(row, col);
            }
            else if (floorGrid[row, col] == FloorGenerator.RoomType.BigSubunit)
            {
                if (floorGrid.isRoomBig(row, col - 1))
                {
                    minimapGrid.setBigRoomExplored(row, col - 1);
                }
                else if (floorGrid.isRoomBig(row - 1, col))
                {
                    minimapGrid.setBigRoomExplored(row - 1, col);
                }
                else
                {
                    minimapGrid.setBigRoomExplored(row - 1, col - 1);
                }
            }
        }
    }

    private void checkToActivate(bool showAllMap)
    {
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (minimapGrid[i, j].cell != MinimapCell.NotExplored)
                {
                    if (showAllMap || Mathf.Abs(i - curRow) <= minimapHeight / 2 - 1 && Mathf.Abs(j - curCol) <= minimapWidth / 2 - 1)
                    {
                        if (minimapGrid.isRoomExplored(i, j))
                        {
                            minimapGrid[i, j].notExplored.SetActive(true);
                        }
                        else if (minimapGrid.isRoomVisited(i, j))
                        {
                            minimapGrid[i, j].explored.SetActive(true);
                        }
                    }
                    else
                    {
                        minimapGrid[i, j].notExplored.SetActive(false);
                        minimapGrid[i, j].explored.SetActive(false);
                    }
                }
            }
        }
    }

    private IEnumerator fullMapToggleReload()
    {
        yield return new WaitForSeconds(fullMapToggleReloadTime);
        canToggleMinimap = true;
    }
}
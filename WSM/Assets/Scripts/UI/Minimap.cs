using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject smallRoomNotExplored;
    public GameObject smallRoomExplored;
    public GameObject bigRoomNotExplored;
    public GameObject bigRoomExplored;
    public GameObject bossIcon;

    [HideInInspector]
    public int[,] floorMatrix;
    [HideInInspector]
    public int floorHeight;
    [HideInInspector]
    public int floorWidth;

    private int[,] floorExplored;
    // 0 - not explored
    // 1 - explored small room, not visited
    // 2 - explored big room core, not visited
    // 3 - explored big room units, not visited
    // 4 - explored boss room core, not visited
    // 5 - small room visited
    // 6 - big room core visited
    // 7 - big room subuints visited
    // 8 - boss room visited
    private int curRow;
    private int curCol;
    private Transform player;
    private Transform minimapBase;
    private GameObject[,] notExploredRooms;
    private GameObject[,] exploredRooms;

    private void Start()
    {
        floorExplored = new int[floorHeight, floorWidth];
        player = GameObject.FindGameObjectWithTag("Player").transform;
        minimapBase = transform.Find("MinimapBase");
        notExploredRooms = new GameObject[floorHeight, floorWidth];
        exploredRooms = new GameObject[floorHeight, floorWidth];
        for (int i = 0; i < floorHeight; ++i)
        {
            for (int j = 0; j < floorWidth; ++j)
            {
                if (floorMatrix[i, j] == 4 || floorMatrix[i, j] == 8)
                {
                    notExploredRooms[i, j] = Instantiate(smallRoomNotExplored, new Vector3(floorWidth / 2 + j * 20f, floorHeight / 2 - i * 20f, 0f), Quaternion.identity, minimapBase);
                    notExploredRooms[i, j].SetActive(false);
                    exploredRooms[i, j] = Instantiate(smallRoomExplored, new Vector3(floorWidth / 2 + j * 20f, floorHeight / 2 - i * 20f, 0f), Quaternion.identity, minimapBase);
                    exploredRooms[i, j].SetActive(false);
                }
                else if (floorMatrix[i, j] == 5 || floorMatrix[i, j] == 7)
                {
                    notExploredRooms[i, j] = Instantiate(bigRoomNotExplored, new Vector3(floorWidth / 2 + j * 20f + 10f, floorHeight / 2 - i * 20f - 10f, 0f), Quaternion.identity, minimapBase);
                    exploredRooms[i, j] = Instantiate(bigRoomExplored, new Vector3(floorWidth / 2 + j * 20f + 10f, floorHeight / 2 - i * 20f - 10f, 0f), Quaternion.identity, minimapBase);
                    if (floorMatrix[i, j] == 7)
                    {
                        Instantiate(bossIcon, notExploredRooms[i, j].transform.position, Quaternion.identity, notExploredRooms[i, j].transform);
                        Instantiate(bossIcon, exploredRooms[i, j].transform.position, Quaternion.identity, exploredRooms[i, j].transform);
                    }
                    notExploredRooms[i, j].SetActive(false);
                    exploredRooms[i, j].SetActive(false);
                }

            }
        }
    }

    public void checkRoom(Vector3 roomPos)
    {
        int row = floorHeight / 2 - Mathf.RoundToInt(roomPos.y / 11f);
        int col = floorWidth / 2 + Mathf.RoundToInt(roomPos.x / 17.6f);
        if (floorExplored[row, col] < 5)
        {
            if (floorMatrix[row, col] == 4 || floorMatrix[row, col] == 8)
            {
                floorExplored[row, col] = 5;
            }
            else if (floorMatrix[row, col] == 5 || floorMatrix[row, col] == 7)
            {
                if (floorMatrix[row, col] == 5)
                {
                    floorExplored[row, col] = 6;
                }
                else
                {
                    floorExplored[row, col] = 8;
                }
                floorExplored[row, col + 1] = floorExplored[row + 1, col] = floorExplored[row + 1, col + 1] = 7;
            }
        }
    }
}

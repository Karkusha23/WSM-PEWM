using Unity.VisualScripting;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject[] doors;
    public EnemyLoadout loadout;

    private int enemyCount;

    private void Start()
    {
        lockDoors();
        enemyCount = loadout.enemyPoss.Length;
        for (int i = enemyCount - 1; i >= 0; --i)
        {
            Instantiate(loadout.enemyTypes[i], loadout.enemyPoss[i] + transform.position, Quaternion.identity, transform);
        }
    }

    public void checkEnemyKilled()
    {
        enemyCount -= 1;
        if (enemyCount <= 0)
        {
            openDoors();
        }
    }

    private void lockDoors()
    {
        foreach (var door in doors)
        {
            door.SetActive(true);
        }
    }

    private void openDoors()
    {
        foreach (var door in doors)
        {
            door.SetActive(false);
        }
    }
}

using UnityEngine;

public class HUDHP : MonoBehaviour
{
    public GameObject hpPic;
    public Vector3 hpOffset;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        addHP(player.GetComponent<PlayerController>().health - 1);
    }

    public void addHP()
    {
        Instantiate(hpPic, transform.position + hpOffset * (transform.childCount + 1), Quaternion.identity, transform);
    }

    public void addHP(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            addHP();
        }
    }

    public void removeHP()
    {
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(transform.childCount - 1).gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

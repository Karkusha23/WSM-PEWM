using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Boss1WeaponController : MonoBehaviour
{
    public float spearsOffset;
    public GameObject EnemyBullet;

    private GameObject[] spears;

    private Transform player;
    private bool isTargetting;
    private Vector3 directionTmp;
    private float rotationTmp;
    private float magnitudeTmp;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        isTargetting = true;
        spears = new GameObject[4];
        for (int i = 0; i < 4; ++i)
        {
            spears[i] = transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (isTargetting)
        {
            directionTmp = player.position - transform.position;
            rotationTmp = Mathf.Atan2(directionTmp.y, directionTmp.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationTmp + spearsOffset);
            magnitudeTmp = directionTmp.magnitude;
            for (int i = 0; i < 4; ++i)
            {
                rotationTmp = Mathf.Atan2(directionTmp.magnitude, spears[i].transform.position.x) * Mathf.Rad2Deg;
                spears[i].transform.rotation = Quaternion.Euler(0f, 0f, rotationTmp);
            }
        }
    }

    public void spiralAttack()
    {

    }
}

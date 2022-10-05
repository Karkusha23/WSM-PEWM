using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Boss1WeaponController : MonoBehaviour
{
    public float spearsOffset;
    public float rotationSpeed;
    public float spearsDumping;
    public float changeDistance;
    public float spearsRotationDumping;
    public GameObject EnemyBullet;

    private GameObject[] spears;

    private float spearsScale;
    private Transform player;
    private int spearsState;
    private Vector3 directionTmp;
    private float rotationTmp;
    private float magnitudeTmp;
    private Vector3[] spearsRotatePositions;

    private void Start()
    {
        spearsRotatePositions = new Vector3[4];
        spearsRotatePositions[0] = new Vector3(0f, 0.8f, 0f);
        spearsRotatePositions[1] = new Vector3(0f, -0.8f, 0f);
        spearsRotatePositions[2] = new Vector3(-0.8f, 0f, 0f);
        spearsRotatePositions[3] = new Vector3(0.8f, 0f, 0f);
        spearsScale = transform.parent.localScale.x;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spearsState = 0;
        spears = new GameObject[4];
        for (int i = 0; i < 4; ++i)
        {
            spears[i] = transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (spearsState == 0)
        {
            directionTmp = player.position - transform.position;
            rotationTmp = Mathf.Atan2(directionTmp.y, directionTmp.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotationTmp + spearsOffset);
            magnitudeTmp = directionTmp.magnitude;
            for (int i = 0; i < 4; ++i)
            {
                rotationTmp = Mathf.Atan2(magnitudeTmp, spears[i].transform.localPosition.x * spearsScale) * Mathf.Rad2Deg;
                spears[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationTmp + spearsOffset);
            }
        }
        else if (spearsState == 1)
        {
            for (int i = 0; i < 4; ++i)
            {
                spears[i].transform.localPosition = Vector3.Lerp(spears[i].transform.localPosition, spearsRotatePositions[i], spearsDumping * Time.deltaTime);
                spears[i].transform.localRotation = Quaternion.Lerp(spears[i].transform.localRotation, i < 2 ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, 0f), spearsRotationDumping * Time.deltaTime);
            }
            transform.Rotate(0f, 0f, Time.deltaTime * rotationSpeed);
            if ((spears[0].transform.localPosition - spearsRotatePositions[0]).magnitude <= changeDistance)
            {
                for (int i = 0; i < 4; ++i)
                {
                    spears[i].transform.localPosition = spearsRotatePositions[i];
                }
                spearsState = 2;
            }
        }
        else if (spearsState == 2)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    public void spiralAttack()
    {
        spearsState = 1;
    }
}

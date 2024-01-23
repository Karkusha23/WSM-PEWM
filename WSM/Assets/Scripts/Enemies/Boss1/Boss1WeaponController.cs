using UnityEngine;
using System.Collections;

public class Boss1WeaponController : MonoBehaviour
{
    public float spearsOffset = 90.0f;
    public float rotationSpeed = 400.0f;
    public float spearsDumping = 5.0f;
    public float changeDistance = 0.005f;
    public float spearsRotationDumping = 10.0f;
    public float[] attackDuration = { 3.0f, 3.0f };
    public float[] attackTimeBetweenShots = { 0.1f, 0.05f };
    public float[] attackBulletSpeed = { 5.0f, 7.0f };
    public float minBulSpeedScale = 1.0f;
    public float maxBulSpeedScale = 1.2f;
    public float maxBulDirectionDeviation = 1.5f;
    public GameObject EnemyBullet;

    private GameObject[] spears;
    private GameObject[] shootingPoints;

    private float spearsScale;
    private Transform player;
    private int spearsState;
    private Vector3 directionTmp;
    private float rotationTmp;
    private float magnitudeTmp;
    private Vector3[] spearsStartPositions;
    private Vector3[] spearsRotatePositions;
    private float timeBetweenAttacks;
    private Boss1 bosscon;
    private int[] spSpiralAttack = { 0, 3, 5, 6 };

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
        spearsStartPositions = new Vector3[4];
        shootingPoints = new GameObject[8];
        for (int i = 0; i < 4; ++i)
        {
            spears[i] = transform.GetChild(i).gameObject;
            spearsStartPositions[i] = spears[i].transform.localPosition;
            shootingPoints[i * 2] = spears[i].transform.GetChild(0).gameObject;
            shootingPoints[i * 2 + 1] = spears[i].transform.GetChild(1).gameObject;
        }
        bosscon = transform.parent.GetComponent<Boss1>();
        timeBetweenAttacks = bosscon.betweenAttacksTime;
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
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            if ((spears[3].transform.localPosition - spearsRotatePositions[3]).magnitude <= changeDistance)
            {
                for (int i = 0; i < 4; ++i)
                {
                    spears[i].transform.localPosition = spearsRotatePositions[i];
                    spears[i].transform.localRotation = i < 2 ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, 0f);
                }
                spearsState = 2;
            }
        }
        else if (spearsState == 2)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        else if (spearsState == 3)
        {
            directionTmp = player.position - transform.position;
            rotationTmp = Mathf.Atan2(directionTmp.y, directionTmp.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, rotationTmp + spearsOffset), spearsRotationDumping * Time.deltaTime);
            magnitudeTmp = directionTmp.magnitude;
            for (int i = 0; i < 4; ++i)
            {
                spears[i].transform.localPosition = Vector3.Lerp(spears[i].transform.localPosition, spearsStartPositions[i], spearsDumping * Time.deltaTime);
                rotationTmp = Mathf.Atan2(magnitudeTmp, spears[i].transform.localPosition.x * spearsScale) * Mathf.Rad2Deg;
                spears[i].transform.localRotation = Quaternion.Lerp(spears[i].transform.localRotation, Quaternion.Euler(0f, 0f, rotationTmp + spearsOffset), spearsDumping * Time.deltaTime);
            }
            if ((spears[0].transform.localPosition - spearsStartPositions[0]).magnitude <= changeDistance)
            {
                for (int i = 0; i < 4; ++i)
                {
                    spears[i].transform.localPosition = spearsStartPositions[i];
                }
                spearsState = 0;
            }
        }
    }

    public void spiralAttack()
    {
        StartCoroutine("spiralAttacking");
    }

    public void machineGunAttack()
    {
        StartCoroutine("machineGunAttacking");
    }

    private IEnumerator spiralAttacking()
    {
        bosscon.stopMoving();
        int count = Mathf.FloorToInt(attackDuration[0] / attackTimeBetweenShots[0]) + 1;
        spearsState = 1;
        yield return new WaitForSeconds(timeBetweenAttacks);
        GameObject bul;
        for (int i = 0; i < count; ++i)
        {
            foreach (int j in spSpiralAttack)
            {
                bul = Instantiate(EnemyBullet, shootingPoints[j].transform.position, Quaternion.identity);
                bul.GetComponent<Rigidbody2D>().velocity = (shootingPoints[j].transform.position - transform.position).normalized * attackBulletSpeed[0];
            }
            yield return new WaitForSeconds(attackTimeBetweenShots[0]);
        }
        spearsState = 3;
        yield return new WaitForSeconds(timeBetweenAttacks);
        bosscon.refresh();
    }

    private IEnumerator machineGunAttacking()
    {
        int count = Mathf.FloorToInt(attackDuration[1] / attackTimeBetweenShots[1]) / 4 + 1;
        spearsState = 3;
        yield return new WaitForSeconds(timeBetweenAttacks);
        GameObject bul;
        Vector3 destination, tmp;
        for (int i = 0; i < count; ++i)
        {
            for (int j = 0; j < 8; j += 2)
            {
                destination = (player.position - shootingPoints[j].transform.position).normalized;
                tmp = new Vector3(destination.y, -destination.x, 0f);
                destination += tmp * Random.Range(-maxBulDirectionDeviation, maxBulDirectionDeviation);
                bul = Instantiate(EnemyBullet, shootingPoints[j].transform.position, Quaternion.identity);
                bul.GetComponent<Rigidbody2D>().velocity = destination.normalized * attackBulletSpeed[1] * Random.Range(minBulSpeedScale, maxBulSpeedScale);
                yield return new WaitForSeconds(attackTimeBetweenShots[1]);
            }
        }
        bosscon.refresh();
    }
}

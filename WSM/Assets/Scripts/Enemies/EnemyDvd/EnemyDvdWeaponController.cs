using UnityEngine;
using System.Collections;

public class EnemyDvdWeaponController : MonoBehaviour
{
    public GameObject shootingPoint;
    public float shootingPointOffset;
    public GameObject bullet;
    public float fireRate;
    public float timeBetweenBullets;
    public int bulletAmount;
    public float bulletSpeed;
    public bool type;

    private GameObject[] sps;
    private Rigidbody2D enemyRB;
    private EnemyDvd enemy;
    private GameObject bul;
    private Vector3 targetAngle;
    private Quaternion targetRotation;
    private bool isRotating;
    private float activationTime;

    private void Start()
    {
        sps = new GameObject[4];
        Vector3 tmp = new Vector3(shootingPointOffset, 0f, 0f);
        sps[0] = Instantiate(shootingPoint, transform.position + tmp, Quaternion.identity, transform);
        tmp.x = -tmp.x;
        sps[1] = Instantiate(shootingPoint, transform.position + tmp, Quaternion.identity, transform);
        tmp.y = tmp.x;
        tmp.x = 0f;
        sps[2] = Instantiate(shootingPoint, transform.position + tmp, Quaternion.identity, transform);
        tmp.y = -tmp.y;
        sps[3] = Instantiate(shootingPoint, transform.position + tmp, Quaternion.identity, transform);
        enemyRB = transform.parent.GetComponent<Rigidbody2D>();
        enemy = transform.parent.GetComponent<EnemyDvd>();
        if (type)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            targetAngle = new Vector3(0f, 0f, 45f);
            targetRotation = Quaternion.Euler(0f, 0f, 45f);
        }
        else
        {
            targetAngle = Vector3.zero;
            targetRotation = Quaternion.identity;
        }
        isRotating = false;
        activationTime = transform.parent.GetComponent<EnemyDvd>().activationTime;
        StartCoroutine("activateEnemyWeapon");
    }

    private void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f / fireRate);
        }
    }

    private IEnumerator activateEnemyWeapon()
    {
        yield return new WaitForSeconds(activationTime);
        StartCoroutine("shoot");
    }

    private IEnumerator shoot()
    {
        yield return new WaitForSeconds(fireRate);
        for (; ; )
        {
            isRotating = false;
            enemy.allowedToMove = false;
            enemyRB.velocity = Vector2.zero;
            for (int i = 0; i < bulletAmount; ++i)
            {
                foreach (var point in sps)
                {
                    bul = Instantiate(bullet, point.transform.position, Quaternion.identity);
                    bul.GetComponent<Rigidbody2D>().velocity = (point.transform.position - transform.position).normalized * bulletSpeed;
                }
                yield return new WaitForSeconds(timeBetweenBullets);
            }
            enemy.allowedToMove = true;
            enemyRB.velocity = enemy.direction * enemy.enemySpeed;
            isRotating = true;
            targetAngle.z = ((int)targetAngle.z + 45) % 360;
            targetRotation.eulerAngles = targetAngle;
            yield return new WaitForSeconds(fireRate);
        }
    }
}
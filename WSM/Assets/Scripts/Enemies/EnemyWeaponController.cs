using UnityEngine;
using System.Collections;

public class EnemyWeaponController : MonoBehaviour
{
    public float angleOffset;
    public Vector3 shootingPointOffset;
    public float reloadTime;
    public float bulletSpeed;
    public GameObject shootingPoint;
    public GameObject bullet;

    private Transform player;
    private GameObject sp;
    private Vector3 direction;
    private float rotation;
    private GameObject bul;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sp = Instantiate(shootingPoint, transform.position + shootingPointOffset, Quaternion.identity, transform);
        StartCoroutine("shoot");
    }

    private void Update()
    {
        direction = player.position - transform.position;
        rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation + angleOffset);
    }

    private IEnumerator shoot()
    {
        yield return new WaitForSeconds(1f);
        for (; ; )
        {
            bul = Instantiate(bullet, sp.transform.position, Quaternion.identity);
            bul.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
            yield return new WaitForSeconds(reloadTime);
        }
    }
}

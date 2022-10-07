using UnityEngine;
using System.Collections;

public class EnemyShoot : MonoBehaviour
{
    public float enemyHealth;
    public float enemySpeed;
    public float refreshFrequency;
    public GameObject enemyWeapon;
    public float stayDistanceSqr;
    public float activationTime;

    private Rigidbody2D rb;
    private Transform player;
    private Vector3 destination;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Instantiate(enemyWeapon, transform.position, Quaternion.identity, transform);
        StartCoroutine("activateEnemy");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            enemyHealth -= collision.collider.GetComponent<Bullet>().damage;
            if (enemyHealth <= 0)
            {
                transform.parent.GetComponent<RoomController>().checkEnemyKilled();
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        StartCoroutine("refreshPlayerPos");
    }


    private IEnumerator refreshPlayerPos()
    {
        for (; ; )
        {
            destination = player.position - transform.position;
            if (destination.sqrMagnitude > stayDistanceSqr)
            {
                rb.velocity = destination.normalized * enemySpeed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
            yield return new WaitForSeconds(refreshFrequency);
        }
    }
}

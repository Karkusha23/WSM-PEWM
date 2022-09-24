using UnityEngine;
using System.Collections;

public class EnemyChase : MonoBehaviour
{
    public float enemyHealth;
    public float enemySpeed;
    public float refreshFrequency;

    private Rigidbody2D rb;
    private Transform player;
    private Vector3 destination;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine("refreshPlayerPos");
    }

    private IEnumerator refreshPlayerPos()
    {
        for (; ; )
        {
            destination = player.position - transform.position;
            rb.velocity = destination.normalized * enemySpeed;
            yield return new WaitForSeconds(refreshFrequency);
        }
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
}

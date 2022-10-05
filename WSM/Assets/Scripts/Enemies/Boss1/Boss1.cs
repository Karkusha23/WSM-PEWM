using UnityEngine;
using System.Collections;

public class Boss1 : MonoBehaviour
{
    public GameObject enemyWeapon;
    public float enemyHealth;
    public float enemySpeed;
    public float refreshPlayerPosTime;
    public float betweenAttacksTime;

    private Rigidbody2D rb;
    private Transform player;
    private Coroutine approach;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        approach = StartCoroutine("approachPlayer");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            enemyHealth -= collision.collider.GetComponent<Bullet>().damage;
            if (enemyHealth <= 0)
            {
                if (transform.parent != null)
                {
                    transform.parent.GetComponent<RoomController>().checkEnemyKilled();
                }
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator attacks()
    {
        yield return new WaitForSeconds(betweenAttacksTime);
        for (; ; )
        {
            StopCoroutine(approach);
            yield return new WaitForSeconds(betweenAttacksTime);
        }
    }

    private IEnumerator approachPlayer()
    {
        for (; ; )
        {
            rb.velocity = (player.position - transform.position).normalized * enemySpeed;
            yield return new WaitForSeconds(refreshPlayerPosTime);
        }
    }
}

using UnityEngine;
using System.Collections;
using System;

public class Boss1 : MonoBehaviour
{
    public GameObject enemyWeapon;
    public float enemyHealth;
    public float enemySpeed;
    public float refreshPlayerPosTime;
    public float betweenAttacksTime;

    private Rigidbody2D rb;
    private Transform player;
    private int attackCount;
    private Boss1WeaponController wepcon;
    private bool isAllowedToMove;

    private void Start()
    {
        isAllowedToMove = true;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackCount = 1;
        wepcon = enemyWeapon.GetComponent<Boss1WeaponController>();
        StartCoroutine("approachPlayer");
        refresh();
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

    public void stopMoving()
    {
        isAllowedToMove = false;
        rb.velocity = Vector2.zero;
    }

    public void refresh()
    {
        isAllowedToMove = true;
        StartCoroutine("attacks");
    }

    private IEnumerator attacks()
    {
        yield return new WaitForSeconds(betweenAttacksTime);
        int tmp = UnityEngine.Random.Range(0, attackCount);
        if (tmp == 0)
        {
            wepcon.spiralAttack();
        }
    }

    private IEnumerator approachPlayer()
    {
        for (; ; )
        {
            if (isAllowedToMove)
            {
                rb.velocity = (player.position - transform.position).normalized * enemySpeed;
            }
            yield return new WaitForSeconds(refreshPlayerPosTime);
        }
    }
}

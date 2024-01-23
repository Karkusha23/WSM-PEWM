using UnityEngine;
using System.Collections;

public class Boss1 : MonoBehaviour
{
    public GameObject enemyWeapon;
    public float enemyHealth = 30.0f;
    public float enemySpeed = 0.5f;
    public float refreshPlayerPosTime = 0.5f;
    public float betweenAttacksTime = 1.0f;
    public GameObject bossHealthBar;

    private Rigidbody2D rb;
    private Transform player;
    private int attackCount;
    private Boss1WeaponController wepcon;
    private bool isAllowedToMove;
    private BossHealthBar healthBar;

    private void Start()
    {
        isAllowedToMove = true;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackCount = 2;
        wepcon = enemyWeapon.GetComponent<Boss1WeaponController>();
        healthBar = Instantiate(bossHealthBar, GameObject.FindGameObjectWithTag("HUD").transform).GetComponent<BossHealthBar>();
        healthBar.maxHealth = healthBar.health = enemyHealth;
        StartCoroutine("approachPlayer");
        refresh();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            enemyHealth -= collision.collider.GetComponent<Bullet>().damage;
            healthBar.health = enemyHealth;
            if (enemyHealth <= 0)
            {
                if (transform.parent != null)
                {
                    transform.parent.GetComponent<Room>().checkEnemyKilled();
                }
                Destroy(healthBar.gameObject);
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
        int tmp = Random.Range(0, attackCount);
        if (tmp == 0)
        {
            wepcon.spiralAttack();
        }
        else if (tmp == 1)
        {
            wepcon.machineGunAttack();
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

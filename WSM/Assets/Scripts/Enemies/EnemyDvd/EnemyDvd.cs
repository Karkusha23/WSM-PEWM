using UnityEngine;
using System.Collections;

public class EnemyDvd : MonoBehaviour
{
    public GameObject enemyWeapon;
    public float enemyHealth;
    public float enemySpeed;
    public float minSpeedScale;
    public float maxSpeedScale;
    public float activationTime;

    [HideInInspector]
    public bool allowedToMove;
    [HideInInspector]
    public Vector2 direction;

    private Rigidbody2D rb;
    private Vector2 tmp;

    private void Start()
    {
        direction = new Vector2(Random.Range(0, 2) == 0 ? -1f : 1f, Random.Range(0, 2) == 0 ? -1f : 1f);
        rb = GetComponent<Rigidbody2D>();
        Instantiate(enemyWeapon, transform.position, Quaternion.identity, transform);
        StartCoroutine("activateEnemy");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            tmp = -transform.position * collision.contactCount;
            for (int i = collision.contactCount - 1; i >= 0; --i)
            {
                tmp += collision.GetContact(i).point;
            }
            if (Mathf.Abs(tmp.x) > Mathf.Abs(tmp.y))
            {
                if (tmp.x > 0)
                {
                    direction.x = -1f;
                }
                else
                {
                    direction.x = 1f;
                }
            }
            else if (Mathf.Abs(tmp.x) < Mathf.Abs(tmp.y))
            {
                if (tmp.y > 0)
                {
                    direction.y = -1f;
                }
                else
                {
                    direction.y = 1f;
                }
            }
            else
            {
                direction *= -1;
            }
            if (allowedToMove)
            {
                rb.velocity = direction * enemySpeed * Random.Range(minSpeedScale, maxSpeedScale);
            }
        }
        else if (collision.collider.CompareTag("Bullet"))
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
        rb.velocity = direction * enemySpeed;
        allowedToMove = true;
    }
}

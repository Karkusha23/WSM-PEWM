using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public float health = 3.0f;
    public float speed = 10.0f;
    public float activationTime = 0.5f;
    public float refreshPlayerTime = 0.1f;

    protected Rigidbody2D rigidBody;
    protected Transform player;
    protected Vector3 destination;

    protected abstract void onActivation();

    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            transform.parent.GetComponent<RoomController>().checkEnemyKilled();
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            takeDamage(collision.collider.GetComponent<Bullet>().damage);
        }
    }

    protected virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine("activateEnemy");
    }

    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        StartCoroutine("refreshPlayerPos");
        onActivation();
    }

    private IEnumerator refreshPlayerPos()
    {
        for (; ; )
        {
            destination = player.position - transform.position;
            yield return new WaitForSeconds(refreshPlayerTime);
        }
    }
}

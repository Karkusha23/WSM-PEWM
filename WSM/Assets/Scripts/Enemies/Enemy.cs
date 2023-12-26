using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public float health = 3.0f;
    public float speed = 10.0f;

    // Time between spawn and player attacking start
    public float activationTime = 0.5f;

    // How frequent does player position refreshing
    public float refreshPlayerTime = 0.1f;

    protected Rigidbody2D rigidBody;
    protected Transform player;
    protected Vector3 destination;

    // Called activationTime seconds after spawning
    protected abstract void onActivation();

    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            transform.parent.GetComponent<Room>().checkEnemyKilled();
            Destroy(gameObject);
        }
    }

    //Find weapon among children of gameObject
    public GameObject findWeapon()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.CompareTag("EnemyWeapon"))
            {
                return child;
            }
        }
        return null;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
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

    // Activate enemy on spawn
    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        StartCoroutine("refreshPlayerPos");
        onActivation();
    }

    // Refresh player position
    private IEnumerator refreshPlayerPos()
    {
        for (; ; )
        {
            destination = player.position - transform.position;
            yield return new WaitForSeconds(refreshPlayerTime);
        }
    }
}

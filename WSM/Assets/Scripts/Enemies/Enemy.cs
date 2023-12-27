using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    public float health = 3.0f;
    public float speed = 10.0f;

    // Time between spawn and player attacking start
    public float activationTime = 0.5f;

    // If enemy is able to build path to player. Set false if enemy don't need chase player
    public bool canBuildPaths = true;

    // Enemy will count waypoint as reached when this close to it
    public float waypointReachDistance = 0.1f;

    // How frequent does player position refreshing
    public float refreshPlayerTime = 0.1f;

    protected Rigidbody2D rigidBody;
    protected Transform player;
    protected Room room;
    protected Vector2 destination;
    protected RoomPath.Path path;

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
        room = transform.parent.GetComponent<Room>();
        StartCoroutine("activateEnemy");
    }

    protected virtual void Update()
    {
        if (canBuildPaths)
        {
            if (Vector3.Distance(transform.position, player.position) <= Room.tileSize)
            {
                destination = player.position - transform.position;
            }
            else if (!(path is null))
            {
                Vector3 waypoint = Room.RoomPointToLocal(path[1]) + transform.parent.position;
                destination = waypoint - transform.position;
                if (destination.magnitude < waypointReachDistance)
                {
                    path.RemoveAt(1);
                    if (path.Count > 1)
                    {
                        waypoint = Room.RoomPointToLocal(path[1]) + transform.parent.position;
                        destination = waypoint - transform.position;
                    }
                }
            }
        }
    }

    // Activate enemy on spawn
    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        if (canBuildPaths)
        {
            StartCoroutine("refreshPlayerPos");
        }
        onActivation();
    }

    // Refresh player position
    private IEnumerator refreshPlayerPos()
    {
        for (; ; )
        {
            path = RoomPath.BuildPath(transform.position - transform.parent.position, player.position - transform.parent.position, room.roomGrid);
            yield return new WaitForSeconds(refreshPlayerTime);
        }
    }
}

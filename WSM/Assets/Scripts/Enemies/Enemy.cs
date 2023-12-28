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

    // How frequent does paths refreshing
    public float refreshPathsTime = 0.1f;

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
            if (Vector3.Distance(transform.position, player.position) <= Room.tileSize / 2.0f)
            {
                destination = player.position - transform.position;
            }
            else if (!(path is null))
            {
                setDestinationToPath();
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
            path = RoomPath.BuildPathSmoothed(transform.position - transform.parent.position, player.position - transform.parent.position, room.roomGrid);
            yield return new WaitForSeconds(refreshPathsTime);
        }
    }

    // Set destination variable according to calculated path
    private void setDestinationToPath()
    {
        Vector3 waypoint = Room.RoomPointToLocal(path[1]) + transform.parent.position;
        Vector3 norm = getNormalToPath();
        norm *= norm.magnitude > 0.5f ? 3.0f : 1.0f;
        destination = waypoint - transform.position + norm;
        if (destination.magnitude < waypointReachDistance)
        {
            path.RemoveAt(0);
            if (path.Count > 1)
            {
                waypoint = Room.RoomPointToLocal(path[1]) + transform.parent.position;
                norm = getNormalToPath();
                norm *= norm.magnitude > 0.5f ? 3.0f : 1.0f;
                destination = waypoint - transform.position + norm;
            }
        }
    }

    // Get vector towards the line of path
    private Vector3 getNormalToPath()
    {
        Vector3 startVec = Room.RoomPointToLocal(path[0]);
        Vector3 endVec = Room.RoomPointToLocal(path[1]);
        Vector3 localPosition = transform.position - transform.parent.position;

        // Vector of line of path
        Vector2 pathVec = endVec - startVec;

        // Normal vector
        Vector2 norm = new Vector2(pathVec.y, -pathVec.x);

        // If normal facing wrong side
        if (Vector3.Dot(endVec - localPosition, norm) < 0.0f)
        {
            norm = -norm;
        }
        norm.Normalize();

        // Multiply by distance to line of path
        norm *= Mathf.Abs((localPosition.x - startVec.x) * norm.x + (localPosition.y - startVec.y) * norm.y);

        return norm;
    }
}

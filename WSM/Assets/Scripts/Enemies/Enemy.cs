using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class Enemy : MonoBehaviour
{
    public float health = 3.0f;
    public float speed = 10.0f;

    // Time between spawn and player attacking start
    public float activationTime = 0.5f;

    // If enemy is able to build path to player. Set false if enemy don't need chase player
    public bool canBuildPaths = true;

    // Enemy will count waypoint as reached when this close to it
    public float waypointReachDistance = 0.05f;

    // How frequent does paths refreshing
    public float refreshPathsTime = 0.1f;

    protected Rigidbody2D rigidBody;
    protected Transform player;
    protected Room room;

    // vec from current location to next path waypoint
    protected Vector3 destination;

    protected RoomPath.Path path;

    // Called activationTime seconds after spawning
    protected abstract void onActivation();

    // Defines to which point enemy is going to build paths
    // Default is player position
    // Override for diffrenet destinations
    public virtual Vector3 getGoalLocalPosition()
    {
        return player.position - transform.parent.position;
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
            if (canTravelDirectlyToPlayer())
            {
                destination = player.position - transform.position;
            }
            else
            {
                setDestinationToPath();
            }
        }
    }

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

    // Returns local position that is not affected by parent scale
    public Vector3 getUnscaledLocalPosition()
    {
        return transform.position - transform.parent.position;
    }

    // If enemy has direct sight on player
    public bool canSeePlayer()
    {
        return RoomPath.Raycast(getUnscaledLocalPosition(), player.position - transform.parent.position, room.roomGrid, false);
    }

    // If enemy can travel directly to the player
    public bool canTravelDirectlyToPlayer()
    {
        return RoomPath.Raycast(getUnscaledLocalPosition(), player.position - transform.parent.position, room.roomGrid, true);
    }

    public bool canTravelDirectlyToGoal()
    {
        return RoomPath.Raycast(getUnscaledLocalPosition(), getGoalLocalPosition(), room.roomGrid, true);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            takeDamage(collision.collider.GetComponent<Bullet>().damage);
        }
    }

    // Activate enemy on spawn
    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        if (canBuildPaths)
        {
            StartCoroutine("refreshPlayerPath");
        }
        onActivation();
    }

    // Refresh path to player
    private IEnumerator refreshPlayerPath()
    {
        for (; ; )
        {
            Vector3 localPosition = getUnscaledLocalPosition();
            Vector3 goalPosition = getGoalLocalPosition();
            if (path is null || path.Count == 0 || Room.LocalToRoomPoint(goalPosition) != path.Last())
            {
                path = RoomPath.BuildPathSmoothed(localPosition, goalPosition, room.roomGrid);
            }
            yield return new WaitForSeconds(refreshPathsTime);
        }
    }

    // Set destination variable according to calculated path
    private void setDestinationToPath()
    {
        if (path is null || path.Count == 0)
        {
            destination = Vector3.zero;
            return;
        }
        destination = getDestination();
        if (destination.magnitude < waypointReachDistance)
        {
            path.RemoveAt(0);
            if (path.Count > 1)
            {
                destination = getDestination();
            }
        }
    }

    // Returns destination in which enemy have to go according to path
    private Vector3 getDestination()
    {
        Vector3 waypoint = Room.RoomPointToLocal(path[1]);
        Vector3 norm = getNormalToPath();
        Vector3 result = waypoint - getUnscaledLocalPosition();
        if (!RoomPath.Raycast(getUnscaledLocalPosition(), getUnscaledLocalPosition() + result.normalized * Room.tileSize, room.roomGrid, true))
        {
            result += norm * result.magnitude * 10.0f;
        }
        return result;
    }

    // Get vector towards the line of path
    private Vector3 getNormalToPath()
    {
        Vector3 startVec = Room.RoomPointToLocal(path[0]);
        Vector3 endVec = Room.RoomPointToLocal(path[1]);
        Vector3 localPosition = getUnscaledLocalPosition();

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

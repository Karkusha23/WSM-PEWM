using UnityEngine;
using System.Collections;
using System.IO;

// Base class for enemy
// Implements basic mechanics such as health, taking damage from player bullets, moving and pathfinding

public abstract class Enemy : MonoBehaviour
{
    public float health = 3.0f;
    public float speed = 10.0f;

    // Time between spawn and player attacking start
    public float activationTime = 0.5f;

    // If enemy is able to build paths. Set false if enemy don't need it
    // If set true, enemy will be moving to getGoalLocalPosition()
    [HideInInspector]
    public bool canBuildPaths = true;

    // Enemy will count waypoint as reached when this close to it
    public float waypointReachDistance = 0.05f;

    // How frequent does paths refreshing
    public float refreshPathsTime = 0.1f;

    protected Rigidbody2D rigidBody;
    protected Transform player;
    protected Room room;

    // Vector in which enemy must move according to path
    public Vector3 destination { get => getDestination(); }

    // Defines if enemy is allowed to act
    protected bool canAct;

    // Current path
    protected RoomPath.Path path;

    // If enemy has reached end of its path
    private bool hasReachedPathEnd = false;

    [HideInInspector]
    public bool HasReachedPathEnd { get => hasReachedPathEnd; }

    // Called activationTime seconds after spawning
    protected virtual void onActivation()
    {
        canAct = true;
    }

    // Defines to which point enemy is going to build paths
    // Default is player position
    // Override for diffrenet destinations
    public virtual Vector3 getGoalLocalPosition()
    {
        return player.position - transform.parent.position;
    }

    // Defines path that enemy is following
    // Default is build path to goal local position
    // Override for different paths
    public virtual RoomPath.Path getPath()
    {
        return RoomPath.BuildPath(getUnscaledLocalPosition(), getGoalLocalPosition(), room.roomGrid);
    }

    // Called when following path to define speed
    // Default is speed variable
    // Override for speed differentiation
    public virtual float getSpeed()
    {
        return speed;
    }

    protected virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        room = transform.parent.GetComponent<Room>();
        canAct = false;
        StartCoroutine("activateEnemy");
    }

    protected virtual void Update()
    {
        if (canBuildPaths && canAct)
        {
            if (destination != Vector3.zero)
            {
                rigidBody.velocity = destination.normalized * getSpeed();
            }
            else
            {
                rigidBody.velocity = Vector3.zero;
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Bullet"))
        {
            takeDamage(collision.collider.GetComponent<Bullet>().damage);
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

    // If enemy can travel directly to its goal
    public bool canTravelDirectlyToGoal()
    {
        return RoomPath.Raycast(getUnscaledLocalPosition(), getGoalLocalPosition(), room.roomGrid, true);
    }

    // Recalculate path to goal
    public void refreshGoalPath()
    {
        path = getPath();
        hasReachedPathEnd = false;
    }

    public void drawPath()
    {
        if (path is null)
        {
            return;
        }
        for (int i = 0; i < path.Count - 1; ++i)
        {
            Debug.DrawLine(RoomPath.RoomPointToLocal(path[i]) + transform.parent.position, RoomPath.RoomPointToLocal(path[i + 1]) + transform.parent.position, Color.green, refreshPathsTime);
        }
    }

    // Activate enemy on spawn
    private IEnumerator activateEnemy()
    {
        yield return new WaitForSeconds(activationTime);
        if (canBuildPaths)
        {
            StartCoroutine("findingPathSequence");
        }
        onActivation();
    }

    // Refresh path to goal
    private IEnumerator findingPathSequence()
    {
        for (; ; )
        {
            refreshGoalPath();
            drawPath();
            yield return new WaitForSeconds(refreshPathsTime);
        }
    }

    // Get vector in which enemy must move according to path
    private Vector3 getDestination()
    {
        if (hasReachedPathEnd)
        {
            return Vector3.zero;
        }

        if (path is null || path.Count < 2)
        {
            hasReachedPathEnd = true;
            return Vector3.zero;
        }

        if (Vector3.Distance(getUnscaledLocalPosition(), RoomPath.RoomPointToLocal(path[1])) < waypointReachDistance)
        {
            path.RemoveAt(0);
            if (path.Count < 2)
            {
                hasReachedPathEnd = true;
                return Vector3.zero;
            }
        }

        if (canTravelDirectlyToGoal())
        {
            return getGoalLocalPosition() - getUnscaledLocalPosition();
        }
        
        return getPathDestination();
    }

    // Returns destination in which enemy have to go according to path
    private Vector3 getPathDestination()
    {
        Vector3 waypoint = RoomPath.RoomPointToLocal(path[1]);
        Vector3 norm = getNormalToPath();
        Vector3 result = waypoint - getUnscaledLocalPosition();
        if (!RoomPath.Raycast(getUnscaledLocalPosition(), getUnscaledLocalPosition() + result.normalized * RoomPath.tileSize, room.roomGrid, true))
        {
            result += norm * result.magnitude * 10.0f;
        }
        return result;
    }

    // Get vector towards the line of path
    private Vector3 getNormalToPath()
    {
        Vector3 startVec = RoomPath.RoomPointToLocal(path[0]);
        Vector3 endVec = RoomPath.RoomPointToLocal(path[1]);
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

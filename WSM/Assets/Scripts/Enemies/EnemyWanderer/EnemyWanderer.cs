using System.Collections;
using UnityEngine;

public class EnemyWanderer : Enemy
{
    public float timeWaitOnTravelPoint = 1.0f;

    [HideInInspector]
    public bool canMove = false;

    protected Vector3 travelPoint;
    protected bool isUpdatingTravelPointInProcess = false;

    protected override void onActivation()
    {
        canMove = true;
        waypointReachDistance = 0.1f;
        travelPoint = RoomPath.GetRandomTravablePointInRadius(getUnscaledLocalPosition(), room.roomGrid, 3, 10);
    }

    protected virtual void Update()
    {
        if (canMove)
        {
            rigidBody.velocity = destination.normalized * speed;
        }
    }

    public override Vector3 getGoalLocalPosition()
    {
        if (!isUpdatingTravelPointInProcess && HasReachedPathEnd)
        {
            StartCoroutine("updateTravelPoint");
        }
        return travelPoint;
    }

    private IEnumerator updateTravelPoint()
    {
        isUpdatingTravelPointInProcess = true;
        yield return new WaitForSeconds(timeWaitOnTravelPoint);
        travelPoint = RoomPath.GetRandomTravablePointInRadius(getUnscaledLocalPosition(), room.roomGrid, 3, 10);
        isUpdatingTravelPointInProcess = false;
    }
}

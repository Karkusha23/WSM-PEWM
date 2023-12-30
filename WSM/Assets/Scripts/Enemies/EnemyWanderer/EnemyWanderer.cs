using System.Collections;
using UnityEngine;

// Enemy that just wanders around

public class EnemyWanderer : Enemy
{
    public float timeWaitOnTravelPoint = 1.0f;

    protected float timeTravelPointRefresh = 0.2f;

    protected Vector3 travelPoint;

    protected override void onActivation()
    {
        waypointReachDistance = 0.1f;
        travelPoint = RoomPath.GetRandomTravablePointInRadius(getUnscaledLocalPosition(), room.roomGrid, 3, 10);
        StartCoroutine("updateTravelPoint");
        base.onActivation();
    }

    public override Vector3 getGoalLocalPosition()
    {
        return travelPoint;
    }

    private IEnumerator updateTravelPoint()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(timeTravelPointRefresh);
            if (HasReachedPathEnd)
            {
                yield return new WaitForSeconds(timeWaitOnTravelPoint);
                travelPoint = travelPoint = RoomPath.GetRandomTravablePointInRadius(getUnscaledLocalPosition(), room.roomGrid, 3, 10);
                refreshGoalPath();
            }
        }
    }
}

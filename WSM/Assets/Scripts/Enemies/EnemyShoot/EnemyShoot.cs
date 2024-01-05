using UnityEngine;
using System.Collections;
using System.Linq;

// Enemy that chases player but tries to keep some distance and shoots

public class EnemyShoot : Enemy
{
    public GameObject weapon;
    public float chaseDistance = 5.0f;
    public float reloadTime = 0.8f;

    private EnemyShootWeapon weaponCon;

    protected override void Start()
    {
        if (weapon == null)
        {
            weapon = findWeapon();
        }
        weaponCon = weapon.GetComponent<EnemyShootWeapon>();

        base.Start();
    }

    protected override void onActivation()
    {
        canAct = true;
        StartCoroutine("shootingSequence");
    }

    public override RoomPath.Path getPath()
    {
        return RoomPath.BuildPathToPointWithSightOn(getUnscaledLocalPosition(), player.position - transform.parent.position, room.roomGrid, chaseDistance);
    }

    public override Vector3 getGoalLocalPosition()
    {
        if (path is null || path.Count == 0)
        {
            return getUnscaledLocalPosition();
        }
        return RoomPath.RoomPointToLocal(path.Last());
    }

    private IEnumerator shootingSequence()
    {
        for (; ; )
        {
            if (canSeePlayer())
            {
                weaponCon.shoot();
            }
            yield return new WaitForSeconds(reloadTime);
        }
    }
}

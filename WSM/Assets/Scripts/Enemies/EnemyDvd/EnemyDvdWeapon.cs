using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EnemyDvdWeapon : Weapon
{
    public float timeBetweenBullets = 0.2f;
    public int bulletAmount = 5;

    public float directionChangeAlpha = 0.5f;

    [HideInInspector]
    bool isShooting = false;

    [HideInInspector]
    public Vector2 targetDirection = new Vector2(0.0f, 1.0f);

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (Vector2.Distance(direction.normalized, targetDirection.normalized) > 0.1f)
        {
            direction = Vector2.Lerp(direction.normalized, targetDirection.normalized, directionChangeAlpha);
        }
        else
        {
            direction = targetDirection;
        }

        base.Update();
    }

    public override void shoot()
    {
        if (isShooting)
        {
            return;
        }
        isShooting = true;
        StartCoroutine("shootingExecution");
    }

    private IEnumerator shootingExecution()
    {
        for (int i = 0; i < bulletAmount; ++i)
        {
            for (int j = 0; j < shootingPoints.Count; ++j)
            {
                launchBullet(j, shootingPoints[j].transform.position - transform.position);
            }
            yield return new WaitForSeconds(timeBetweenBullets);
        }
        isShooting = false;
    }
}
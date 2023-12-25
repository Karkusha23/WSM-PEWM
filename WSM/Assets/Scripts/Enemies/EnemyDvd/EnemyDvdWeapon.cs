using UnityEngine;
using System.Collections;

public class EnemyDvdWeapon : Weapon
{
    public float timeBetweenBullets = 0.2f;
    public int bulletAmount = 5;

    public float directionChangeSpeed = 1.0f;

    [HideInInspector]
    bool isShooting = false;

    [HideInInspector]
    public Quaternion targetRotation;

    protected override void Start()
    {
        targetRotation = Quaternion.identity;
        base.Start();
    }

    protected override void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, directionChangeSpeed * Time.deltaTime);
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
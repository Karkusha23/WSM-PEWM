using UnityEngine;

public class EnemyShootWeapon : Weapon
{
    private Transform player;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        base.Start();
    }

    protected override void Update()
    {
        direction = player.position - transform.position;

        base.Update();
    }

    public override void shoot()
    {
        launchBullet(0, direction);
    }

}

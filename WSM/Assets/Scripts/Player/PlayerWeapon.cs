using UnityEngine;

public class PlayerWeapon : Weapon
{
    public float reloadTimeMult;
    public float damageMult;
    public bool canPlayerControlWeapon = true;

    public GameObject weaponDroppedSample;

    private float timer;
    private Player player;

    protected override void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        base.Start();
    }

    protected override void Update()
    {
        if (canPlayerControlWeapon)
        {
            Vector3 direction3 = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            direction.x = direction3.x;
            direction.y = direction3.y;

            base.Update();

            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
            }
            if (timer <= 0f && Input.GetMouseButton(0))
            {
                shoot();
            }
        }
    }

    public override void shoot()
    {
        GameObject bullet = launchBullet(0, direction);
        bullet.GetComponent<Bullet>().damage = player.damage * damageMult;
        timer = player.reloadTime * reloadTimeMult;
    }
}

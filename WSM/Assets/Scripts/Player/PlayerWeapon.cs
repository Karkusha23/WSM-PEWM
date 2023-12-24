using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerWeapon : Weapon
{
    public Player player;

    public float reloadTimeMult;
    public float damageMult;

    private float timer;

    public bool canPlayerControlWeapon = true;

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

            if (timer <= 0f && Input.GetMouseButton(0))
            {
                Shoot();
            }
            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
            }
        }
    }

    public override void Shoot()
    {
        GameObject bullet = Instantiate(bulletSample, shootingPoints[0].transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
        bullet.GetComponent<Bullet>().damage = player.damage * damageMult;
        timer = player.reloadTime * reloadTimeMult;
    }
}

using UnityEngine;
using System.Collections;

public class EnemyDvd : Enemy
{
    public float reloadTime = 2.5f;
    public float minSpeedScale = 0.9f;
    public float maxSpeedScale = 1.2f;

    public GameObject weapon;

    [HideInInspector]
    public Vector2 direction;

    private EnemyDvdWeapon weaponCon;

    protected override void Start()
    {
        if (weapon == null)
        {
            weapon = findWeapon();
        }
        weaponCon = weapon.GetComponent<EnemyDvdWeapon>();
        direction = new Vector2(Random.Range(0, 2) == 0 ? -1f : 1f, Random.Range(0, 2) == 0 ? -1f : 1f);
        base.Start();
    }

    protected override void onActivation()
    {
        rigidBody.velocity = direction.normalized * speed;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 tmp = -transform.position * collision.contactCount;
            for (int i = collision.contactCount - 1; i >= 0; --i)
            {
                tmp += collision.GetContact(i).point;
            }
            if (Mathf.Abs(tmp.x) > Mathf.Abs(tmp.y))
            {
                if (tmp.x > 0)
                {
                    direction.x = -1f;
                }
                else
                {
                    direction.x = 1f;
                }
            }
            else if (Mathf.Abs(tmp.x) < Mathf.Abs(tmp.y))
            {
                if (tmp.y > 0)
                {
                    direction.y = -1f;
                }
                else
                {
                    direction.y = 1f;
                }
            }
            else
            {
                direction *= -1;
            }
            rigidBody.velocity = direction.normalized * speed * Random.Range(minSpeedScale, maxSpeedScale);
        }
        base.OnCollisionEnter2D(collision);
    }

    private IEnumerator shootingSequence()
    {
        for (; ; )
        {
            weaponCon.shoot();
            yield return new WaitForSeconds(reloadTime);
        }
    }
}

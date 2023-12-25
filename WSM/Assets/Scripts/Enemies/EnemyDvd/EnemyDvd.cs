using UnityEngine;
using System.Collections;

public class EnemyDvd : Enemy
{
    public float reloadTime = 1.5f;
    public float minSpeedScale = 0.9f;
    public float maxSpeedScale = 1.2f;

    public bool isAllowedToMoveWhileShooting = false;

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
        StartCoroutine("shootingSequence");
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            bounceOffWall(collision);
        }
        base.OnCollisionEnter2D(collision);
    }

    // Change move direction upon hitting wall like a DVD icon
    private void bounceOffWall(Collision2D collision)
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

    private IEnumerator shootingSequence()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        for (; ; )
        {
            Vector2 velocity = rigidBody.velocity;
            if (!isAllowedToMoveWhileShooting)
            {
                // Stop while shooting
                rigidBody.velocity = Vector2.zero;
            }
            weaponCon.shoot();

            yield return new WaitForSeconds(weaponCon.bulletAmount * weaponCon.timeBetweenBullets);

            // Move again
            rigidBody.velocity = velocity;

            // Rotate weapons on 45 deg clockwise
            weaponCon.targetRotation = Quaternion.Euler(0.0f, 0.0f, weaponCon.targetRotation.eulerAngles.z + 45.0f);

            yield return new WaitForSeconds(reloadTime);
        }
    }
}

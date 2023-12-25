using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    // List of points from which projectiles can be spawned
    public List<GameObject> shootingPoints;

    // Direction which weapon is facing 
    public Vector2 direction = new Vector2(1.0f, -1.0f);

    // Offset of weapon angle
    public float angleOffset;


    public GameObject bulletPrefab;
    public float bulletSpeed;

    protected virtual void Start()
    {
        // if none of shootingPoints assigned, find ones among the children of gameObject
        if (shootingPoints.Count == 0)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (transform.GetChild(i).CompareTag("ShootingPoint"))
                {
                    shootingPoints.Add(child);
                }
            }
        }
    }

    protected virtual void Update()
    {
        // Rotate weapon using direction variable
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation + angleOffset);
    }

    // Execute the attack
    public abstract void shoot();

    // Launch bullet from specific shooting point in given direction
    public GameObject launchBullet(int shootingPointIndex, Vector2 bulletDirection)
    {
        GameObject bullet = Instantiate(bulletPrefab, shootingPoints[shootingPointIndex].transform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().velocity = bulletDirection.normalized * bulletSpeed;
        return bullet;
    }
}

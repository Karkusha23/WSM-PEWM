using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public List<GameObject> shootingPoints;
    public Vector2 direction = new Vector2(1.0f, -1.0f);
    public float angleOffset;

    public GameObject bulletSample;
    public float bulletSpeed;

    protected virtual void Start()
    {
        if (shootingPoints.Count == 0)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).CompareTag("ShootingPoint"))
                {
                    shootingPoints.Add(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    protected virtual void Update()
    {
        float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation + angleOffset);
    }

    public abstract void Shoot();
}

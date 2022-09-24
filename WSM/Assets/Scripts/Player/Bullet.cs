using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
        if (collision.collider.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            return;
        }
    }
}

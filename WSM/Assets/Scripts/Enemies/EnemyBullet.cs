using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
        if (collision.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }
    }
}

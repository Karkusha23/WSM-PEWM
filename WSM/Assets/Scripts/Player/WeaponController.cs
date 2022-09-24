using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public float angleOffset;
    public Vector3 shootingPointOffset;
    public GameObject shootingPoint;
    public GameObject bullet;
    public float bulletSpeed;
    public float reloadTime;
    public float damage;

    private GameObject sp;
    private GameObject bul;
    private float timer;
    private Vector2 tmp;
    private Vector3 direction;
    private float rotation;

    private void Start()
    {
        sp = Instantiate(shootingPoint, transform.position + shootingPointOffset, Quaternion.identity, transform);
    }

    private void Update()
    {
        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation + angleOffset);

        if (Input.GetMouseButton(0) && timer <= 0)
        {
            bul = Instantiate(bullet, sp.transform.position, Quaternion.identity);
            tmp.x = direction.x;
            tmp.y = direction.y;
            bul.GetComponent<Rigidbody2D>().velocity = tmp.normalized * bulletSpeed;
            bul.GetComponent<Bullet>().damage = damage;
            timer = reloadTime;
        }

        timer -= Time.deltaTime;
    }
}

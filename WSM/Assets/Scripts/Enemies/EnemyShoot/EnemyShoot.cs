using UnityEngine;
using System.Collections;

public class EnemyShoot : Enemy
{
    public GameObject weapon;
    public float chaseDistance;

    [HideInInspector]
    public bool canChase = false;

    protected override void Start()
    {
        Instantiate(weapon, transform.position, Quaternion.identity, transform);
        base.Start();
    }

    protected override void onActivation()
    {
        canChase = true;
    }

    protected virtual void Update()
    {
        if (canChase)
        {
            rigidBody.velocity = destination.magnitude > chaseDistance ? destination.normalized * speed : Vector2.zero;
        }
    }
}

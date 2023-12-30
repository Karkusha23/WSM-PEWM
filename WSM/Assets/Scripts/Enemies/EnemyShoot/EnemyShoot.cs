using UnityEngine;
using System.Collections;

public class EnemyShoot : Enemy
{
    public GameObject weapon;
    public float chaseDistance = 5.0f;
    public float reloadTime = 0.8f;

    [HideInInspector]
    public bool canChase = false;

    private EnemyShootWeapon weaponCon;

    protected override void Start()
    {
        if (weapon == null)
        {
            weapon = findWeapon();
        }
        weaponCon = weapon.GetComponent<EnemyShootWeapon>();

        base.Start();
    }

    protected override void onActivation()
    {
        canChase = true;
        StartCoroutine("shootingSequence");
    }

    protected virtual void Update()
    {
        if (canChase)
        {
            rigidBody.velocity = (transform.position - player.position).magnitude > chaseDistance ? destination.normalized * speed : Vector2.zero;
        }
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

using UnityEngine;
using System.Collections;

// Enemy that chases player but tries to keep some distance and shoots

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

    private IEnumerator shootingSequence()
    {
        for (; ; )
        {
            weaponCon.shoot();
            yield return new WaitForSeconds(reloadTime);
        }
    }
}

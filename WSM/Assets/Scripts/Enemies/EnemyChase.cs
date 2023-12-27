using UnityEngine;

public class EnemyChase : Enemy
{
    [HideInInspector]
    public bool canChase = false;

    protected override void onActivation()
    {
        canChase = true;
    }

    protected override void Update()
    {
        base.Update();

        if (canChase)
        {
            rigidBody.velocity = destination.normalized * speed;
        }
    }
}

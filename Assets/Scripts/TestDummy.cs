using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TestDummy : Enemy
{
    [SerializeField]
    private GameObject arrow = null;


    protected virtual void Start()
    {
        initialAttack = StartCoroutine(Attack());
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        switch (state)
        {
            case States.Attacking:
                Instantiate(arrow, this.transform.position + new Vector3(0.0f, 0.3f), Quaternion.Euler(0, 0, 0));
                break;
        }
    }

    public override void Hurt(float damage, Vector2 hitForce)
    {
        base.Hurt(damage, hitForce);
        CancelAttack();
        initialAttack = StartCoroutine(Attack());
    }
}

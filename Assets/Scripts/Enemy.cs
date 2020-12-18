using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    [SerializeField]
    protected float attackTime = 0;
    protected bool isAttack = false, attackWaiting = false;

    protected Coroutine initialAttack = null;

    public Enemy()
    {
    }
    public Enemy(float newHealth)
    {
        health = newHealth;
    }

    protected override void Update()
    {
        base.Update();
        if(actionable && attackWaiting)
        {
            state = States.Attacking;
            attackWaiting = false;
            actionable = false;
        }
    }
    protected override void FixedUpdate()
    {
        isGrounded = tDetection.GetGrounded();

        if (!stateLock && actionable)
        {
            if (isGrounded)
            {
                if (Mathf.Approximately(horizontal, 0))
                {
                    if (Mathf.Abs(rb2D.velocity.x) > 0)
                    {
                        /*if (state == States.Run || state == States.RunStop)
                        {
                            state = States.RunStop;
                        }
                        else
                        {
                            state = States.Stopping;
                        }*/
                        state = States.Stopping;
                    }
                    else
                    {
                        state = States.Still;
                    }
                }
            }
        }

        switch (state)
        {
            case States.Attacking:
                actionable = true;
                initialAttack = StartCoroutine(Attack());
                break;
            case States.Dead:
                CancelAttack();
                break;
        }

        base.FixedUpdate();
    }

    protected virtual void CancelAttack()
    {
        isAttack = false;
        StopCoroutine(initialAttack);
    }

    protected virtual IEnumerator Attack()
    {
        isAttack = true;
        yield return new WaitForSeconds(attackTime);
        if (actionable)
        {
            state = States.Attacking;
            actionable = false;
        }
        else
            attackWaiting = true;
        isAttack = false;
    }
}

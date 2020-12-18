using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Damageable
{
    public enum States
    {
        Still,
        Stopping,
        Running,
        Jumping,
        Midair,
        Attacking,
        Turnaround,
        Knockdown,
        Dead,
        Walk,//0%-75% same direction
        InitalRun,
        Run,//75%-100% same direction
        RunStop,//0% was running
        Hitstun,
        DoubleJumping
    }

    [SerializeField]
    protected States state = States.Still;
    protected States oldState = States.Still;
    [SerializeField]
    protected float friction = 0f, frictionBoundary = 0f, pushSpeed = 0f, maxPushSpeed = 0f, horizontal = 0f, dampening = 0f, knockdownTime = 0f;
    [SerializeField]
    protected PhysicsMaterial2D regularMat = null, knockdownMat = null;
    [SerializeField]
    protected bool stateLock = false, actionable = true;
    protected bool isGrounded = false, lookRight = true, isKnockdown = false, knockdownStart = false;

    protected TerrainDetection tDetection;
    protected BoxCollider2D hitBox;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Pushbox pushbox;

    protected Vector2 veloTarget, veloChange;

    protected Coroutine knockdownTimer = null;

    public Character()
    {
    }
    public Character(float newHealth)
    {
        health = newHealth;
    }

    protected override void Awake()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        tDetection = GetComponentInChildren<TerrainDetection>();
        pushbox = GetComponentInChildren<Pushbox>();
    }

    protected virtual void Update()
    {
        CheckLookDirection();

        if (oldState != state)
        {
            switch (oldState)
            {
                case States.Knockdown:
                    rb2D.sharedMaterial = regularMat;
                    knockdownStart = false;
                    break;
            }
            oldState = state;
        }
    }

    protected override void FixedUpdate()
    {
        if (pushbox.GetPush() != 0)
        {
            veloTarget = new Vector2(-maxPushSpeed * pushbox.GetPush(), 0);
            veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
            veloChange.x = Mathf.Clamp(veloChange.x, -pushSpeed, pushSpeed);
            rb2D.AddForce(veloChange, ForceMode2D.Impulse);
        }

        switch (state)
        {
            case States.Still:
                break;

            case States.Stopping:
                //Stop from affecting pushbox
                veloTarget = new Vector2(-maxPushSpeed * pushbox.GetPush(), 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -dampening, dampening);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                if (Mathf.Approximately(rb2D.velocity.x, 0))
                    state = States.Still;
                break;

            case States.Knockdown:
                if(knockdownStart == false)
                {
                    knockdownTimer = StartCoroutine(Knockdown());
                }
                rb2D.sharedMaterial = knockdownMat;
                if (tDetection.GetWall())
                {
                    //veloTarget = (Vector2.Reflect(rb2D.velocity, Vector2.right));
                    //veloChange = veloTarget - rb2D.velocity;
                    //rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                    Debug.Log("Reflected");
                }
                if (tDetection.GetGrounded() && rb2D.velocity.x < frictionBoundary)
                {
                    veloTarget = new Vector2(0, 0);
                    veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                    veloChange.x = Mathf.Clamp(veloChange.x, -friction, friction);
                    rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                    /*if (Mathf.Approximately(rb2D.velocity.x, 0) && Mathf.Approximately(rb2D.velocity.y, 0))
                    {
                        actionable = true;
                        stateLock = false;
                        state = States.Still;
                    }*/
                }
                break;
        }
        base.FixedUpdate(); //gravity
    }
    protected bool CheckLookDirection()
    {
        if (actionable)
        {
            if (isGrounded && state != States.Turnaround && !Mathf.Approximately(horizontal, 0))
            {
                Vector3 flip = transform.localScale;
                if (horizontal > 0)
                {
                    lookRight = true;
                    flip.x = 1;
                }
                else
                {
                    lookRight = false;
                    flip.x = -1;
                }
                transform.localScale = flip;

            }
        }

        return lookRight;
    }

    public bool GetLookDirection() //Right = true, left = false
    {
        return lookRight;
    }

    void CancelKnockdown()
    {
        StopCoroutine(knockdownTimer);
        isKnockdown = false;
        stateLock = false;
        actionable = true;
    }

    protected virtual IEnumerator Knockdown()
    {
        knockdownStart = true;
        isKnockdown = true;
        yield return new WaitForSeconds(knockdownTime);
        isKnockdown = false;
        stateLock = false;
        actionable = true;
    }

    public override void Hurt(float damage, Vector2 hitForce)
    {
        if (damageable)
        {
            state = States.Knockdown;
            stateLock = true;
            actionable = false;
            knockdownStart = false;

            base.Hurt(damage, hitForce);

            rb2D.AddForce(hitForce, ForceMode2D.Impulse);
            Debug.Log("Hit for " + damage + "\nVector is " + hitForce);
        }
    }

}

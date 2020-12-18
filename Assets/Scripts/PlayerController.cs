using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character
{
    [SerializeField]
    private float walkSpeed = 0f, maxWalkSpeed = 0f, initialWalkTime = 0f, initialRunTime = 0.5f, runSpeed = 0f, maxRunSpeed = 0f, runDampening = 0f,
    turnDampening = 0f, attackDampening = 0f, jumpSpeed = 0f, maxJumpSpeed = 0f, dblJumpSpeed = 0f, maxDblJumpSpeed = 0f, airSpeed = 0f, negativeAirSpeed = 0f,
    maxAirSpeed = 0f, fastFallXSpeed = 0f, cancelJumpSpeed = 0f, fastFallSpeed = 0f, maxFastFallSpeed = 0f, maxVeloChange = 0f, vertical, maxHorz = 0f;

    [SerializeField]
    Vector2 playerVelo;
    private bool walkDown = false, doubleJump = true, isDashing = false, cancelJump = false, fastFalling = false,
    zeroVert = false, isWalking = false, walkStart = false, runJump = false; //stateLock = false, actionable = true;

    //walkStart is for starting the coroutine and only runs once per walk. isWalking is for if the coroutine is running.
    Coroutine initialRun = null, initialWalk = null;

    /*enum States
    {
        Walk,//0%-75% same direction
        InitalRun,
        Run,//75%-100% same direction
        Turnaround,// other direction not 0%
        Stopping,//0% moving
        RunStop,//0% was running
        Still,//0% not moving
        Attacking,
        Hitstun,
        Knockdown,
        Jumping,
        DoubleJumping,
        Midair,
        Dead
    }*/
    protected override void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        hitBox = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tDetection = GetComponentInChildren<TerrainDetection>();
        pushbox = GetComponentInChildren<Pushbox>();
    }

    protected override void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        walkDown = Input.GetButton("Walk");
        
        //For all the cancelable actions like jumping and dodging (?)
        if (actionable)
        {
            if (Input.GetButtonDown("Jump") && doubleJump)
            {
                cancelJump = false;
                if (isGrounded)
                {
                    runJump = (state == States.Run || state == States.InitalRun);
                    state = States.Jumping;
                }
                else
                    state = States.DoubleJumping;
                stateLock = true;
            }

            if (!stateLock)
            {
                if(Input.GetButtonDown("Attack"))
                {
                    stateLock = true;
                    actionable = false;
                    state = States.Attacking;
                    animator.SetTrigger("Attack"); //exit stuff will be taken care of by the animator
                    GameManager.Instance.ChangeAttack("TestAttack");
                }
            }
        }

        if (Input.GetButtonUp("Jump")) //Midair state checks for velo, don't worry about it
            cancelJump = true;
        if (vertical >= -0.6)
        {
            zeroVert = true;
        }
        else
        {
            if (zeroVert && vertical < -0.6)
                fastFalling = true;
            zeroVert = false;
        }

        CheckLookDirection();

        if (oldState != state)
        {
            switch (oldState)
                {
                case States.Walk:
                    walkStart = false;
                    CancelInitialWalk();
                    break;

                case States.InitalRun:
                    if (isDashing)
                    {
                        bool tempLock = stateLock;
                        CancelInitialRun();
                        if (tempLock)
                            stateLock = true;
                    }
                    break;

                case States.Run:
                    pushbox.EnablePush();
                    break;

                case States.Attacking:
                    break;

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
        isGrounded = tDetection.GetGrounded();

        if (isGrounded)
        {
            doubleJump = true;
            fastFalling = false;
        }

        float horzSign = Mathf.Sign(horizontal);
        if (horizontal == 0)
            horzSign = 0;

        if (!stateLock && actionable)
        {
            if (isGrounded)
            {
                if (Mathf.Approximately(horizontal, 0))
                {
                    if (Mathf.Abs(rb2D.velocity.x) > 0)
                    {
                        if (state == States.Run || state == States.RunStop)
                        {
                            state = States.RunStop;
                        }
                        else
                        {
                            state = States.Stopping;
                        }
                    }
                    else
                    {
                        state = States.Still;
                    }
                }
                else
                {
                    bool runTurn = state == States.Run || state == States.RunStop;
                    if (runTurn && Mathf.Sign(horizontal) != Mathf.Sign(rb2D.velocity.x) && !Mathf.Approximately(rb2D.velocity.x, 0))
                    {
                        state = States.Turnaround;
                    }
                    else
                    {
                        bool run;

                        if (Mathf.Abs(horizontal) > 0.75 && !walkDown && (state != States.Walk || isWalking))
                        {
                            run = true;
                        }
                        else
                        {
                            run = false;
                        }

                        if (run)
                        {
                            if (state != States.Run)
                            {
                                state = States.InitalRun;
                                pushbox.DisablePush();
                            }
                        }
                        else
                        {
                            if (runTurn)
                            {
                                state = States.RunStop;
                            }
                            else
                            {
                                state = States.Walk;
                            }
                        }
                    }
                }
            }
            else
            {
                state = States.Midair;
            }
        }

        base.FixedUpdate();

        switch (state)
        {
            case States.Walk:
                if (!walkStart)
                {
                    //animator.SetBool("Walking", true);
                    initialWalk = StartCoroutine(InitialWalk());
                }
                veloTarget = new Vector2(horizontal * maxWalkSpeed, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -walkSpeed, walkSpeed);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                animator.SetFloat("WalkSpeedMultiplier", Mathf.Abs(horizontal));
                break;

            case States.InitalRun:
                if (!isDashing)
                    initialRun = StartCoroutine(InitalRun());
                if (horizontal != 0)
                    veloTarget = new Vector2(horzSign * maxRunSpeed, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -runSpeed, runSpeed);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                if (Mathf.Sign(horizontal) != Mathf.Sign(rb2D.velocity.x) && Mathf.Abs(horizontal) > 0.2)
                {
                    rb2D.AddForce(new Vector2(-rb2D.velocity.x * 2, 0), ForceMode2D.Impulse);
                    CancelInitialRun();
                    initialRun = StartCoroutine(InitalRun());
                }
                break;

            case States.Run:
                veloTarget = new Vector2(horzSign * maxRunSpeed, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -runSpeed, runSpeed);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                break;
                
            case States.Turnaround:
                stateLock = true;
                veloTarget = new Vector2(0, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -turnDampening, turnDampening);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);

                if (Mathf.Approximately(rb2D.velocity.x, 0))
                    stateLock = false;
                break;

            case States.RunStop:
                veloTarget = new Vector2(0, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -runDampening, runDampening);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                if (Mathf.Approximately(rb2D.velocity.x, 0))
                    state = States.Still;
                break;

            case States.Midair:
                float yTarget = 0;
                float xTarget = horizontal * maxAirSpeed;
             
                if (fastFalling)
                {
                    xTarget = horizontal * fastFallXSpeed;
                    yTarget = Mathf.Clamp(-rb2D.velocity.y - maxFastFallSpeed, -fastFallSpeed, fastFallSpeed);
                } else if (cancelJump)
                {
                    if (rb2D.velocity.y > 0)
                    {
                        yTarget = Mathf.Clamp(-rb2D.velocity.y, -cancelJumpSpeed, cancelJumpSpeed);
                    }
                    else
                        cancelJump = false;
                } //else
                    //yTarget = Mathf.Clamp(-rb2D.velocity.y - maxFallSpeed, -fallSpeed, fallSpeed);
                veloTarget = new Vector2(xTarget, yTarget);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                if ((horizontal < 0 && lookRight && rb2D.velocity.x > 0) || (horizontal > 0 && !lookRight && rb2D.velocity.x < 0))
                {
                    veloChange.x = Mathf.Clamp(veloChange.x, -negativeAirSpeed, negativeAirSpeed);
                }
                else
                    veloChange.x = Mathf.Clamp(veloChange.x, -airSpeed, airSpeed);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                break;

            case States.Jumping:
                if (runJump)
                {
                    veloTarget = new Vector2(horzSign * maxAirSpeed, maxJumpSpeed);
                    veloChange = veloTarget - new Vector2(rb2D.velocity.x, rb2D.velocity.y);
                }
                else
                {
                    veloTarget = new Vector2(horizontal * maxAirSpeed, maxJumpSpeed);
                    veloChange = veloTarget - new Vector2(rb2D.velocity.x, rb2D.velocity.y);
                    veloChange.x = Mathf.Clamp(veloChange.x, -jumpSpeed, jumpSpeed);
                }
                rb2D.AddForce(veloChange, ForceMode2D.Impulse); //horiz jump?
                fastFalling = false;
                stateLock = false;
                isGrounded = false;
                state = States.Midair;
                break;

            case States.DoubleJumping:
                doubleJump = false;
                veloTarget = new Vector2(horizontal * maxAirSpeed, maxDblJumpSpeed);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, rb2D.velocity.y);
                veloChange.x = Mathf.Clamp(veloChange.x, -dblJumpSpeed, dblJumpSpeed);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse); //horiz jump?
                fastFalling = false;
                stateLock = false;
                isGrounded = false;
                state = States.Midair;
                break;

            case States.Attacking:
                veloTarget = new Vector2(0, 0);
                veloChange = veloTarget - new Vector2(rb2D.velocity.x, 0);
                veloChange.x = Mathf.Clamp(veloChange.x, -attackDampening, attackDampening);
                rb2D.AddForce(veloChange, ForceMode2D.Impulse);
                break;
        }

        animator.SetBool("Walking", state == States.Walk);
        //CheckLookDirection();

        //Debug Stuff
        playerVelo = rb2D.velocity;
        if (Mathf.Abs(maxVeloChange) < Mathf.Abs(veloChange.x))
            maxVeloChange = veloChange.x;
        if (maxHorz < horizontal)
            maxHorz = horizontal;

    }

    //REMEMBER THAT THIS SETS STATELOCK TO FALSE!!!
    void CancelInitialRun()
    {
        StopCoroutine(initialRun);
        stateLock = false;
        isDashing = false;
        pushbox.EnablePush();
    }

    IEnumerator InitalRun()
    {
        stateLock = true;
        isDashing = true;
        yield return new WaitForSeconds(initialRunTime);
        stateLock = false;
        isDashing = false;
        if (Mathf.Abs(horizontal) > 0)
            state = States.Run;
        else
        {
            pushbox.EnablePush();
            state = States.Stopping;
        }
    }

    void CancelInitialWalk()
    {
        StopCoroutine(initialWalk);
        isWalking = false;
    }

    IEnumerator InitialWalk()
    {
        walkStart = true;
        isWalking = true;
        yield return new WaitForSeconds(initialWalkTime);
        isWalking = false;
    }

    //Disables unused warning
    #pragma warning disable IDE0051
    void AttackFinish()
    {
        stateLock = false;
        animator.SetTrigger("AttackFinished");
    }

    void CancelReady()
    {
        actionable = true;
    }
#pragma warning restore IDE0051

    protected override IEnumerator Knockdown()
    {
        knockdownStart = true;
        isKnockdown = true;
        damageable = false;
        yield return new WaitForSeconds(knockdownTime);
        damageable = true;
        isKnockdown = false;
        stateLock = false;
        actionable = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            isGrounded = true;
            doubleJump = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            isGrounded = false;
        }
    }
}

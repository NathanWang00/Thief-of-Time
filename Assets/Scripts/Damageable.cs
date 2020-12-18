using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    protected float health, fallSpeed = 0f, maxFallSpeed = 0f;
    protected bool damageable = true;

    protected Rigidbody2D rb2D;


    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        rb2D.AddForce(new Vector2(0, Mathf.Clamp(-rb2D.velocity.y - maxFallSpeed, -fallSpeed, fallSpeed)), ForceMode2D.Impulse);
    }

    public virtual void Hurt(float damage, Vector2 hitForce)
    {
        if (damageable)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }
    }

    protected virtual void Die()
    {
        Debug.Log(this.gameObject.name + " is dead");
    }
}

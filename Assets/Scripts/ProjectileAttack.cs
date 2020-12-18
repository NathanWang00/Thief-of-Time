using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [SerializeField]
    private float damage = 0f, knockback = 0f, kbAngle = 0f;

    protected Rigidbody2D rb2D;
    protected Collider2D col2D;
    protected virtual void Awake()
    {
        rb2D = GetComponentInParent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable d = collision.transform.root.GetComponent<Damageable>();
        if (d != null)
        {
            //col2D.isTrigger = true;
            Vector2 hitForce = new Vector2(knockback, 0);
            hitForce = (Vector2)(Quaternion.Euler(0, 0, kbAngle) * hitForce);

            if (rb2D.velocity.x<0)
            {
                hitForce = Vector2.Reflect(hitForce, Vector2.right);
            }
            GameManager.Instance.DamageGeneral(d, damage, hitForce);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public AttacksDictionary attackDictionary;
    private Attack currentAttack = null;

    public void ChangeAttack(string str)
    {
        currentAttack = attackDictionary[str];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable d = collision.transform.root.GetComponent<Damageable>();
        if (d != null)
        {
            Vector2 hitForce = new Vector2(currentAttack.knockback, 0);
            hitForce = (Vector2)(Quaternion.Euler(0, 0, currentAttack.angle) * hitForce);

            if (!GameManager.Instance.GetLookDirection(this.transform.parent.gameObject))
            {
                hitForce = Vector2.Reflect(hitForce, Vector2.right);
            }
            GameManager.Instance.DamageGeneral(d, currentAttack.damage, hitForce);
        }
    }
}

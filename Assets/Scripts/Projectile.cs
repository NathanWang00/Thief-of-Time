using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Damageable
{
    [SerializeField]
    private float speed = 0, depth = 0.5f, initialFade = 0.8f, fadeSpeed = 1f;
    [SerializeField]
    private bool reflectable = false, dead = false;
    protected TerrainDetection tDetection;
    protected GameObject hitboxObject;
    protected Collider2D hitCollider;
    protected Collision2D lastCol;
    protected SpriteRenderer spriteRenderer;

    [SerializeField]
    private Vector2 debugVelo;

    protected override void Awake()
    {
        base.Awake();
        tDetection = GetComponentInChildren<TerrainDetection>();
        hitboxObject = transform.GetChild(0).gameObject;
        hitCollider = hitboxObject.GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        var dir = GameManager.Instance.GetPlayerTransform().position + new Vector3 (0.0f, 0.5f) - transform.position + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //Debug.Log(transform.rotation.eulerAngles);

        rb2D.AddForce( transform.up * speed, ForceMode2D.Impulse);
        
    }

    protected virtual void Update()
    {
        if (tDetection.GetFirstTouch() && !dead)
        {
            lastCol = tDetection.GetCollision();
            transform.Translate(depth * Vector2.up);
            Die();
            // Make the arrow a child of the thing it's stuck to
            transform.parent = lastCol.transform;
            rb2D.isKinematic = true;
            rb2D.velocity = Vector2.zero;
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        if (dead)
        {
            if (spriteRenderer.color.a <= 0)
                Destroy(gameObject);
            else
                spriteRenderer.color = new Color (1, 1, 1, spriteRenderer.color.a - fadeSpeed * Time.deltaTime);
        }

    }

    protected override void FixedUpdate()
    {
        if (!tDetection.GetFirstTouch())
        {
            base.FixedUpdate();
            if (rb2D.velocity.x != 0 && rb2D.velocity.y != 0 && !dead)
            {
                float angle = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg - 90;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            debugVelo = rb2D.velocity;
        }
    }

    public override void Hurt(float damage, Vector2 hitForce)
    {
        if (damageable)
        {
            if (reflectable)
            {
                rb2D.velocity = Vector2.zero;
                rb2D.AddForce(hitForce, ForceMode2D.Impulse);
                hitboxObject.layer = 12;
            }
            else
            {
                Die();
            }
        }
    }

    protected override void Die()
    {
        //Destroy(gameObject);
        dead = true;
        spriteRenderer.color = new Color(1, 1, 1, initialFade);
        //hitCollider.enabled = false;
        //rb2D.freezeRotation = false;
    }
}

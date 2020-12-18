using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDetection : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D /*Head = null,*/ Body = null;
    [SerializeField]
    private bool showGizmo = true;
    private bool isGrounded, isWallTouch, m_Started = false, firstTouch = false;
    private readonly float overlapBoxSize = 0.02f;

    Collision2D lastCol;
    void Awake()
    {
        m_Started = true;
    }

    private void Update()
    {
        CheckGrounded();
        CheckWall();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(new Vector2(Body.bounds.center.x, Body.bounds.center.y - Body.bounds.size.y / 2), 
            new Vector2(Body.bounds.size.x - 0.05f, overlapBoxSize), 0, LayerMask.GetMask("Terrain"));
    }

    private void CheckWall()
    {
        isWallTouch = (Physics2D.OverlapBox(new Vector2(Body.bounds.center.x + Body.bounds.size.x / 2, Body.bounds.center.y),
            new Vector2(overlapBoxSize, Body.bounds.size.y - 0.05f), 0, LayerMask.GetMask("Terrain")) && 
            Physics2D.OverlapBox(new Vector2(Body.bounds.center.x - Body.bounds.size.x / 2, Body.bounds.center.y),
            new Vector2(overlapBoxSize, Body.bounds.size.y - 0.05f), 0, LayerMask.GetMask("Terrain")));
    }

    public bool GetGrounded()
    {
        return isGrounded;
    }

    public bool GetWall()
    {
        return isWallTouch;
    }

    public bool GetFirstTouch()
    {
        return firstTouch;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (m_Started && showGizmo)
        {
            Gizmos.DrawWireCube(new Vector2(Body.bounds.center.x, Body.bounds.center.y - Body.bounds.size.y / 2),
            new Vector2(Body.bounds.size.x - 0.05f, overlapBoxSize));
            Gizmos.DrawWireCube(new Vector2(Body.bounds.center.x + Body.bounds.size.x / 2, Body.bounds.center.y),
            new Vector2(overlapBoxSize, Body.bounds.size.y - 0.05f));
            Gizmos.DrawWireCube(new Vector2(Body.bounds.center.x - Body.bounds.size.x / 2, Body.bounds.center.y),
            new Vector2(overlapBoxSize, Body.bounds.size.y - 0.05f));
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col != null)
        {
            lastCol = col;
            firstTouch = true;
        }
    }

    public Collision2D GetCollision()
    {
        return lastCol;
    }
}

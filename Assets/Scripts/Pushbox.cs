using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushbox : MonoBehaviour
{
    float pushDirection = 0; //0 is no push, -1 is enemy to left, 1 is enemy to right
    BoxCollider2D pBox;

    private void Awake()
    {
        pBox = GetComponent<BoxCollider2D>();
    }
    private void OnTriggerStay2D(Collider2D other)
    {

        if (other != null)
        {
            if (other.bounds.center.x >= pBox.bounds.center.x)
            {
                pushDirection = 1;
            }
            else
            {
                pushDirection = -1;
            }

        }
        else
            pushDirection = 0;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        pushDirection = 0;
    }

    public void EnablePush()
    {
        pBox.enabled = true;
    }

    public void DisablePush()
    {
        pushDirection = 0;
        pBox.enabled = false;
    }

    public float GetPush()
    {
        return pushDirection;
    }
}

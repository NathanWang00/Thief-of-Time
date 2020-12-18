using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance //Singleton Stuff
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("Game Manager is Null");
            }
            return _instance;
        }
    }

    PlayerController player;
    GameObject playerObject;
    PlayerMelee playerMelee;
    Transform playerTransform;

    private void Awake()
    {
        _instance = this;
        playerObject = GameObject.Find("Player");
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        playerMelee = GameObject.Find("PlayerAttack").GetComponent<PlayerMelee>();
        playerTransform = player.transform;
        Application.targetFrameRate = -1;
    }

    private void Update()
    {
        //Debug FPS
        if (Input.GetKeyDown("[0]"))
        {
            Application.targetFrameRate = -1;
        }
        if (Input.GetKeyDown("[1]"))
        {
            Application.targetFrameRate = 2;
        }
    }

    public void DamageGeneral(Damageable d, float damage, Vector2 hitForce)
    {
        d.Hurt(damage, hitForce);
    }

    public void ChangeAttack(string str)
    {
        playerMelee.ChangeAttack(str);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public bool GetLookDirection(GameObject parent)
    {
        Character c = parent.GetComponent<Character>();
        return (c.GetLookDirection());
    }
    public Vector2 GetPlayerLocation()
    {
        return (Vector2) playerObject.transform.position;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}

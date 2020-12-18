using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Attack
{
    public float damage;
    public float knockback;
    public float angle;
}

[Serializable]
public class AttacksDictionary : SerializableDictionary<String, Attack> { }
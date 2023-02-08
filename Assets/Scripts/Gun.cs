using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public float fireRate = 0.2f;
    public float clipSize = 30f;
    public float reservedAmmoCapacity = 270f;

    bool canShoot;
    float ammoInClip;
    float ammoInReserve;

    public abstract override void Use();

    public GameObject bulletImpactPrefab;
}

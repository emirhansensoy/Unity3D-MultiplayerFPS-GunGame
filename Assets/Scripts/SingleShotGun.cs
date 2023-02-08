using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class SingleShotGun : Gun
{
    [SerializeField] Transform playerTransform;
    [SerializeField] GameObject cameraHolder;
    [SerializeField] Camera cam;
    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public Transform raycastOrigin;
    public Transform raycastTarget;
    public TrailRenderer bulletTracer;
    float lastFired = 0;

    Ray ray;
    RaycastHit hit;
    PhotonView pv;

    private void Awake()
    {
        pv = playerTransform.GetComponent<PhotonView>();
    }

    public override void Use()
    {
        Shoot();
    }

    void Shoot()
    {
        if (Time.time - lastFired > fireRate)
         {
            lastFired = Time.time;
            RayCast();
            muzzleFlash.Emit(1);
         }
    }

    void RayCast()
    {
        // creating a raycast and checking did we hit and is the other object idamageable.
        // if yes then object takes damage
        ray.origin = raycastOrigin.position;
        ray.direction = raycastTarget.position - raycastOrigin.position;

        var tracer = Instantiate(bulletTracer, ray.origin, Quaternion.identity);
        tracer.AddPosition(ray.origin);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red, 2);
            hitEffect.transform.position = hit.point;
            hitEffect.transform.forward = hit.normal;
            hitEffect.Emit(1);
            Debug.Log(hit.collider.gameObject.name);
            hit.collider.gameObject.GetComponentInChildren<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
            tracer.transform.position = hit.point;
        }
    }

}

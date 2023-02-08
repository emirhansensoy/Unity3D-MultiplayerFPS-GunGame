using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageable : MonoBehaviour, IDamageable
{
    public static PlayerDamageable Instance;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;
    PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        //reference to our player manager
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();

    }

    private void Update()
    {
        if(!pv.IsMine) return;

        //Die if you fall down the map
        if (transform.position.y < -10f) 
            Die();
    }

    public void TakeDamage(float damage)
    {
        //after enemy takes damage we call a rpc function to reduce their health
        pv.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        if(!pv.IsMine) return;

        Debug.Log("took damage:" + damage);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
            PlayerManager.Find(info.Sender).GetKill();
        }
    }

    public void Die()
    {
        playerManager.Die();
    }

}

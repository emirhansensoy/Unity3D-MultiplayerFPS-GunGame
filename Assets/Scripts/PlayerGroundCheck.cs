using UnityEngine;

// class for checking if the player is grounded
// basically we are trying to find out if the groundCheck object is colliding with the ground
public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(false);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(true);
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(true);
    }

    void OnCollisionExit(Collision other) 
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(false);
    }

    void OnCollisionStay(Collision other)
    {
        if(other.gameObject == playerController.gameObject)
        {
            return;
        }
        playerController.setGroundedState(true);
    }
}

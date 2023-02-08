using UnityEngine;
using System.Collections;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.Animations.Rigging;

public class PlayerController : MonoBehaviourPunCallbacks
{
    Animator animator;
    int jumpAnimation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerTransform;

    [SerializeField] private float jumpForce;

    [SerializeField] Item[] items;

    [SerializeField] GameObject ui;

    [SerializeField] private float animationPlayerTransition = 0.15f;

    [SerializeField] Camera playerCamera;

    [SerializeField] SkinnedMeshRenderer playersHead;

    LayerMask cullingMask;

    // Aiming
    // getting the camera holder to rotate the camera with mouse
    [SerializeField] GameObject cameraHolder;
    [SerializeField] private float mouseSensitivity, lookUpClamp, lookDownClamp;
    float verticalLookRotation;

    // Moving
    // moving animation variables
    int moveXParameter;
    int moveZParameter;
    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;
    [SerializeField] private float sprintSpeed, walkSpeed, smoothTime;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    bool isRunning = false;

    public Rig aimLayer;
    public Rig adsLayer;

    int itemIndex;
    int previousItemIndex = -1;

    bool grounded = false;

    PhotonView pv;

    void Awake()
    {
        // initialize
        pv = playerTransform.GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        jumpAnimation = Animator.StringToHash("Jump");
        moveXParameter = Animator.StringToHash("MoveX");
        moveZParameter = Animator.StringToHash("MoveZ");

    }

    void Start() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //if the player is me then equip the first weapon else destroy the camera and rigidbody objects to handle the audio listener problem
        if(pv.IsMine)
        {
            playersHead.enabled = false;
            EquipItem(0);
        }
        else
        {
            playerCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            playerCamera.gameObject.SetActive(false);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void Update()
    {
        //we just want to update our character
        if(!pv.IsMine) return;

        Look();
        Move();
        Jump();

        // setting animation values for blend tree
        animator.SetFloat(moveXParameter, currentAnimationBlendVector.x);
        animator.SetFloat(moveZParameter, currentAnimationBlendVector.y);

        //pause menu
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        }

        //equiping items with number keys
        for(int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i+1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        //equiping items with mouse scroll wheel
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if(itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
        }


        //using the equipped item. shoot, melee etc.
        if (Input.GetMouseButton(0) && !isRunning)
        {
            if (!grounded)
            {
                items[itemIndex].Use();
            }
            else if(grounded && !Input.GetKey(KeyCode.LeftShift))
            {
                items[itemIndex].Use();
            }
        }

        //if the player is running then we play the running ik rig
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            aimLayer.weight -= Time.deltaTime / items[itemIndex].aimDuration;
            isRunning = true;
        }
        else
        {
            aimLayer.weight += Time.deltaTime / items[itemIndex].aimDuration;
            isRunning = false;
        }
        
        //if the player presses right mouse button we play the ads ik rig
        if (Input.GetMouseButton(1) && !isRunning)
        {
            if (!grounded)
                adsLayer.weight += Time.deltaTime / items[itemIndex].aimDuration;
            else if(grounded && !Input.GetKey(KeyCode.LeftShift))
            {
                adsLayer.weight += Time.deltaTime / items[itemIndex].aimDuration;
            }
        }
        else
        {
            adsLayer.weight -= Time.deltaTime / items[itemIndex].aimDuration;
        }

    }
    void Look()
    {
        //mouse movement on the x axis
        playerTransform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        //mouse movement on the y axis. clamped down the degrees because of body restrictions
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, lookDownClamp, lookUpClamp);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
    void Move()
    {
        // animation blend vector. we will use this variable in the animation float values to smooth the transition between running forwards,backwards etc.
        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, dir * walkSpeed, ref animationVelocity, smoothTime);
        
        // getting input and setting the movement amount to move rigidbody. normalized the vector because we dont want to walk or sprint faster while moving diagonal
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if(moveDir.z > -0.7)
        {
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
        }
        else
        {
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * walkSpeed, ref smoothMoveVelocity, smoothTime);
        }
    }
    void Jump()
    {
        //if the player is grounded and presses space, add force to jump
        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(playerTransform.up * jumpForce);
            animator.CrossFade(jumpAnimation, animationPlayerTransition);
        }
    }

    public void setGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    void FixedUpdate() 
    {
        //we dont want to control other players
        if(!pv.IsMine) return;

        // moving the rigid body
        rb.MovePosition(rb.position + playerTransform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    void EquipItem(int _index)
    {
        //fix for pressing the same weapon slot key for more then once
        if (_index == previousItemIndex) return;

        //setting the selected item active
        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);

        //setting the previous weapon deactive
        if(previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }
        previousItemIndex = itemIndex;

        //setting property to our player to show our weapon to other players
        if (pv.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex",itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //changing the weapon at hand after a property update
        if (changedProps.ContainsKey("itemIndex") && !pv.IsMine && targetPlayer == pv.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
}

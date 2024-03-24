using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb;
    private PhotonView PV;
    public float maxVelocityChange = 10f;

    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool enableSprint = true;
    public bool isSprinting;
    public bool isWalking;
    public bool playerCanMove;
    public bool playerCanShoot;
    public bool isGrounded;
    public float sprintRemaining;
    public bool isSprintCooldown;
    public float sprintSpeed = 35f;
    public float walkSpeed = 30f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;
    }
    private void FixedUpdate()
    {
        Movement();
    }
    void Movement()
    {
        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 targetVelocity = new Vector3(x, 0, z);

            anim.SetFloat("VelocityX", x);
            anim.SetFloat("VelocityY", z);

            if (isSprinting && Input.GetKey(sprintKey))
                targetVelocity *= sprintSpeed;
            else
                targetVelocity *= walkSpeed;

            rb.velocity = targetVelocity;

        }
    }
    void Shoot()
    {
        if(playerCanShoot)
        {
            if(Input.GetKey("Fire1"))
            {

            }
        }
    }
}

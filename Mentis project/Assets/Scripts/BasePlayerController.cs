using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class BasePlayerController : MonoBehaviour
{
    //This was made following a tutorial by 9Gigabyte https://youtu.be/VOHth420ik4
    //Sprinting and Mouse Look taken from Albion's code
    private const float GROUND_CHECK_SPHERE_OFFSET = 0.05f;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform orientation;

    [Space]
    public LayerMask groundMask = 1;
    public float groundCheckDistance = 0.05f;

    [Space]
    public float jumpForce = 5.0f;
    public int maxJumps = 1;
    public float walkForce = 3.0f;
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    private float maxSpeed = 3.0f;
    public float airMovementMultiplier = 0.15f;

    [Space]
    public float mouseSensitivityX = 20;
    public float mouseSensitivityY = 20;
    private float camRotationX;
    private float camRotationY;

    [Space]
    public float friction = 10.0f;
    public float minVelocity = 0.1f;
    public float maxSlopeAngle = 45.0f;
    public float slopeRayLength = 0.5f;

    [Space]
    public float gravityMultiplier = 1.0f;
    public float extraGravity = 2.0f;
    public float extraGravityTimeAfterSlope = 0.3f;

    [Space]
    public bool debug;

    private Rigidbody playerRigidBody;
    private CapsuleCollider playerCollider;

    private int jumpsLeft;
    private bool isGrounded;

    private bool pressedJump;
    private Vector3 yaw;

    private float slopeAngle;
    private float lastTimeOnSlope;

    private Vector3 movementInput;
    private float forward;
    private float sideways;
    private float timeOfLastMove;

    void Start()
    {
        jumpsLeft = maxJumps;
        playerRigidBody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        maxSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        sideways = Input.GetAxisRaw("Horizontal");
        forward = Input.GetAxisRaw("Vertical");
        movementInput.x = sideways;
        movementInput.z = forward;

        if (Input.GetButtonDown("Jump"))
        {
            pressedJump = true;
        }

        if (Input.GetButton("Sprint"))
        {
            maxSpeed = runSpeed;
        }
        else
        {
            maxSpeed = walkSpeed;
        }

        MoveCamera();
    }

    public void FixedUpdate()
    {
        //Physics applied in FixedUpdate to avoid jitter
        RotateBodyToLookingDirection();

        isGrounded = IsGrounded();
        slopeAngle = SlopeAngle();
        if (isGrounded && slopeAngle >= 0 && OnFlatGround())
        {
            lastTimeOnSlope = Time.time;
        }

        if (isGrounded)
        {
            jumpsLeft = maxJumps;
        }

        ExtraGravity();

        if (pressedJump)
        {
            pressedJump = false;

            if (isGrounded || jumpsLeft > 0)
            {
                Jump();
            }
        }
        Movement();
        FrictionForces();
    }

    void MoveCamera()
    {
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");

        float rotX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float rotY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        camRotationX -= rotY;
        camRotationX = Mathf.Clamp(camRotationX, -90f, 90f);
        camRotationY -= rotX * -1;


        //rotX = Mathf.Clamp(rotX, -90, 90);

        playerCamera.transform.rotation = Quaternion.Euler(camRotationX, camRotationY, 0f);
    }

    private void RotateBodyToLookingDirection()
    {
        yaw.y = playerCamera.eulerAngles.y;
        transform.eulerAngles = yaw;
    }

    private void Jump()
    {
        Vector3 currentVelocity = playerRigidBody.velocity;
        currentVelocity.y = 0;
        playerRigidBody.velocity = currentVelocity;

        playerRigidBody.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
        jumpsLeft--;
    }

    private void Movement()
    {
        Vector3 currentVelocity = playerRigidBody.velocity;

        Vector3 finalDir = (transform.forward * forward + transform.right * sideways).normalized;
        if (isGrounded && OnFlatGround())
        {
            Vector3 dir = Vector3.zero;
            dir += orientation.transform.forward * forward;
            dir += orientation.transform.right * sideways;
            finalDir = dir.normalized;

            if (debug)
            {
                Debug.DrawLine(FeetPosition(), FeetPosition() + finalDir * 25f, Color.green);
            }
        }

        if (!isGrounded)
        {
            finalDir *= airMovementMultiplier;
        }

        if (OnFlatGround())
        {
            playerRigidBody.AddForce(walkForce * finalDir);
        }

        if (currentVelocity.magnitude > maxSpeed)
        {
            if (isGrounded && OnFlatGround())
            {
                Vector3 clamped = Vector3.ClampMagnitude(playerRigidBody.velocity, maxSpeed);
                playerRigidBody.velocity = clamped;
            }
            else if (!isGrounded)
            {
                Vector3 horizontalClamped = Vector3.ClampMagnitude(new Vector3(playerRigidBody.velocity.x, 0, playerRigidBody.velocity.z), maxSpeed);
                playerRigidBody.velocity = horizontalClamped + Vector3.up * playerRigidBody.velocity.y;
            }
        }
    }

    private void ExtraGravity()
    {
        playerRigidBody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        if (!pressedJump && !(isGrounded && OnFlatGround()) && Time.time < lastTimeOnSlope + extraGravityTimeAfterSlope)
        {
            playerRigidBody.AddForce(Physics.gravity * extraGravity, ForceMode.Acceleration);
        }
    }

    private void FrictionForces()
    {
        if (movementInput != Vector3.zero) return;
        if (!OnFlatGround() || !isGrounded) return;

        if (playerRigidBody.velocity.magnitude < minVelocity)
        {
            playerRigidBody.velocity = Vector3.zero;
        }
        else
        {
            playerRigidBody.AddForce(-1 * friction * playerRigidBody.velocity.normalized);
        }
    }

    private Vector3 FeetPosition()
    {
        Vector3 sphereOffset = (playerCollider.height * transform.localScale.y / 2 - playerCollider.radius * transform.localScale.x) * -1 * transform.up;
        Vector3 feetPosition = playerRigidBody.position + sphereOffset;
        return feetPosition;
    }

    private float SlopeAngle()
    {
        float distance = slopeRayLength + playerCollider.height * transform.localScale.y * 0.5f;
        bool hit = Physics.Raycast(playerRigidBody.position, Vector3.down, out RaycastHit info, distance);
        if (hit)
        {
            return Vector3.Angle(Vector3.up, info.normal);
        }
        return -1;
    }

    private bool OnFlatGround()
    {
        return SlopeAngle() <= maxSlopeAngle;
    }

    public bool IsGrounded()
    {
        Vector3 upOffset = transform.up * GROUND_CHECK_SPHERE_OFFSET;
        bool isGrounded = Physics.SphereCast(FeetPosition() + upOffset,
            playerCollider.radius * transform.localScale.x, -1 * transform.up, out RaycastHit info,
            groundCheckDistance + GROUND_CHECK_SPHERE_OFFSET, groundMask);
        return isGrounded;
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Vector3 feetOffset = -1 * groundCheckDistance * transform.up;
            Gizmos.DrawWireSphere(FeetPosition() + feetOffset, playerCollider.radius * transform.localScale.x);
        }
    }
}

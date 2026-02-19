using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;

    [Header("Movement")]
    public float moveSpeed = 4500f;
    public float maxSpeed = 14f;
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;

    [Header("Jumping")]
    public float jumpForce = 550f;
    public float jumpCooldown = 0.1f;
    private bool readyToJump = true;

    [Header("Ground")]
    public bool grounded;
    public LayerMask whatIsGround;
    public float maxSlopeAngle = 35f;

    [Header("Air Control")]
    [Range(0.1f, 1f)]
    public float airMultiplier = 0.7f;

    // Input
    private Vector2 moveInput;
    private bool jumping;

    // Private
    private Rigidbody rb;
    private Vector3 normalVector = Vector3.up;
    private bool cancellingGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    #region INPUT EVENTS
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) jumping = true;
        if (context.canceled) jumping = false;
    }
    #endregion

    #region MOVEMENT
    private void Movement()
    {
        float x = moveInput.x;
        float y = moveInput.y;

        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        CounterMovement(x, y, mag);

        if (readyToJump && jumping) Jump();

        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        float multiplier = 1f, multiplierV = 1f;

        if (!grounded)
        {
            multiplier = airMultiplier;
            multiplierV = airMultiplier;
        }

        rb.AddForce(orientation.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.right * x * moveSpeed * Time.deltaTime * multiplier);
    }
    #endregion
    
    #region JUMP
    private void Jump()
    {
        if (!grounded || !readyToJump) return;

        readyToJump = false;

        rb.AddForce(Vector2.up * jumpForce * 1.5f);
        rb.AddForce(normalVector * jumpForce * 0.5f);

        Vector3 vel = rb.linearVelocity;
        if (vel.y < 0.5f)
            rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
        else if (vel.y > 0)
            rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    #endregion

    #region COUNTER MOVEMENT
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f ||
            (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f ||
            (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        float horizontalSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        if (horizontalSpeed > maxSpeed)
        {
            float fallspeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(n.x, fallspeed, n.z);
        }
    }
    #endregion

    #region HELPERS
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = rb.linearVelocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    // Esto lo necesita PlayerCamera para saber 
    // si el jugador camina (head bob)
    public bool IsMoving()
    {
        return grounded && (moveInput.x != 0 || moveInput.y != 0);
    }
    #endregion

    #region GROUND DETECTION
    private bool IsFloor(Vector3 v)
    {
        return Vector3.Angle(Vector3.up, v) < maxSlopeAngle;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * 3f);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }
    #endregion
}
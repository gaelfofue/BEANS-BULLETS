using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4500f;
    [SerializeField] private float walkSpeed = 14f;
    [SerializeField] private float counterMovement = 0.175f;
    private float threshold = 0.01f;

    [Header("Air")]
    [SerializeField] private float airMultiplier = 0.4f;
    [SerializeField] private float airAcceleration = 12f;
    [SerializeField] private float airStrafeAccel = 70f;
    [SerializeField] private float airStrafeMaxSpeed = 1f;
    [SerializeField] private float airControl = 0.3f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private bool autoBunnyHop = true;
    [SerializeField] private float extraGravity = 15f;

    [Header("Ground Detection")]
    [SerializeField] private float maxSlopeAngle = 35f;
    public LayerMask whatIsGround;

    [Header("References")]
    public Transform orientation;

    // Público para otros scripts
    public float maxSpeed => walkSpeed;
    public float Speed => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

    // Modificadores externos (mutaciones)
    [HideInInspector] public float speedMultiplier = 1f;

    // Componentes
    private Rigidbody rb;

    // Ground
    public bool grounded;
    private Vector3 normalVector = Vector3.up;
    private bool cancellingGrounded;

    // Input
    private Vector2 moveInput;
    private bool jumpHeld = false;
    private bool jumpPressed = false;
    private bool readyToJump = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        readyToJump = true;
    }

    #region INPUT EVENTS

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpHeld = true;
            jumpPressed = true;
        }

        if (context.canceled)
        {
            jumpHeld = false;
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (autoBunnyHop)
        {
            if (jumpHeld && readyToJump && grounded)
                Jump();
        }
        else
        {
            if (jumpPressed && readyToJump && grounded)
            {
                Jump();
                jumpPressed = false;
            }
        }

        Movement();
    }

    #region MOVEMENT

    private void Movement()
    {
        rb.AddForce(Vector3.down * Time.deltaTime * extraGravity);

        float x = moveInput.x;
        float y = moveInput.y;

        Vector2 mag = FindVelRelativeToLook();

        float currentMaxSpeed = walkSpeed * speedMultiplier;

        if (grounded)
        {
            CounterMovement(x, y, mag);

            if (x > 0 && mag.x > currentMaxSpeed) x = 0;
            if (x < 0 && mag.x < -currentMaxSpeed) x = 0;
            if (y > 0 && mag.y > currentMaxSpeed) y = 0;
            if (y < 0 && mag.y < -currentMaxSpeed) y = 0;

            rb.AddForce(orientation.forward * y * moveSpeed * Time.deltaTime * speedMultiplier);
            rb.AddForce(orientation.right * x * moveSpeed * Time.deltaTime * speedMultiplier);
        }
        else
        {
            AirMove(x, y);
        }
    }

    #endregion

    #region AIR MOVEMENT

    private void AirMove(float x, float y)
    {
        Vector3 wishdir = (orientation.forward * y + orientation.right * x);

        if (Mathf.Abs(y) < 0.01f && Mathf.Abs(x) > 0.01f)
        {
            wishdir.Normalize();
            float strafeSpeed = airStrafeMaxSpeed * speedMultiplier;
            Accelerate(wishdir, strafeSpeed, airStrafeAccel);
        }
        else
        {
            wishdir.Normalize();
            float wishspeed = walkSpeed * speedMultiplier * airMultiplier;
            Accelerate(wishdir, wishspeed, airAcceleration);
        }

        if (airControl > 0 && Mathf.Abs(y) > 0.01f)
        {
            AirControl(wishdir, walkSpeed * speedMultiplier);
        }
    }

    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        Vector3 currentVel = rb.linearVelocity;
        currentVel.y = 0;

        float currentSpeed = Vector3.Dot(currentVel, targetDir);
        float addSpeed = targetSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.deltaTime * targetSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        rb.AddForce(targetDir * accelSpeed, ForceMode.VelocityChange);
    }

    private void AirControl(Vector3 targetDir, float targetSpeed)
    {
        Vector3 vel = rb.linearVelocity;
        float ySpeed = vel.y;
        vel.y = 0;

        float speed = vel.magnitude;
        if (speed < 0.1f) return;

        vel.Normalize();

        float dot = Vector3.Dot(vel, targetDir);
        float k = 32f * airControl * dot * dot * Time.deltaTime;

        if (dot > 0)
        {
            vel.x = vel.x * speed + targetDir.x * k;
            vel.z = vel.z * speed + targetDir.z * k;

            vel.Normalize();
            vel *= speed;
            vel.y = ySpeed;

            rb.linearVelocity = vel;
        }
    }

    #endregion

    #region COUNTER MOVEMENT

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded) return;

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
    }

    #endregion

    #region JUMP

    private void Jump()
    {
        if (!grounded || !readyToJump) return;

        readyToJump = false;

        Vector3 vel = rb.linearVelocity;
        if (vel.y < 0.5f)
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
        else if (vel.y > 0)
            rb.linearVelocity = new Vector3(vel.x, vel.y / 2f, vel.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    #endregion

    #region HELPERS

    public bool IsMoving()
    {
        return grounded &&
               (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f);
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.linearVelocity.x, rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
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

        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    #endregion
}
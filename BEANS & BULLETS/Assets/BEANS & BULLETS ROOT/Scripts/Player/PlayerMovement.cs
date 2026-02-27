using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement - Ground")]
    [SerializeField] private float groundMaxSpeed = 7f;
    [SerializeField] private float groundAcceleration = 14f;
    [SerializeField] private float groundDeceleration = 10f;
    [SerializeField] private float friction = 6f;

    [Header("Movement - Air")]
    [SerializeField] private float airMaxSpeed = 7f;
    [SerializeField] private float airAcceleration = 2f;
    [SerializeField] private float airDeceleration = 2f;

    [Header("Movement - Strafe (Air)")]
    [SerializeField] private float strafeMaxSpeed = 1f;
    [SerializeField] private float strafeAcceleration = 50f;

    [Header("Air Control")]
    [SerializeField] private float airControl = 0.3f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private bool autoBunnyHop = false;

    [Header("Physics")]
    [SerializeField] private float gravity = 20f;

    [Header("References")]
    public Transform orientation;

    // Público para GunBob y GunTilt
    public float maxSpeed => groundMaxSpeed;
    public float Speed => new Vector3(playerVelocity.x, 0f, playerVelocity.z).magnitude;

    // Privado
    private CharacterController controller;
    private Vector3 playerVelocity = Vector3.zero;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private bool jumpQueued = false;

    // Input
    private Vector2 moveInput;
    private bool jumpHeld = false;
    private bool jumpPressed = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
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

    private void Update()
    {
        QueueJump();

        if (controller.isGrounded)
        {
            GroundMove();
        }
        else
        {
            AirMove();
        }

        controller.Move(playerVelocity * Time.deltaTime);
    }

    #region JUMP

    private void QueueJump()
    {
        if (autoBunnyHop)
        {
            jumpQueued = jumpHeld;
            return;
        }

        if (jumpPressed)
        {
            jumpQueued = true;
        }

        jumpPressed = false;
    }

    #endregion

    #region GROUND MOVEMENT

    private void GroundMove()
    {
        if (!jumpQueued)
        {
            ApplyFriction(1.0f);
        }
        else
        {
            ApplyFriction(0f);
        }

        Vector3 wishdir = new Vector3(moveInput.x, 0f, moveInput.y);
        wishdir = orientation.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed = wishdir.magnitude * groundMaxSpeed;

        Accelerate(wishdir, wishspeed, groundAcceleration);

        // Gravedad mínima para mantener isGrounded
        playerVelocity.y = -gravity * Time.deltaTime;

        if (jumpQueued)
        {
            // Resetear velocidad vertical antes de saltar
            playerVelocity.y = jumpForce;
            jumpQueued = false;
        }
    }

    #endregion

    #region AIR MOVEMENT

    private void AirMove()
    {
        float accel;

        Vector3 wishdir = new Vector3(moveInput.x, 0f, moveInput.y);
        wishdir = orientation.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= airMaxSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
        {
            accel = airDeceleration;
        }
        else
        {
            accel = airAcceleration;
        }

        if (moveInput.y == 0 && moveInput.x != 0)
        {
            if (wishspeed > strafeMaxSpeed)
            {
                wishspeed = strafeMaxSpeed;
            }
            accel = strafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);

        if (airControl > 0)
        {
            AirControl(wishdir, wishspeed2);
        }

        playerVelocity.y -= gravity * Time.deltaTime;
    }

    private void AirControl(Vector3 targetDir, float targetSpeed)
    {
        if (Mathf.Abs(moveInput.y) < 0.001f || Mathf.Abs(targetSpeed) < 0.001f)
        {
            return;
        }

        float zSpeed = playerVelocity.y;
        playerVelocity.y = 0;

        float speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        float dot = Vector3.Dot(playerVelocity, targetDir);
        float k = 32f;
        k *= airControl * dot * dot * Time.deltaTime;

        if (dot > 0)
        {
            playerVelocity.x *= speed + targetDir.x * k;
            playerVelocity.y *= speed + targetDir.y * k;
            playerVelocity.z *= speed + targetDir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zSpeed;
        playerVelocity.z *= speed;
    }

    #endregion

    #region PHYSICS

    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(playerVelocity, targetDir);
        float addSpeed = targetSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.deltaTime * targetSpeed;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed;
        }

        playerVelocity.x += accelSpeed * targetDir.x;
        playerVelocity.z += accelSpeed * targetDir.z;
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        vec.y = 0f;
        float speed = vec.magnitude;
        float drop = 0f;

        if (controller.isGrounded)
        {
            float control = speed < groundDeceleration ? groundDeceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        float newSpeed = speed - drop;
        if (newSpeed < 0) newSpeed = 0;
        if (speed > 0) newSpeed /= speed;

        playerVelocity.x *= newSpeed;
        playerVelocity.z *= newSpeed;
    }

    #endregion

    #region HELPERS

    public bool IsMoving()
    {
        return controller.isGrounded &&
               (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f);
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.eulerAngles.y;
        float moveAngle = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = new Vector2(playerVelocity.x, playerVelocity.z).magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    #endregion
}
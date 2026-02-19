// GunTilt.cs en el GunHolder
using UnityEngine;

public class GunTilt : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Tilt Settings")]
    public float tiltAmount = 2f;
    public float tiltSmooth = 6f;

    private Quaternion originRot;
    private float currentTilt = 0f;

    void Start()
    {
        originRot = transform.localRotation;
    }

    void Update()
    {
        if (playerMovement == null) return;

        float targetTilt = 0f;

        if (playerMovement.IsMoving())
        {
            Vector2 vel = playerMovement.FindVelRelativeToLook();
            targetTilt = -vel.x / playerMovement.maxSpeed * tiltAmount;
        }

        currentTilt = Mathf.Lerp(
            currentTilt,
            targetTilt,
            Time.deltaTime * tiltSmooth
        );

        transform.localRotation = originRot * Quaternion.Euler(0, 0, currentTilt);
    }
}
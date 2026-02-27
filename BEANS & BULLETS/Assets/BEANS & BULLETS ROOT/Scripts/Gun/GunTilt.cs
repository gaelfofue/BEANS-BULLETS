using UnityEngine;

public class GunTilt : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Tilt Settings")]
    public float tiltAmount = 2f;
    public float tiltSmooth = 6f;

    private float currentTilt = 0f;

    // YA NO se guarda originRot porque este objeto
    // empieza en rotación identity y solo hace tilt
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

        // Escribe en SU propio transform (GunTiltPivot)
        // No pisa a GunSway que está en el padre
        transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}
using UnityEngine;

public class GunBob : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Walk Bob - Half Life Style")]
    public float bobFrequency = 6f;
    public float bobAmountZ = 0.015f;   // Adelante-atrás (péndulo)
    public float bobAmountX = 0.005f;   // Lateral sutil

    [Header("Side Tilt")]
    public float tiltAmount = 2f;
    public float tiltSmooth = 6f;

    [Header("Smooth")]
    public float bobSmooth = 6f;

    // Private
    private float bobTimer = 0f;
    private Vector3 originPos;
    private Quaternion originRot;
    private float currentTilt = 0f;

    void Start()
    {
        originPos = transform.localPosition;
        originRot = transform.localRotation;
    }

    void Update()
    {
        if (playerMovement == null) return;

        WalkBob();
        SideTilt();
    }

    private void WalkBob()
    {
        Vector3 targetPos = originPos;

        if (playerMovement.IsMoving())
        {
            bobTimer += Time.deltaTime * bobFrequency;

            // Péndulo adelante-atrás en Z
            float bobZ = Mathf.Sin(bobTimer) * bobAmountZ;
            // Lateral sutil en X (mitad de frecuencia)
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;

            targetPos = originPos + new Vector3(bobX, 0, bobZ);
        }
        else
        {
            bobTimer = 0f;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * bobSmooth
        );
    }

    private void SideTilt()
    {
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

        // Tilt sobre rotación original
        transform.localRotation = originRot * Quaternion.Euler(0, 0, currentTilt);
    }
}
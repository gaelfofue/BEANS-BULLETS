using UnityEngine;

public class GunBob : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Bob Settings")]
    public float bobAmount = 0.1f;
    public float bobSpeed = 8f;
    public float bobSmooth = 6f;

    private float bobCycle = 0f;
    private Vector3 originPos;

    void Start()
    {
        originPos = transform.localPosition;
    }

    void LateUpdate()
    {
        if (playerMovement == null) return;

        HalfLifeBob();
    }

    private void HalfLifeBob()
    {
        Vector3 targetPos = originPos;

        if (playerMovement.IsMoving())
        {
            // Usar Speed del CharacterController en vez de Rigidbody
            float normalizedSpeed = Mathf.Clamp01(
                playerMovement.Speed / playerMovement.maxSpeed
            );

            bobCycle += Time.deltaTime * bobSpeed;

            float bob = normalizedSpeed * bobAmount * Mathf.Sin(bobCycle);

            targetPos = originPos + new Vector3(0, 0, bob);
        }
        else
        {
            bobCycle = 0f;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * bobSmooth
        );
    }
}
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
    private Rigidbody playerRb;

    void Start()
    {
        originPos = transform.localPosition;

        if (playerMovement != null)
            playerRb = playerMovement.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (playerMovement == null || playerRb == null) return;

        HalfLifeBob();
    }

    private void HalfLifeBob()
    {
        Vector3 targetPos = originPos;

        if (playerMovement.IsMoving())
        {
            float speed = new Vector2(
                playerRb.linearVelocity.x,
                playerRb.linearVelocity.z
            ).magnitude;

            float normalizedSpeed = Mathf.Clamp01(speed / playerMovement.maxSpeed);

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
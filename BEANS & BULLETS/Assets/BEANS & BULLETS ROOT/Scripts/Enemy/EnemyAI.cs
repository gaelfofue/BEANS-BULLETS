using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        LungeWindup,
        LungeDash,
        LungeRecover
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float lungeRange = 3.5f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Lunge")]
    [SerializeField] private float windupDuration = 0.25f;
    [SerializeField] private float lungeDuration = 0.18f;
    [SerializeField] private float lungeForce = 15f;
    [SerializeField] private float recoverDuration = 0.5f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 2.5f;
    [SerializeField] private float lungeCooldown = 1.2f;

    [Header("Visual Feedback")]
    [SerializeField] private float windupLeanBack = 15f;

    [Header("CharacterController Setup")]
    [SerializeField] private float ccHeight = 1.8f;
    [SerializeField] private float ccRadius = 0.4f;
    [SerializeField] private Vector3 ccCenter = new Vector3(0f, 0.9f, 0f);

    private State currentState = State.Idle;
    private float stateTimer = 0f;
    private float cooldownTimer = 0f;
    private bool hasHitThisLunge = false;

    private Vector3 lungeDirection;
    private Transform player;
    private CharacterController cc;

    private Transform modelTransform;
    private Quaternion modelOriginalRot;

    private float gravity = -20f;
    private float verticalVelocity = 0f;

    private float logTimer = 0f;

    void Start()
    {
        // Remove NavMeshAgent if present
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            Destroy(agent);
        }

        // Remove Rigidbody if present
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);

        // Setup CharacterController
        cc = GetComponent<CharacterController>();
        if (cc == null)
            cc = gameObject.AddComponent<CharacterController>();

        cc.height = ccHeight;
        cc.radius = ccRadius;
        cc.center = ccCenter;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        cc.skinWidth = 0.08f;

        if (transform.childCount > 0)
        {
            modelTransform = transform.GetChild(0);
            modelOriginalRot = modelTransform.localRotation;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Snap to ground immediately
        SnapToGround();

        Debug.Log($"[ENEMY] START | pos:{transform.position} | playerPos:{(player != null ? player.position.ToString() : "null")}");
    }

    private void SnapToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 5f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
        {
            Vector3 groundPos = hit.point;
            transform.position = groundPos;
            Debug.Log($"[ENEMY] Snapped to ground Y:{groundPos.y:F2}");
        }
    }

    void Update()
    {
        if (player == null) return;

        cooldownTimer -= Time.deltaTime;
        logTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.LungeWindup:
                UpdateWindup();
                break;
            case State.LungeDash:
                UpdateLungeDash();
                break;
            case State.LungeRecover:
                UpdateRecover();
                break;
        }
    }

    private float GetHorizontalDistance()
    {
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void ApplyGravity()
    {
        if (cc == null) return;

        if (cc.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    private void MoveTowardsPlayer()
    {
        if (cc == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.25f) return; // Stop if very close

        dir.Normalize();

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );

        // Move with gravity
        ApplyGravity();
        Vector3 move = (dir * chaseSpeed) + (Vector3.up * verticalVelocity);
        cc.Move(move * Time.deltaTime);
    }

    private void UpdateIdle()
    {
        ApplyGravity();
        if (cc != null)
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        if (GetHorizontalDistance() <= detectionRange)
            ChangeState(State.Chase);
    }

    private void UpdateChase()
    {
        float dist = GetHorizontalDistance();

        MoveTowardsPlayer();

        if (logTimer <= 0f)
        {
            Debug.Log($"[ENEMY] CHASE | dist:{dist:F1} | grounded:{cc.isGrounded} | pos:{transform.position}");
            logTimer = 1f;
        }

        if (dist <= lungeRange && cooldownTimer <= 0f)
            ChangeState(State.LungeWindup);
    }

    private void UpdateWindup()
    {
        stateTimer -= Time.deltaTime;

        ApplyGravity();
        if (cc != null)
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Face player
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Visual telegraph
        if (modelTransform != null)
        {
            float t = 1f - (stateTimer / windupDuration);
            float lean = Mathf.Sin(t * Mathf.PI * 0.5f) * windupLeanBack;
            modelTransform.localRotation = modelOriginalRot * Quaternion.Euler(-lean, 0f, 0f);
        }

        if (stateTimer <= 0f)
        {
            lungeDirection = (player.position - transform.position);
            lungeDirection.y = 0f;
            lungeDirection.Normalize();
            ChangeState(State.LungeDash);
        }
    }

    private void UpdateLungeDash()
    {
        stateTimer -= Time.deltaTime;

        float t = 1f - (stateTimer / lungeDuration);
        float dashCurve = Mathf.Sin(t * Mathf.PI * 0.5f);

        ApplyGravity();

        // Lunge speed: lungeForce only, NO multiplier stacking
        float currentSpeed = lungeForce * dashCurve;
        Vector3 move = (lungeDirection * currentSpeed) + (Vector3.up * verticalVelocity);

        if (cc != null)
            cc.Move(move * Time.deltaTime);

        if (modelTransform != null)
        {
            float lean = Mathf.Sin(t * Mathf.PI) * 25f;
            modelTransform.localRotation = modelOriginalRot * Quaternion.Euler(lean, 0f, 0f);
        }

        if (stateTimer <= 0f)
            ChangeState(State.LungeRecover);
    }

    private void UpdateRecover()
    {
        stateTimer -= Time.deltaTime;

        ApplyGravity();
        if (cc != null)
            cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        if (modelTransform != null)
        {
            modelTransform.localRotation = Quaternion.Lerp(
                modelTransform.localRotation,
                modelOriginalRot,
                Time.deltaTime * 8f
            );
        }

        if (stateTimer <= 0f)
        {
            cooldownTimer = lungeCooldown;
            ChangeState(State.Chase);
        }
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
            case State.Idle:
                stateTimer = 0f;
                break;
            case State.Chase:
                Debug.Log("[ENEMY] → CHASE");
                break;
            case State.LungeWindup:
                stateTimer = windupDuration;
                hasHitThisLunge = false;
                Debug.Log("[ENEMY] → WINDUP");
                break;
            case State.LungeDash:
                stateTimer = lungeDuration;
                Debug.Log("[ENEMY] → LUNGE");
                break;
            case State.LungeRecover:
                stateTimer = recoverDuration;
                Debug.Log("[ENEMY] → RECOVER");
                break;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckPlayerHit(hit.gameObject);
    }

    private void CheckPlayerHit(GameObject obj)
    {
        if (currentState != State.LungeDash) return;
        if (hasHitThisLunge) return;
        if (!obj.CompareTag("Player")) return;

        hasHitThisLunge = true;

        if (GameTimer.Instance != null)
            GameTimer.Instance.RemoveTime(attackDamage);

        Debug.Log($"[ENEMY] LUNGE HIT! -{attackDamage}s");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }
}
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
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Lunge")]
    [SerializeField] private float windupDuration = 0.4f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private float lungeForce = 12f;
    [SerializeField] private float recoverDuration = 0.6f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 2.5f;
    [SerializeField] private float lungeCooldown = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private float windupLeanBack = 15f;

    private State currentState = State.Idle;
    private float stateTimer = 0f;
    private float cooldownTimer = 0f;
    private bool hasHitThisLunge = false;

    private Vector3 lungeDirection;
    private Transform player;

    private Transform modelTransform;
    private Quaternion modelOriginalRot;

    private float logTimer = 0f;

    // Ground-locked movement (no CharacterController, no NavMesh)
    private float groundY = 0f;
    private bool groundFound = false;

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

        // Remove CharacterController if present (we don't use it anymore)
        var cc = GetComponent<CharacterController>();
        if (cc != null)
            Destroy(cc);

        if (transform.childCount > 0)
        {
            modelTransform = transform.GetChild(0);
            modelOriginalRot = modelTransform.localRotation;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Find the ground Y by raycasting down
        FindGround();

        Debug.Log($"[ENEMY] START | pos:{transform.position} | groundY:{groundY} | playerPos:{(player != null ? player.position.ToString() : "null")}");
    }

    private void FindGround()
    {
        // Try multiple ray origins to find the floor
        Vector3[] rayOrigins = new Vector3[]
        {
            transform.position + Vector3.up * 10f,
            transform.position + Vector3.up * 5f,
            transform.position + Vector3.up * 2f,
        };

        foreach (var origin in rayOrigins)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.down, out hit, 30f))
            {
                groundY = hit.point.y;
                groundFound = true;
                Debug.Log($"[ENEMY] Ground found at Y:{groundY:F2} (hit: {hit.collider.name})");
                break;
            }
        }

        if (!groundFound)
        {
            // Fallback: use player Y when available
            if (player != null)
            {
                groundY = player.position.y;
                groundFound = true;
                Debug.Log($"[ENEMY] No ground raycast, using player Y:{groundY:F2}");
            }
        }

        // Snap to ground
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }

    void Update()
    {
        if (player == null) return;

        cooldownTimer -= Time.deltaTime;
        logTimer -= Time.deltaTime;

        // Always keep enemy at ground level
        KeepOnGround();

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

    private void KeepOnGround()
    {
        // Continuously update ground Y based on player
        // (both are on the same floor)
        if (player != null)
        {
            groundY = player.position.y;
        }

        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }

    private float GetHorizontalDistance()
    {
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void UpdateIdle()
    {
        if (GetHorizontalDistance() <= detectionRange)
            ChangeState(State.Chase);
    }

    private void UpdateChase()
    {
        float dist = GetHorizontalDistance();

        // Direction to player (horizontal only)
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.25f)
        {
            dir.Normalize();

            // Smooth rotation
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            // Move horizontally only
            Vector3 move = dir * chaseSpeed * Time.deltaTime;
            move.y = 0f;
            transform.position += move;
        }

        if (logTimer <= 0f)
        {
            Debug.Log($"[ENEMY] CHASE | dist:{dist:F1} | pos:{transform.position} | playerPos:{player.position}");
            logTimer = 1f;
        }

        if (dist <= lungeRange && cooldownTimer <= 0f)
            ChangeState(State.LungeWindup);
    }

    private void UpdateWindup()
    {
        stateTimer -= Time.deltaTime;

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

        // Move forward
        Vector3 move = lungeDirection * lungeForce * dashCurve * Time.deltaTime;
        move.y = 0f;
        transform.position += move;

        // Check for player hit with overlap sphere
        CheckLungeHitSphere();

        if (modelTransform != null)
        {
            float lean = Mathf.Sin(t * Mathf.PI) * 25f;
            modelTransform.localRotation = modelOriginalRot * Quaternion.Euler(lean, 0f, 0f);
        }

        if (stateTimer <= 0f)
            ChangeState(State.LungeRecover);
    }

    private void CheckLungeHitSphere()
    {
        if (hasHitThisLunge) return;

        // Check a sphere around the enemy for the player
        Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hasHitThisLunge = true;

                if (GameTimer.Instance != null)
                    GameTimer.Instance.RemoveTime(attackDamage);

                Debug.Log($"[ENEMY] LUNGE HIT! -{attackDamage}s");
                return;
            }
        }
    }

    private void UpdateRecover()
    {
        stateTimer -= Time.deltaTime;

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }
}
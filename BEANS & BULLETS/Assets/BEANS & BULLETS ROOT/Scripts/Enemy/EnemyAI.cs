using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private float lungeRange = 3f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 10f;

    [Header("Lunge")]
    [SerializeField] private float windupDuration = 0.25f;
    [SerializeField] private float lungeDuration = 0.18f;
    [SerializeField] private float lungeForce = 25f;
    [SerializeField] private float recoverDuration = 0.5f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 2.5f;
    [SerializeField] private float lungeCooldown = 1.2f;

    [Header("Visual Feedback")]
    [SerializeField] private float windupLeanBack = 15f;

    // State
    private State currentState = State.Idle;
    private float stateTimer = 0f;
    private float cooldownTimer = 0f;
    private bool hasHitThisLunge = false;

    // Lunge
    private Vector3 lungeDirection;
    private Vector3 lungeStartPos;

    // References
    private NavMeshAgent agent;
    private Transform player;
    private Rigidbody rb;

    // Visual
    private Transform modelTransform;
    private Quaternion modelOriginalRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0f;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        if (transform.childCount > 0)
        {
            modelTransform = transform.GetChild(0);
            modelOriginalRot = modelTransform.localRotation;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // DEBUG
        Debug.Log($"ENEMY START | player:{player != null} agent:{agent != null} onNavMesh:{agent.isOnNavMesh} state:{currentState}");
    }

    void Update()
    {
        if (player == null) return;

        cooldownTimer -= Time.deltaTime;

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

    #region STATES

    private void UpdateIdle()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRange)
        {
            ChangeState(State.Chase);
        }
    }

    private void UpdateChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        agent.isStopped = false;
        agent.SetDestination(player.position);

        // DEBUG
        Debug.Log($"CHASE | dist:{dist:F1} agentStopped:{agent.isStopped} onNavMesh:{agent.isOnNavMesh} hasPath:{agent.hasPath} velocity:{agent.velocity.magnitude:F1}");

        LookAtPlayer();

        if (dist <= lungeRange && cooldownTimer <= 0f)
        {
            ChangeState(State.LungeWindup);
        }
    }

    private void UpdateWindup()
    {
        stateTimer -= Time.deltaTime;

        // Parar movimiento
        agent.isStopped = true;

        // Mirar al player
        LookAtPlayer();

        // Inclinarse hacia atrás (telegraph visual)
        if (modelTransform != null)
        {
            float t = 1f - (stateTimer / windupDuration);
            float lean = Mathf.Sin(t * Mathf.PI * 0.5f) * windupLeanBack;
            modelTransform.localRotation = modelOriginalRot * Quaternion.Euler(-lean, 0f, 0f);
        }

        if (stateTimer <= 0f)
        {
            // Calcular dirección del lunge AHORA
            lungeDirection = (player.position - transform.position).normalized;
            lungeDirection.y = 0f;
            lungeStartPos = transform.position;

            ChangeState(State.LungeDash);
        }
    }

    private void UpdateLungeDash()
    {
        stateTimer -= Time.deltaTime;

        // Desactivar NavMeshAgent durante el dash
        agent.isStopped = true;
        agent.updatePosition = false;

        // Mover hacia adelante con fuerza
        float t = 1f - (stateTimer / lungeDuration);
        float dashCurve = Mathf.Sin(t * Mathf.PI * 0.5f); // ease-out

        Vector3 movement = lungeDirection * lungeForce * Time.deltaTime * (1f + dashCurve);
        transform.position += movement;

        // Inclinar hacia adelante durante dash
        if (modelTransform != null)
        {
            float lean = Mathf.Sin(t * Mathf.PI) * 25f;
            modelTransform.localRotation = modelOriginalRot * Quaternion.Euler(lean, 0f, 0f);
        }

        if (stateTimer <= 0f)
        {
            ChangeState(State.LungeRecover);
        }
    }

    private void UpdateRecover()
    {
        stateTimer -= Time.deltaTime;

        agent.updatePosition = true;
        agent.isStopped = true;

        // Volver modelo a rotación original
        if (modelTransform != null)
        {
            modelTransform.localRotation = Quaternion.Lerp(
                modelTransform.localRotation,
                modelOriginalRot,
                Time.deltaTime * 8f
            );
        }

        // Reposicionar agent en navmesh
        if (agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }

        if (stateTimer <= 0f)
        {
            cooldownTimer = lungeCooldown;
            ChangeState(State.Chase);
        }
    }

    #endregion

    #region STATE MANAGEMENT

    private void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                stateTimer = 0f;
                break;

            case State.Chase:
                agent.updatePosition = true;
                agent.isStopped = false;
                break;

            case State.LungeWindup:
                stateTimer = windupDuration;
                hasHitThisLunge = false;
                break;

            case State.LungeDash:
                stateTimer = lungeDuration;
                break;

            case State.LungeRecover:
                stateTimer = recoverDuration;
                break;
        }
    }

    #endregion

    #region COLLISION

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CheckPlayerHit(hit.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckPlayerHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPlayerHit(other.gameObject);
    }

    private void CheckPlayerHit(GameObject obj)
    {
        // Solo hace daño durante el lunge dash
        if (currentState != State.LungeDash) return;

        // Solo una vez por lunge
        if (hasHitThisLunge) return;

        if (!obj.CompareTag("Player")) return;

        hasHitThisLunge = true;

        // Quitar tiempo del combat timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.RemoveTime(attackDamage);
        }

        Debug.Log($"FILTH LUNGE HIT! -{attackDamage}s");
    }

    #endregion

    #region HELPERS

    private void LookAtPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    #endregion

    #region DEBUG

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lungeRange);
    }

    #endregion
}
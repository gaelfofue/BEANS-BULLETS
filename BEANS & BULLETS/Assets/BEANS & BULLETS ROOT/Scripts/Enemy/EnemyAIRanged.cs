using UnityEngine;

public class EnemyAIRanged : MonoBehaviour
{
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Cooldown
    }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float tooCloseRange = 4f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float retreatSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Attack")]
    [SerializeField] private float timeBetweenShots = 1.5f;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 2.5f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Burst (optional)")]
    [SerializeField] private int shotsPerBurst = 1;
    [SerializeField] private float burstDelay = 0.2f;

    // State
    private State currentState = State.Idle;
    private float cooldownTimer;
    private int burstRemaining;
    private float burstTimer;

    // References
    private Transform player;

    void Start()
    {
        // Limpiar componentes que causan problemas
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            Destroy(agent);
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);

        var cc = GetComponent<CharacterController>();
        if (cc != null)
            Destroy(cc);

        // Buscar player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Sincronizar Y con player
        Vector3 pos = transform.position;
        pos.y = player.position.y;
        transform.position = pos;

        // Burst pendiente
        if (burstRemaining > 0)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                FireProjectile();
                burstRemaining--;
                burstTimer = burstDelay;
            }
            return;
        }

        float dist = GetHorizontalDistance();

        switch (currentState)
        {
            case State.Idle:
                if (dist <= detectionRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                UpdateChase(dist);
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Cooldown:
                UpdateCooldown(dist);
                break;
        }
    }

    // ==================
    // STATES
    // ==================

    void UpdateChase(float dist)
    {
        // Siempre mirar al player
        LookAtPlayer();

        if (dist <= tooCloseRange)
        {
            // Demasiado cerca, retroceder
            Retreat();
        }
        else if (dist <= attackRange)
        {
            // En rango de ataque
            currentState = State.Attack;
        }
        else
        {
            // Perseguir
            MoveTowardsPlayer();
        }
    }

    void UpdateAttack()
    {
        LookAtPlayer();

        // Disparar
        if (shotsPerBurst > 1)
        {
            // Burst
            FireProjectile();
            burstRemaining = shotsPerBurst - 1;
            burstTimer = burstDelay;
        }
        else
        {
            // Disparo único
            FireProjectile();
        }

        cooldownTimer = timeBetweenShots;
        currentState = State.Cooldown;
    }

    void UpdateCooldown(float dist)
    {
        cooldownTimer -= Time.deltaTime;

        LookAtPlayer();

        // Mientras espera, mantener distancia
        if (dist <= tooCloseRange)
        {
            Retreat();
        }

        if (cooldownTimer <= 0f)
        {
            if (dist <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
    }

    // ==================
    // SHOOTING
    // ==================

    void FireProjectile()
    {
        if (projectilePrefab == null) return;

        // Punto de disparo
        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.up;

        // Dirección al player
        Vector3 dir = (player.position - spawnPos);

        // Apuntar ligeramente al centro del player (no a los pies)
        dir.y += 0.5f;
        dir.Normalize();

        // Crear proyectil
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Lanzar
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.Launch(dir, projectileSpeed, projectileDamage);
        }

        Debug.Log("[ENEMY RANGED] Fired!");
    }

    // ==================
    // MOVEMENT
    // ==================

    void MoveTowardsPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.25f)
        {
            dir.Normalize();
            Vector3 move = dir * chaseSpeed * Time.deltaTime;
            move.y = 0f;
            transform.position += move;
        }
    }

    void Retreat()
    {
        Vector3 dir = transform.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            Vector3 move = dir * retreatSpeed * Time.deltaTime;
            move.y = 0f;
            transform.position += move;
        }
    }

    void LookAtPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    float GetHorizontalDistance()
    {
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    #region GIZMOS

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tooCloseRange);
    }
    #endregion
}
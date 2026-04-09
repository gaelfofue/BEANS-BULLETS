using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    #region General Variables
    [Header("AI Configuration")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask targetLayer;

    [Header("Patroling Stats")]
    [SerializeField] private float walkPointRange = 8f;
    [SerializeField] private bool useWaypoints;
    [SerializeField] private Transform[] waypoints;

    [Header("Attacking Stats")]
    [SerializeField] private float timeBetweenAttacks = 1.5f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileDamage = 2.5f;
    [SerializeField] private float projectileGravity = 0f; // Para arco parab�lico

    [Header("States & Detection Areas")]
    [SerializeField] private float sightRange = 20f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float tooCloseRange = 4f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float retreatSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;

    // Internal State
    private bool targetInSightRange;
    private bool targetInAttackRange;
    private bool alreadyAttacked;

    // Patrol
    private Vector3 walkPoint;
    private bool walkPointSet;
    private int currentWaypointIndex;

    // NavMesh (opcional)
    private NavMeshAgent agent;
    private bool useNavMesh = false;

    // Direct Movement (fallback)
    private bool useDirectMovement = false;

    // Stuck Detection
    [Header("Stuck Detection")]
    [SerializeField] private float stuckCheckTime = 2f;
    [SerializeField] private float stuckThreshold = 0.1f;
    [SerializeField] private float maxStuckDuration = 3f;

    private float stuckTimer;
    private float lastCheckTime;
    private Vector3 lastPosition;
    #endregion

    void Start()
    {
        // Buscar player por tag (m�s seguro)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;

        // Intentar usar NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                useNavMesh = true;
                agent.speed = chaseSpeed;
                agent.acceleration = 40f;
                agent.angularSpeed = rotationSpeed;
                agent.stoppingDistance = attackRange * 0.8f;
                agent.autoBraking = false;
            }
            else
            {
                // Intentar posicionar en NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    useNavMesh = true;
                    agent.speed = chaseSpeed;
                }
                else
                {
                    // No hay NavMesh, usar movimiento directo
                    agent.enabled = false;
                    useDirectMovement = true;
                    Debug.LogWarning($"[ENEMY] {name} no NavMesh found, using direct movement");
                }
            }
        }
        else
        {
            useDirectMovement = true;
        }

        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }

    void Update()
    {
        if (target == null) return;

        // Sincronizar Y con player si usa movimiento directo
        if (useDirectMovement)
        {
            Vector3 pos = transform.position;
            pos.y = target.position.y;
            transform.position = pos;
        }

        EnemyStateUpdater();
        CheckIfStuck();
    }

    void EnemyStateUpdater()
    {
        // Detecci�n con OverlapSphere
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
        targetInSightRange = hits.Length > 0;

        if (targetInSightRange)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            targetInAttackRange = distance <= attackRange;
        }
        else
        {
            targetInAttackRange = false;
        }

        // Estado
        if (!targetInSightRange && !targetInAttackRange)
            Patroling();
        else if (targetInSightRange && !targetInAttackRange)
            ChaseTarget();
        else if (targetInSightRange && targetInAttackRange)
            AttackTarget();
    }

    void Patroling()
    {
        if (!walkPointSet)
        {
            if (useWaypoints && waypoints.Length > 0)
            {
                walkPoint = waypoints[currentWaypointIndex].position;
                walkPointSet = true;
            }
            else
            {
                SearchWalkPoint();
            }
        }

        if (walkPointSet)
        {
            MoveTowards(walkPoint);

            // Llegamos al punto
            float dist = Vector3.Distance(transform.position, walkPoint);
            if (dist < 1.5f)
            {
                walkPointSet = false;

                if (useWaypoints && waypoints.Length > 0)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Length)
                        currentWaypointIndex = 0;
                }
            }
        }
    }

    void SearchWalkPoint()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = transform.position + new Vector3(
                Random.Range(-walkPointRange, walkPointRange),
                0f,
                Random.Range(-walkPointRange, walkPointRange)
            );

            if (useNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
                {
                    walkPoint = hit.position;
                    walkPointSet = true;
                    return;
                }
            }
            else
            {
                // Movimiento directo: solo usar el punto random
                walkPoint = randomPoint;
                walkPoint.y = target.position.y;
                walkPointSet = true;
                return;
            }
        }
    }

    void ChaseTarget()
    {
        float dist = Vector3.Distance(transform.position, target.position);

        // Si demasiado cerca, retroceder
        if (dist < tooCloseRange)
        {
            Retreat();
        }
        else
        {
            MoveTowards(target.position);
        }

        LookAtTarget();
    }

    void AttackTarget()
    {
        // Detenerse
        if (useNavMesh && agent.isOnNavMesh)
            agent.SetDestination(transform.position);

        // Mirar al target
        LookAtTarget();

        // Disparar
        if (!alreadyAttacked)
        {
            FireProjectile();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        // Mantener distancia
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < tooCloseRange)
            Retreat();
    }

    void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"[ENEMY] {name} has no projectile prefab!");
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.up;

        // Direcci�n al player
        Vector3 dir = (target.position - spawnPos);
        dir.y += 0.5f; // Apuntar al centro del player
        dir.Normalize();

        // Instanciar
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Si tiene el script EnemyProjectile
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.Launch(dir, projectileSpeed, projectileDamage);
        }
        else
        {
            // Fallback: usar Rigidbody si existe
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = dir * projectileSpeed;
                if (projectileGravity > 0f)
                    rb.AddForce(Vector3.up * projectileGravity, ForceMode.Impulse);
            }
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // ==================
    // MOVEMENT
    // ==================

    void MoveTowards(Vector3 destination)
    {
        if (useNavMesh && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
        else
        {
            // Movimiento directo
            Vector3 dir = destination - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.25f)
            {
                dir.Normalize();
                Vector3 move = dir * chaseSpeed * Time.deltaTime;
                move.y = 0f;
                transform.position += move;
            }
        }
    }

    void Retreat()
    {
        Vector3 dir = transform.position - target.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();

            if (useNavMesh && agent.isOnNavMesh)
            {
                Vector3 retreatPoint = transform.position + dir * 2f;
                agent.SetDestination(retreatPoint);
            }
            else
            {
                Vector3 move = dir * retreatSpeed * Time.deltaTime;
                move.y = 0f;
                transform.position += move;
            }
        }
    }

    void LookAtTarget()
    {
        Vector3 dir = target.position - transform.position;
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

    void CheckIfStuck()
    {
        if (Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckThreshold)
            {
                stuckTimer += stuckCheckTime;
            }
            else
            {
                stuckTimer = 0;
            }

            if (stuckTimer >= maxStuckDuration)
            {
                walkPointSet = false;
                if (useNavMesh && agent.isOnNavMesh)
                    agent.ResetPath();
                stuckTimer = 0;
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tooCloseRange);
    }
}
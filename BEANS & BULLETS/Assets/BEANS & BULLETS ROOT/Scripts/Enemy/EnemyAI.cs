using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 30f;
    public float attackRange = 2f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    private float nextAttackTime;

    [Header("Movement")]
    public float moveSpeed = 8f;

    // References
    private NavMeshAgent agent;
    private Transform player;
    private bool playerDetected = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        // Buscar player automáticamente
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Detectar
        if (distance <= detectionRange)
        {
            playerDetected = true;
        }

        if (!playerDetected) return;

        // Perseguir o atacar
        if (distance > attackRange)
        {
            Chase();
        }
        else
        {
            Attack();
        }
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        // Parar
        agent.isStopped = true;

        // Mirar al player
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Atacar con cooldown
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            // TODO: Hacer daño al player cuando 
            // tengamos PlayerHealth
            Debug.Log("Enemy attacks for " + attackDamage + " damage!");
        }
    }

    // Debug visual
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
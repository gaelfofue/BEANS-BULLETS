using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal.Internal;

public class EnemyAIBase : MonoBehaviour
{
    #region GENERAL VARIABLES
    [Header("AI Configuration")]
    [SerializeField] NavMeshAgent agent; //Ref al cerebro NavMesh del objeto
    [SerializeField] Transform target; //Ref a la posicion del targer a perseguir
    [SerializeField] LayerMask targetLayer; //Define la capa del target (Deteccion)
    [SerializeField] LayerMask groundLayer; //Define la capa del suelo (Definir puntos navegables)

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 8f; //Radio maximo de margen espacial para buscar puntos navegables
    Vector3 walkPoint; //Posicion del punto a perseguir
    bool walkPointSet; //Si es falso, busca punto. Si es verdadero, no puede buscar punto

    [Header("Attacking Stats")]
    [SerializeField] float timeBetweenAttacks = 1f; //Tiempo entre ataque y ataque
    [SerializeField] GameObject projectile; //Ref al prefab del projectil
    [SerializeField] Transform shootPoint; //Posicion inicial del disparo
    [SerializeField] float shootSpeedY; //Potencia de disparo vertical (Solo para catapulta)
    [SerializeField] float shootSpeedZ = 10f; //Potencia de disparo hacia delante (Siempre esta)
    bool alreadyAttacked; //Se pregunta si esta atacando para no stackear ataques (Capa de seguridad)

    [Header("States & Detection Areas")]
    [SerializeField] float sightRange = 10f; //Radio de la deteccion de persecucion
    [SerializeField] float attackRange = 4f; //Radio de la deteccion del ataque
    [SerializeField] bool targetInSightRange; //Determina si entra el estado PERSEGUIR
    [SerializeField] bool targetInAttackRange; //Determina si entra el estado ATACAR

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f; //Tiempo que el agente espera quieto antes de preguntarse si esta stuck
    [SerializeField] float stuckThreshold = 0.1f; //Margen de deteccion de stuck
    [SerializeField] float maxStuckDuration = 3f; //Tiempo maximo de estar stuck

    float stuckTimer; //Reloj que cuenta el tiempo de estar stuck
    float lastCheckTime; //Define el tiempo de chequeo previo a estar stuck
    Vector3 lastPosition; //Posicion del ultimo walkpoint perseguido
    #endregion

    private void Awake()
    {
        target = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }

    void Update()
    {
        EnemyStateUpdater();
        CheckIfStuck();
    }

    void EnemyStateUpdater()
    {
        //Accion que se encarga de la gestion de los estados de la IA
        //Esfera de deteccion fisica
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
        targetInSightRange = hits.Length > 0;
        //Si esta persiguiendo, calcula la distancia hasta que el minimo entre dentro del rango de ataque
        if (targetInSightRange)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            targetInAttackRange = distance <= attackRange;
        }

        //Logica de los cambios de estado
        if (!targetInSightRange && !targetInAttackRange) Patroling();
        else if (targetInSightRange && !targetInAttackRange) ChaseTarget();
        else if (targetInSightRange && targetInAttackRange) AttackTarget();
    }

    void Patroling()
    {
        //Degine que el objeto patrulle y genere puntos de patrulla randoms
        //1 - Revisa si hay punto a patrullar
        if (!walkPointSet)
        {
            //Si no hay walkpoint, busca uno
            SearchWalkPoint();
        }
        else agent.SetDestination(walkPoint); //Si hay punto, lo persigue

        //2 - Una vez ha llegado al punto, hay que decirle al sistema que puede generar uno nuevo
        if ((transform.position - walkPoint).sqrMagnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    void SearchWalkPoint()
    {
        //Accion que busca un punto de patrulla random si no lo hay
        int attempts = 0; //Numero interno de intentos de buscar punto nuevo
        const int maxAttempts = 5; //En pocas palabras, que como maximo, tiene 5 intentos para buscar un punto nuevo

        while (!walkPointSet && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-walkPointRange, walkPointRange), 0, Random.Range(-walkPointRange, walkPointRange));

            //Chequear si el punto esta en un lugar en el que haya NavMesh Surface
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                walkPoint = hit.position; //Determina el Vector 3 random a perseguir
                if (Physics.Raycast(walkPoint, -transform.up, 2f, groundLayer))
                {
                    walkPointSet = true; //Tenemos punto y el agente va hacia el
                }
            }
        }
    }

    void ChaseTarget()
    {
        //Le dice al agente que persiga al target
        agent.SetDestination(target.position);
    }

    void AttackTarget()
    {
        //Accion que determina el ataque al objetivo

        // 1 - Detener el movimiento 
        agent.SetDestination(transform.position);

        // 2 - Rotacion suavizada para mirar al target
        Vector3 direction = (target.position - transform.position).normalized;
        //Condicional que revisa si agente y target NO se estan mirando
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, agent.angularSpeed * Time.deltaTime);
        }

        //3 - Definir el ataque en si
        //Solo atacara si no se esta atacando
        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, shootPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * shootSpeedZ, ForceMode.Impulse);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        //Accion que resetea el ataque
        alreadyAttacked = false;
    }

    void CheckIfStuck()
    {
        //Accion que revisa si el agente esta atrapado
        if (Time.time - lastCheckTime > stuckCheckTime)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved > stuckThreshold && agent.hasPath)
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
                agent.ResetPath();
                stuckTimer = 0;
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return; //Solo se ejecuta los gizmos en el editor de unity, no en build

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}

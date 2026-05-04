using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class RoomPiece : MonoBehaviour
{
    [Header("TYPE")]
    [SerializeField] private PieceType pieceType;

    [Header("CONNECTION POINTS")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    [Header("DOORS (solo salas)")]
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;

    [Header("ENEMIES (solo combat)")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1; // Valor por defecto, será sobreescrito

    [Header("SPAWN SETTINGS")]
    [SerializeField] private float navMeshSearchRadius = 10f;
    [SerializeField] private float spawnDelay = 0.3f;
    [SerializeField] private float initialSpawnDelay = 0.5f;

    public enum PieceType { Combat, Corridor, Shop }

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool activated;
    private bool completed;
    private bool navMeshReady = false;

    // ==================
    // DYNAMIC ENEMY COUNT
    // ==================

    /// <summary>
    /// Llamar desde LevelManager ANTES de Activate para configurar cuántos enemigos spawnear
    /// </summary>
    public void SetEnemyCount(int count)
    {
        enemyCount = Mathf.Max(1, count);
        Debug.Log($"[ROOM] {gameObject.name} enemy count set to {enemyCount}");
    }

    // ==================
    // INITIALIZATION
    // ==================

    public void Initialize()
    {
        if (pieceType == PieceType.Combat || pieceType == PieceType.Shop)
        {
            if (entryDoor != null)
                entryDoor.SetState(Door.DoorState.Open);
            if (exitDoor != null)
                exitDoor.SetState(Door.DoorState.Locked);
        }
    }

    public void SetNavMeshReady()
    {
        navMeshReady = true;
        Debug.Log($"[ROOM] NavMesh ready for {gameObject.name}");
    }

    // ==================
    // ACTIVATION
    // ==================

    public void Activate()
    {
        if (activated) return;
        activated = true;

        if (entryDoor != null)
            entryDoor.SetState(Door.DoorState.Locked);

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Locked);

        if (pieceType == PieceType.Combat)
        {
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
                Debug.Log("[ROOM] Combat timer STARTED");
            }

            StartCoroutine(SpawnEnemiesSequence());
        }
        else if (pieceType == PieceType.Shop)
        {
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
                Debug.Log("[ROOM] Timer PAUSED (Shop)");
            }

            if (ShopManager.Instance != null)
                ShopManager.Instance.SetCurrentShopRoom(this);
        }
        else if (pieceType == PieceType.Corridor)
        {
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
                Debug.Log("[ROOM] Timer PAUSED (Corridor)");
            }
        }
    }

    // ==================
    // ENEMY SPAWNING - ALEATORIO
    // ==================

    private IEnumerator SpawnEnemiesSequence()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || enemyPrefab == null)
        {
            Debug.LogError("[ROOM] No spawn points or enemy prefab assigned!");
            Complete();
            yield break;
        }

        // Esperar NavMesh
        float timeout = 5f;
        float elapsed = 0f;
        while (!navMeshReady && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!navMeshReady)
        {
            Debug.LogWarning("[ROOM] NavMesh not ready after timeout");
        }

        // Espera inicial
        yield return new WaitForSeconds(initialSpawnDelay);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Verificar NavMesh
        NavMeshHit testHit;
        bool navMeshHasData = NavMesh.SamplePosition(transform.position, out testHit, 50f, NavMesh.AllAreas);

        if (!navMeshHasData)
        {
            Debug.LogError($"[ROOM] NavMesh vacío en {gameObject.name}");
            Complete();
            yield break;
        }

        activeEnemies.Clear();

        // Generar lista de spawn points aleatorizados
        List<Transform> randomizedSpawnPoints = GetRandomizedSpawnPoints(enemyCount);

        Debug.Log($"[ROOM] Spawning {enemyCount} enemies across {spawnPoints.Length} spawn points (randomized)");

        for (int i = 0; i < enemyCount; i++)
        {
            Transform sp = randomizedSpawnPoints[i];

            Vector3 spawnPosition;
            if (FindValidNavMeshPosition(sp.position, out spawnPosition))
            {
                GameObject go = SpawnEnemySafe(spawnPosition, sp.rotation);

                if (go != null)
                {
                    EnemyHealth health = go.GetComponentInChildren<EnemyHealth>();
                    if (health != null)
                    {
                        activeEnemies.Add(health);
                        health.SetRoom(this);
                        Debug.Log($"[ROOM] Enemy {i + 1}/{enemyCount} spawned at {spawnPosition} (point: {sp.name})");
                    }
                    else
                    {
                        Debug.LogError($"[ROOM] Enemy prefab has no EnemyHealth!");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ROOM] No valid NavMesh near {sp.name}, trying force spawn");

                GameObject go = SpawnEnemyForced(sp.position, sp.rotation);
                if (go != null)
                {
                    EnemyHealth health = go.GetComponentInChildren<EnemyHealth>();
                    if (health != null)
                    {
                        activeEnemies.Add(health);
                        health.SetRoom(this);
                        Debug.LogWarning($"[ROOM] Enemy {i + 1}/{enemyCount} FORCE spawned at {sp.position}");
                    }
                }
            }

            if (i < enemyCount - 1)
                yield return new WaitForSeconds(spawnDelay);
        }

        if (activeEnemies.Count == 0)
        {
            Debug.LogError("[ROOM] No enemies spawned! Auto-completing.");
            Complete();
        }
        else
        {
            Debug.Log($"[ROOM] All {activeEnemies.Count} enemies spawned successfully");
        }
    }

    /// <summary>
    /// Genera una lista de spawn points aleatorizados del tamaño necesario.
    /// Si hay más enemigos que spawn points, reutiliza con offset aleatorio.
    /// </summary>
    private List<Transform> GetRandomizedSpawnPoints(int count)
    {
        List<Transform> result = new List<Transform>();

        // Crear lista barajada de todos los spawn points
        List<Transform> shuffled = new List<Transform>(spawnPoints);
        ShuffleList(shuffled);

        // Si tenemos suficientes puntos, usar los primeros N
        if (count <= shuffled.Count)
        {
            for (int i = 0; i < count; i++)
            {
                result.Add(shuffled[i]);
            }
        }
        else
        {
            // Más enemigos que puntos: repartir de forma round-robin aleatorio
            // Primero usar todos una vez (barajados)
            result.AddRange(shuffled);

            // Luego re-barajar y seguir añadiendo
            int remaining = count - shuffled.Count;
            while (remaining > 0)
            {
                ShuffleList(shuffled);
                int toAdd = Mathf.Min(remaining, shuffled.Count);
                for (int i = 0; i < toAdd; i++)
                {
                    result.Add(shuffled[i]);
                }
                remaining -= toAdd;
            }
        }

        return result;
    }

    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // ==================
    // SPAWN HELPERS
    // ==================

    private bool FindValidNavMeshPosition(Vector3 sourcePosition, out Vector3 result)
    {
        NavMeshHit hit;
        float[] searchRadii = { 1f, 2f, 5f, navMeshSearchRadius };

        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(sourcePosition, out hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = sourcePosition;
        return false;
    }

    private GameObject SpawnEnemySafe(Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate(enemyPrefab);

        NavMeshAgent agent = go.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        go.transform.position = position;
        go.transform.rotation = rotation;

        if (agent != null)
            StartCoroutine(EnableNavMeshAgentSafe(agent, position));

        return go;
    }

    private GameObject SpawnEnemyForced(Vector3 position, Quaternion rotation)
    {
        RaycastHit hit;
        Vector3 rayStart = position + Vector3.up * 5f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
        {
            position = hit.point + Vector3.up * 0.1f;
        }

        return SpawnEnemySafe(position, rotation);
    }

    private IEnumerator EnableNavMeshAgentSafe(NavMeshAgent agent, Vector3 targetPosition)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (agent == null || agent.gameObject == null) yield break;

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log($"[ROOM] Agent warped to {hit.position}");
            }
            else
            {
                Debug.LogError("[ROOM] Agent could not find NavMesh! Destroying.");
                EnemyHealth health = agent.GetComponentInChildren<EnemyHealth>();
                if (health != null)
                    activeEnemies.Remove(health);
                Destroy(agent.gameObject);
            }
        }
    }

    // ==================
    // ENEMY DEATH
    // ==================

    public void OnEnemyDied(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);
        Debug.Log($"[ROOM] Enemy died. Remaining: {activeEnemies.Count}");

        if (activeEnemies.Count <= 0)
        {
            StartCoroutine(DelayedComplete());
        }
    }

    private IEnumerator DelayedComplete()
    {
        yield return new WaitForSeconds(0.2f);
        Complete();
    }

    // ==================
    // COMPLETION
    // ==================

    private void Complete()
    {
        if (completed) return;
        completed = true;

        Debug.Log("[ROOM] Combat CLEARED");

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
            Debug.Log("[ROOM] Timer STOPPED");
        }

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Closed);

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnRoomCompleted();

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Open);
    }

    // ==================
    // SHOP
    // ==================

    public void OnShopInteractionComplete()
    {
        if (pieceType != PieceType.Shop) return;
        if (completed) return;

        completed = true;

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Closed);

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnShopCompleted();

        Debug.Log("[SHOP] Interaction complete.");
    }

    // ==================
    // CLEANUP
    // ==================

    public void PrepareForUnload()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Locked);
    }

    // ==================
    // GETTERS
    // ==================

    public Transform GetEntryPoint() { return entryPoint; }
    public Transform GetExitPoint() { return exitPoint; }
    public PieceType GetPieceType() { return pieceType; }
    public bool IsActivated() { return activated; }
}
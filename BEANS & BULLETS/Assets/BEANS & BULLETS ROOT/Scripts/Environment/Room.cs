using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("ROOM CONFIG")]
    [SerializeField] private bool isCombatRoom = true;
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;

    [Header("ENEMIES (solo combat rooms)")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 5;

    [Header("TRIGGER")]
    [SerializeField] private BoxCollider roomTrigger;

    // Estado
    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool roomActivated;
    private bool roomCompleted;

    private void Start()
    {
        // Todas las puertas empiezan bloqueadas
        if (entryDoor != null) entryDoor.SetLocked(true);
        if (exitDoor != null) exitDoor.SetLocked(true);
    }

    #region PLAYER ENTERS

    private void OnTriggerEnter(Collider other)
    {
        if (roomActivated) return;
        if (!other.CompareTag("Player")) return;

        ActivateRoom();
    }

    private void ActivateRoom()
    {
        roomActivated = true;

        Debug.Log($"Sala activada: {gameObject.name} | Combate: {isCombatRoom}");

        // Cerrar puerta de entrada
        if (entryDoor != null)
        {
            entryDoor.ForceClose();
            entryDoor.SetLocked(true);
        }

        // Bloquear puerta de salida
        if (exitDoor != null)
        {
            exitDoor.SetLocked(true);
        }

        if (isCombatRoom)
        {
            // Reanudar timer
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.SetPaused(false);
            }

            SpawnEnemies();
        }
        else
        {
            // Es un pasillo: pausar timer y desbloquear salida
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.SetPaused(true);
            }

            CompleteRoom();
        }
    }

    #endregion

    #region ENEMIES

    private void SpawnEnemies()
    {
        activeEnemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

            GameObject enemyGO = Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            EnemyHealth health = enemyGO.GetComponent<EnemyHealth>();

            if (health != null)
            {
                activeEnemies.Add(health);
                health.SetRoom(this);
            }
        }

        Debug.Log($"Spawneados {enemyCount} enemigos");
    }

    // Llamado desde EnemyHealth cuando muere un enemigo
    public void OnEnemyDied(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);

        Debug.Log($"Enemigo muerto. Restantes: {activeEnemies.Count}");

        if (activeEnemies.Count <= 0)
        {
            CompleteRoom();
        }
    }

    #endregion

    #region ROOM CLEAR

    private void CompleteRoom()
    {
        if (roomCompleted) return;

        roomCompleted = true;

        Debug.Log($"SALA LIMPIA: {gameObject.name}");

        // Pausar timer (zona segura)
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.SetPaused(true);
        }

        // Desbloquear AMBAS puertas
        if (entryDoor != null) entryDoor.SetLocked(false);
        if (exitDoor != null) exitDoor.SetLocked(false);
    }

    #endregion

    #region DEBUG

    private void OnDrawGizmos()
    {
        Gizmos.color = roomCompleted ? Color.green : (roomActivated ? Color.red : Color.yellow);
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);

        if (spawnPoints == null) return;

        Gizmos.color = Color.cyan;
        foreach (Transform sp in spawnPoints)
        {
            if (sp != null)
            {
                Gizmos.DrawWireSphere(sp.position, 0.5f);
            }
        }
    }

    #endregion
}
using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("ROOM CONFIG")]
    [SerializeField] private bool isCombatRoom = true;
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;

    [Header("CONNECTION POINTS")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    [Header("ENEMIES")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1;

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool roomActivated;
    private bool roomCompleted;

    private void Start()
    {
        // Puerta de entrada: abierta y desbloqueada para que el player entre
        if (entryDoor != null)
            entryDoor.Unlock();

        // Puerta de salida: cerrada y bloqueada
        if (exitDoor != null)
            exitDoor.Lock();
    }

    // Llamado por LevelManager cuando el player entra
    public void ActivateRoom()
    {
        if (roomActivated) return;
        roomActivated = true;

        Debug.Log($"Sala activada: {gameObject.name}");

        // Cerrar y bloquear entrada (no volver atrás)
        if (entryDoor != null)
            entryDoor.Lock();

        // Salida bloqueada
        if (exitDoor != null)
            exitDoor.Lock();

        if (isCombatRoom)
        {
            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(false);

            SpawnEnemies();
        }
        else
        {
            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(true);

            CompleteRoom();
        }
    }

    private void SpawnEnemies()
    {
        activeEnemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            Transform sp = spawnPoints[i % spawnPoints.Length];
            GameObject enemyGO = Instantiate(enemyPrefab, sp.position, sp.rotation);

            EnemyHealth health = enemyGO.GetComponent<EnemyHealth>();
            if (health != null)
            {
                activeEnemies.Add(health);
                health.SetRoom(this);
            }
        }
    }

    public void OnEnemyDied(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);

        if (activeEnemies.Count <= 0)
            CompleteRoom();
    }

    private void CompleteRoom()
    {
        if (roomCompleted) return;
        roomCompleted = true;

        Debug.Log($"SALA LIMPIA: {gameObject.name}");

        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        // Desbloquear salida
        if (exitDoor != null)
            exitDoor.Unlock();

        // Notificar al LevelManager que puede empezar a cargar lo siguiente
        LevelManager.Instance.OnRoomCompleted();
    }

    public Transform GetEntryPoint() { return entryPoint; }
    public Transform GetExitPoint() { return exitPoint; }
}
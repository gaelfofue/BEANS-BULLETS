using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("ROOM CONFIG")]
    [SerializeField] private bool isCombatRoom = true;
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;

    [Header("ENTRY POINT")]
    [SerializeField] private Transform entryPoint;

    [Header("ENEMIES")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1;

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool roomActivated;
    private bool roomCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (roomActivated) return;
        if (!other.CompareTag("Player")) return;

        ActivateRoom();
    }

    private void ActivateRoom()
    {
        roomActivated = true;

        // Cerrar y bloquear entrada
        if (entryDoor != null)
        {
            entryDoor.Lock();
        }

        // Bloquear salida
        if (exitDoor != null)
        {
            exitDoor.Lock();
        }

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
        {
            CompleteRoom();
        }
    }

    private void CompleteRoom()
    {
        if (roomCompleted) return;
        roomCompleted = true;

        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        if (exitDoor != null)
            exitDoor.Unlock();
    }

    // Usado por LevelManager para alinear la sala
    public Transform GetEntryPoint()
    {
        return entryPoint;
    }
}
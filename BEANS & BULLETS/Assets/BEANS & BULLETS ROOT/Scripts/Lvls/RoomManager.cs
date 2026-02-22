// RoomManager.cs - ESTRUCTURA (implementar después de estética)

using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN DE SALA ===")]
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private GameObject[] doorObjects; // Puertas que se cierran
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private int totalWaves = 3;

    [Header("Trigger")]
    [SerializeField] private BoxCollider roomTrigger; // Trigger de entrada

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int currentWave = 0;
    private bool roomActive = false;
    private bool roomCleared = false;

    // Se activa cuando el jugador entra
    private void OnTriggerEnter(Collider other)
    {
        if (roomCleared || roomActive) return;
        if (!other.CompareTag("Player")) return;

        StartRoom();
    }

    private void StartRoom()
    {
        roomActive = true;
        CloseDoors();
        SpawnWave();
    }

    private void SpawnWave()
    {
        currentWave++;
        // TODO: Instanciar enemigos en spawnPoints
        // TODO: Registrar en activeEnemies
    }

    // Llamar desde EnemyHealth cuando muere un enemigo
    public void OnEnemyKilled(GameObject enemy)
    {
        activeEnemies.Remove(enemy);

        if (activeEnemies.Count <= 0)
        {
            if (currentWave >= totalWaves)
            {
                ClearRoom();
            }
            else
            {
                SpawnWave();
            }
        }
    }

    private void ClearRoom()
    {
        roomCleared = true;
        roomActive = false;
        OpenDoors();
        // TODO: Mostrar recompensa / mutación
    }

    private void CloseDoors()
    {
        foreach (var door in doorObjects)
            door.SetActive(true); // O animación
    }

    private void OpenDoors()
    {
        foreach (var door in doorObjects)
            door.SetActive(false);
    }
}
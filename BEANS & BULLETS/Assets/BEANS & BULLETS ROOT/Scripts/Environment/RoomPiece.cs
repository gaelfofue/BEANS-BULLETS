// RoomPiece.cs
// Adjuntar al Empty padre de CADA sala y CADA pasillo

using UnityEngine;
using System.Collections.Generic;

public class RoomPiece : MonoBehaviour
{
    [Header("=== TYPE ===")]
    [SerializeField] private PieceType pieceType;

    [Header("=== CONNECTION POINTS ===")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    [Header("=== DOORS (solo salas) ===")]
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;

    [Header("=== ENEMIES (solo combat) ===")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 1;

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool activated;
    private bool completed;

    public enum PieceType
    {
        Combat,
        Corridor,
        Shop
    }

    private void Start()
    {
        // Salas: entrada abierta, salida bloqueada
        if (pieceType == PieceType.Combat || pieceType == PieceType.Shop)
        {
            if (entryDoor != null) entryDoor.Unlock();
            if (exitDoor != null) exitDoor.Lock();
        }
    }

    // Llamado cuando el player entra
    public void Activate()
    {
        if (activated) return;
        activated = true;

        Debug.Log($"Pieza activada: {gameObject.name} | Tipo: {pieceType}");

        if (pieceType == PieceType.Combat)
        {
            // Cerrar entrada
            if (entryDoor != null) entryDoor.Lock();

            // Bloquear salida
            if (exitDoor != null) exitDoor.Lock();

            // Timer activo
            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(false);

            SpawnEnemies();

            // Avisar al LevelManager que empiece a precargar
            LevelManager.Instance.StartPreloading();
        }
        else if (pieceType == PieceType.Corridor)
        {
            // Timer pausado en pasillo
            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(true);
        }
        else if (pieceType == PieceType.Shop)
        {
            if (entryDoor != null) entryDoor.Lock();

            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(true);

            // Tienda: salida abierta directamente
            if (exitDoor != null) exitDoor.Unlock();
        }
    }

    private void SpawnEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (enemyPrefab == null) return;

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

        Debug.Log($"Spawneados {enemyCount} enemigos");
    }

    public void OnEnemyDied(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);
        Debug.Log($"Enemigos restantes: {activeEnemies.Count}");

        if (activeEnemies.Count <= 0)
            Complete();
    }

    private void Complete()
    {
        if (completed) return;
        completed = true;

        Debug.Log($"PIEZA COMPLETADA: {gameObject.name}");

        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        if (exitDoor != null)
            exitDoor.Unlock();

        LevelManager.Instance.OnPieceCompleted();
    }

    // === GETTERS ===

    public Transform GetEntryPoint() { return entryPoint; }
    public Transform GetExitPoint() { return exitPoint; }
    public PieceType GetPieceType() { return pieceType; }
    public bool IsActivated() { return activated; }
}
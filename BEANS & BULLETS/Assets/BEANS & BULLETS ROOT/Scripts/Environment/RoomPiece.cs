using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField] private int enemyCount = 1;

    public enum PieceType { Combat, Corridor, Shop }

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool activated;
    private bool completed;

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

    // ==================
    // ACTIVATION (cuando el player entra)
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
            // Iniciar timer
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
                Debug.Log("[ROOM] Combat timer STARTED");
            }

            SpawnEnemies();
        }
        else if (pieceType == PieceType.Shop)
        {
            // Pausar timer en tienda
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
            // Pausar timer en pasillo
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
                Debug.Log("[ROOM] Timer PAUSED (Corridor)");
            }
        }
    }

    // ==================
    // ENEMY SPAWNING
    // ==================

    private void SpawnEnemies()
    {
        if (spawnPoints == null || enemyPrefab == null) return;

        activeEnemies.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            Transform sp = spawnPoints[i % spawnPoints.Length];
            GameObject go = Instantiate(enemyPrefab, sp.position, sp.rotation);

            EnemyHealth health = go.GetComponentInChildren<EnemyHealth>();
            if (health != null)
            {
                activeEnemies.Add(health);
                health.SetRoom(this);
                Debug.Log($"[ROOM] Enemy spawned and registered. Total: {activeEnemies.Count}");
            }
            else
            {
                Debug.LogError($"[ROOM] Enemy prefab has no EnemyHealth! Prefab: {enemyPrefab.name}");
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

        // Pausar timer
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
    // SHOP INTERACTION
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

        Debug.Log("[SHOP] Interaction complete. Exit door unlocked.");
    }

    // ==================
    // CLEANUP
    // ==================

    public void PrepareForUnload()
    {
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
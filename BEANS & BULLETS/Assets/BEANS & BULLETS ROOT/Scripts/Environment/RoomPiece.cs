using UnityEngine;
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
    [SerializeField] private int enemyCount = 1;

    public enum PieceType { Combat, Corridor, Shop }

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool activated;
    private bool completed;

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

    public void Activate()
    {
        if (activated) return;
        activated = true;

        if (pieceType == PieceType.Combat)
        {
            if (entryDoor != null)
                entryDoor.SetState(Door.DoorState.Locked);
            if (exitDoor != null)
                exitDoor.SetState(Door.DoorState.Locked);

            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(false);

            SpawnEnemies();
        }
        else if (pieceType == PieceType.Corridor)
        {
            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(true);
        }
        else if (pieceType == PieceType.Shop)
        {
            if (entryDoor != null)
                entryDoor.SetState(Door.DoorState.Locked);
            if (exitDoor != null)
                exitDoor.SetState(Door.DoorState.Locked);

            if (GameTimer.Instance != null)
                GameTimer.Instance.SetPaused(true);
        }
    }

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

    public void OnEnemyDied(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);

        if (activeEnemies.Count <= 0)
            Complete();
    }

    private void Complete()
    {
        if (completed) return;
        completed = true;

        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Closed);

        LevelManager.Instance.OnRoomCompleted();
    }

    /// <summary>
    /// Llamado por el objeto interactuable de la tienda.
    /// Desbloquea la puerta de salida.
    /// </summary>
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

    public void PrepareForUnload()
    {
        if (exitDoor != null)
            exitDoor.SetState(Door.DoorState.Locked);
    }

    public Transform GetEntryPoint() { return entryPoint; }
    public Transform GetExitPoint() { return exitPoint; }
    public PieceType GetPieceType() { return pieceType; }
    public bool IsActivated() { return activated; }
}
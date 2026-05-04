using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("SCENES")]
    [SerializeField] private string[] combatScenes;
    [SerializeField] private string[] corridorScenes;
    [SerializeField] private string[] shopScenes;

    [Header("SPAWN ROOM")]
    [SerializeField] private RoomPiece spawnRoom;

    [Header("SHOP FREQUENCY")]
    [SerializeField] private int shopEveryXRooms = 3;

    [Header("ENEMY SCALING")]
    [SerializeField] private int baseEnemies = 1;
    [SerializeField] private int extraEnemiesPerRoom = 1;     // Cada X salas suma 1 enemigo
    [SerializeField] private int roomsPerEnemyIncrease = 2;   // Cada cuántas salas sube
    [SerializeField] private int extraEnemiesPerShop = 1;     // Bonus por cada tienda visitada
    [SerializeField] private int maxEnemiesPerRoom = 8;       // Tope máximo

    [Header("TIMING")]
    [SerializeField] private float unloadDelay = 1.2f;

    // Todas las piezas en orden
    private List<Piece> pieces = new List<Piece>();
    private int playerIndex = -1;

    // Siguiente posición de conexión
    private Vector3 nextSpawnPos;

    // Control de carga
    private bool isLoading;
    private bool nextIsRoom = true;
    private int roomsCompleted;
    private int shopsVisited = 0;
    private int combatRoomsSinceLastShop = 0;
    private string lastRoom = "";
    private string lastCorridor = "";

    // Estado del sistema
    private bool initialLoadDone;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        StartCoroutine(InitialLoad());
    }

    // ============================
    // ENEMY SCALING
    // ============================

    /// <summary>
    /// Calcula cuántos enemigos debe tener la sala actual
    /// </summary>
    public int CalculateEnemyCount()
    {
        // Base + incremento por salas completadas + bonus por tiendas
        int fromRooms = 0;
        if (roomsPerEnemyIncrease > 0)
        {
            fromRooms = (roomsCompleted / roomsPerEnemyIncrease) * extraEnemiesPerRoom;
        }

        int fromShops = shopsVisited * extraEnemiesPerShop;

        int total = baseEnemies + fromRooms + fromShops;
        total = Mathf.Clamp(total, 1, maxEnemiesPerRoom);

        Debug.Log($"[SCALING] Enemies: {total} (base:{baseEnemies} + rooms:{fromRooms} [{roomsCompleted} completed] + shops:{fromShops} [{shopsVisited} visited]) | Max:{maxEnemiesPerRoom}");

        return total;
    }

    // ============================
    // INITIAL LOAD
    // ============================

    private IEnumerator InitialLoad()
    {
        if (spawnRoom != null)
        {
            pieces.Add(new Piece
            {
                roomPiece = spawnRoom,
                sceneHandle = default,
                isSceneLoaded = false,
                unloaded = false
            });

            spawnRoom.Initialize();
            nextSpawnPos = spawnRoom.GetExitPoint().position;
            playerIndex = 0;
        }

        // Cargar: Pasillo_1 + Sala_1
        yield return StartCoroutine(LoadOnePiece());
        yield return StartCoroutine(LoadOnePiece());

        initialLoadDone = true;
        Debug.Log($"INITIAL LOAD COMPLETE Piezas: {pieces.Count}");
    }

    // ============================
    // PLAYER EVENTS
    // ============================

    public void OnPlayerEnteredPiece(RoomPiece piece)
    {
        int newIndex = -1;
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].roomPiece == piece)
            {
                newIndex = i;
                break;
            }
        }

        if (newIndex < 0 || newIndex == playerIndex) return;

        playerIndex = newIndex;

        Debug.Log($"PLAYER MOVED Pieza {playerIndex}: {piece.gameObject.name} ({piece.GetPieceType()})");

        piece.Activate();

        if (piece.GetPieceType() == RoomPiece.PieceType.Combat ||
            piece.GetPieceType() == RoomPiece.PieceType.Shop)
        {
            StartCoroutine(OnEnteredRoom());
        }
    }

    public void OnRoomCompleted()
    {
        roomsCompleted++;
        Debug.Log($"ROOM COMPLETED Total: {roomsCompleted}");
    }

    public void OnShopCompleted()
    {
        shopsVisited++;
        Debug.Log($"SHOP COMPLETED Total shops visited: {shopsVisited}");
    }

    // ============================
    // ROOM ENTERED SEQUENCE
    // ============================

    private IEnumerator OnEnteredRoom()
    {
        yield return new WaitForSeconds(unloadDelay);
        yield return StartCoroutine(UnloadBehindPlayer());
        yield return StartCoroutine(FillBuffer());
    }

    // ============================
    // BUFFER
    // ============================

    private IEnumerator FillBuffer()
    {
        int target = 2;

        while (PiecesAhead() < target)
        {
            yield return StartCoroutine(LoadOnePiece());
        }

        Debug.Log($"=== BUFFER OK === {PiecesAhead()} piezas adelante");
    }

    private int PiecesAhead()
    {
        return pieces.Count - 1 - playerIndex;
    }

    // ============================
    // LOADING
    // ============================

    private IEnumerator LoadOnePiece()
    {
        while (isLoading)
            yield return null;

        isLoading = true;

        string sceneName;
        if (nextIsRoom)
            sceneName = PickNextRoom();
        else
            sceneName = PickNextCorridor();

        Debug.Log($"Cargando: {sceneName}");

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone)
            yield return null;

        yield return null;
        yield return null;

        Scene newScene = FindNewScene(sceneName);

        if (!newScene.IsValid())
        {
            Debug.LogError($"No se encontró escena: {sceneName}");
            isLoading = false;
            yield break;
        }

        GameObject[] roots = newScene.GetRootGameObjects();

        // Eliminar AudioListeners duplicados
        foreach (GameObject root in roots)
        {
            var listeners = root.GetComponentsInChildren<AudioListener>();
            foreach (var l in listeners)
                Destroy(l);
        }

        RoomPiece piece = null;
        foreach (GameObject root in roots)
        {
            piece = root.GetComponentInChildren<RoomPiece>();
            if (piece != null) break;
        }

        if (piece == null)
        {
            Debug.LogError($"No RoomPiece en: {sceneName}");
            isLoading = false;
            yield break;
        }

        // Alinear
        Vector3 entryOffset = piece.GetEntryPoint().position - piece.transform.position;
        Vector3 targetPos = nextSpawnPos - entryOffset;
        Vector3 moveOffset = targetPos - piece.transform.position;

        foreach (GameObject root in roots)
            root.transform.position += moveOffset;

        // Esperar después de mover
        yield return new WaitForFixedUpdate();

        // NavMesh
        foreach (GameObject root in roots)
        {
            var surfaces = root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>();
            foreach (var s in surfaces)
            {
                s.BuildNavMesh();
                Debug.Log($"[LEVEL] NavMesh baked for {root.name}");
            }
        }

        yield return null;
        yield return null;
        yield return new WaitForFixedUpdate();

        // Verificar NavMesh
        UnityEngine.AI.NavMeshHit testHit;
        Vector3 testPos = piece.GetEntryPoint().position;
        bool navMeshValid = UnityEngine.AI.NavMesh.SamplePosition(testPos, out testHit, 20f, UnityEngine.AI.NavMesh.AllAreas);
        Debug.Log($"[LEVEL] NavMesh validation at {testPos}: {(navMeshValid ? $"OK → {testHit.position}" : "FAILED")}");

        // Avisar NavMesh listo
        piece.SetNavMeshReady();

        // Configurar enemigos dinámicamente ANTES de Initialize
        if (piece.GetPieceType() == RoomPiece.PieceType.Combat)
        {
            int enemyCount = CalculateEnemyCount();
            piece.SetEnemyCount(enemyCount);
            Debug.Log($"[LEVEL] Room {piece.gameObject.name} set to {enemyCount} enemies");
        }

        // Actualizar conexión
        nextSpawnPos = piece.GetExitPoint().position;

        // Inicializar
        piece.Initialize();

        // Registrar
        pieces.Add(new Piece
        {
            roomPiece = piece,
            sceneHandle = newScene,
            isSceneLoaded = true,
            unloaded = false
        });

        nextIsRoom = !nextIsRoom;
        isLoading = false;

        Debug.Log($"Pieza lista: {sceneName} Z={piece.GetEntryPoint().position.z:F1} | Total: {pieces.Count}");
    }

    private Scene FindNewScene(string sceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name == sceneName)
            {
                bool registered = false;
                foreach (Piece p in pieces)
                {
                    if (!p.unloaded && p.isSceneLoaded && p.sceneHandle == s)
                    {
                        registered = true;
                        break;
                    }
                }

                if (!registered)
                    return s;
            }
        }

        return default;
    }

    // ============================
    // UNLOADING
    // ============================

    private IEnumerator UnloadBehindPlayer()
    {
        for (int i = 0; i < playerIndex; i++)
        {
            Piece p = pieces[i];
            if (p.unloaded) continue;

            if (p.roomPiece != null)
                p.roomPiece.PrepareForUnload();

            if (p.isSceneLoaded && p.sceneHandle.IsValid() && p.sceneHandle.isLoaded)
            {
                Debug.Log($"Descargando pieza {i}: {p.sceneHandle.name}");

                AsyncOperation unload = SceneManager.UnloadSceneAsync(p.sceneHandle);
                if (unload != null)
                {
                    while (!unload.isDone)
                        yield return null;
                }
            }

            p.unloaded = true;
            p.roomPiece = null;
            pieces[i] = p;
        }
    }

    // ============================
    // PICKING
    // ============================

    private string PickNextRoom()
    {
        if (shopScenes.Length > 0 && combatRoomsSinceLastShop >= shopEveryXRooms)
        {
            combatRoomsSinceLastShop = 0;
            Debug.Log($"[LEVEL] Picking SHOP (after {shopEveryXRooms} combat rooms)");
            return shopScenes[Random.Range(0, shopScenes.Length)];
        }

        combatRoomsSinceLastShop++;
        Debug.Log($"[LEVEL] Picking COMBAT (sinceShop: {combatRoomsSinceLastShop}/{shopEveryXRooms})");
        return PickRandom(combatScenes, ref lastRoom);
    }

    private string PickNextCorridor()
    {
        return PickRandom(corridorScenes, ref lastCorridor);
    }

    private string PickRandom(string[] scenes, ref string last)
    {
        if (scenes.Length == 0) return "";

        string chosen = "";
        int attempts = 0;
        do
        {
            chosen = scenes[Random.Range(0, scenes.Length)];
            attempts++;
        }
        while (chosen == last && scenes.Length > 1 && attempts < 10);

        last = chosen;
        return chosen;
    }

    public int GetRoomsCompleted() { return roomsCompleted; }
    public int GetShopsVisited() { return shopsVisited; }

    // ============================
    // DATA
    // ============================

    private struct Piece
    {
        public RoomPiece roomPiece;
        public Scene sceneHandle;
        public bool isSceneLoaded;
        public bool unloaded;
    }
}
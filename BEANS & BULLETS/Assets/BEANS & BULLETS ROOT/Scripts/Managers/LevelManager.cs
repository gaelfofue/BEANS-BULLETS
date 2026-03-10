using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("TIMING")]
    [SerializeField] private float unloadDelay = 1.2f; // Tiempo para que las puertas se cierren

    // Todas las piezas en orden
    private List<Piece> pieces = new List<Piece>();
    private int playerIndex = -1;

    // Siguiente posición de conexión
    private Vector3 nextSpawnPos;

    // Control de carga
    private bool isLoading;
    private bool nextIsRoom = true;
    private int roomsCompleted;
    private int combatRoomsSinceLastShop = 0; // Contador SOLO de salas combat
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
            GameTimer.Instance.SetPaused(true);

        StartCoroutine(InitialLoad());
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
        Debug.Log($"=== INITIAL LOAD COMPLETE === Piezas: {pieces.Count}");
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

        Debug.Log($"=== PLAYER MOVED === Pieza {playerIndex}: {piece.gameObject.name} ({piece.GetPieceType()})");

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
        Debug.Log($"=== ROOM COMPLETED === Total: {roomsCompleted}");
    }

    /// <summary>
    /// Llamado por la tienda cuando el player termina de comprar o decide irse.
    /// </summary>
    public void OnShopCompleted()
    {
        Debug.Log("=== SHOP COMPLETED ===");
        // No incrementamos roomsCompleted porque la tienda no es combate
    }

    // ============================
    // ROOM ENTERED SEQUENCE
    // ============================

    private IEnumerator OnEnteredRoom()
    {
        // Esperar a que las puertas se cierren visualmente
        yield return new WaitForSeconds(unloadDelay);

        // Descargar todo lo anterior al player
        yield return StartCoroutine(UnloadBehindPlayer());

        // Rellenar buffer: asegurar 2 piezas por delante
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

        // NavMesh
        foreach (GameObject root in roots)
        {
            var surfaces = root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>();
            foreach (var s in surfaces)
                s.BuildNavMesh();
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
        // ¿Toca tienda?
        if (shopScenes.Length > 0 && combatRoomsSinceLastShop >= shopEveryXRooms)
        {
            combatRoomsSinceLastShop = 0;
            Debug.Log($"[LEVEL] Picking SHOP (after {shopEveryXRooms} combat rooms)");
            return shopScenes[Random.Range(0, shopScenes.Length)];
        }

        // Es combat, incrementar aquí
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
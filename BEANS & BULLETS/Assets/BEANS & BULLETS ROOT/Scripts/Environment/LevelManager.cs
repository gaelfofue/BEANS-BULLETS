// LevelManager.cs

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

    [Header("SPAWN")]
    [SerializeField] private Transform firstSpawnPoint;

    [Header("CONFIG")]
    [SerializeField] private int maxPiecesAhead = 3;
    [SerializeField] private int shopEveryXRooms = 3;

    // Piezas cargadas en orden
    private List<LoadedPiece> pieces = new List<LoadedPiece>();

    // Dónde conectar la siguiente pieza
    private Vector3 nextSpawnPosition;

    // Índice de la pieza donde está el player
    private int playerIndex = -1;

    // Control
    private bool isLoading;
    private bool nextIsRoom = true;
    private int roomsCompleted;
    private string lastRoom = "";
    private string lastCorridor = "";

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

        nextSpawnPosition = firstSpawnPoint.position;

        // Cargar primera sala
        StartCoroutine(LoadOnePiece());
    }

    // ============================
    // EVENTS
    // ============================

    public void OnPlayerEnteredPiece(RoomPiece piece)
    {
        // Encontrar el índice de esta pieza
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].piece == piece)
            {
                playerIndex = i;
                break;
            }
        }

        Debug.Log($"Player en pieza {playerIndex}: {piece.gameObject.name}");

        // Descargar piezas viejas
        StartCoroutine(UnloadOldPieces());
    }

    public void StartPreloading()
    {
        StartCoroutine(FillBuffer());
    }

    public void OnPieceCompleted()
    {
        roomsCompleted++;
        Debug.Log($"Salas completadas: {roomsCompleted}");
    }

    // ============================
    // LOADING
    // ============================

    private IEnumerator FillBuffer()
    {
        while (PiecesAheadOfPlayer() < maxPiecesAhead)
        {
            yield return StartCoroutine(LoadOnePiece());
        }

        Debug.Log($"Buffer lleno: {PiecesAheadOfPlayer()} piezas adelante");
    }

    private int PiecesAheadOfPlayer()
    {
        if (playerIndex < 0) return pieces.Count;
        return pieces.Count - 1 - playerIndex;
    }

    private IEnumerator LoadOnePiece()
    {
        if (isLoading) yield break;
        isLoading = true;

        // Elegir escena
        string sceneName;
        if (nextIsRoom)
            sceneName = PickNextRoom();
        else
            sceneName = PickNextCorridor();

        Debug.Log($"Cargando: {sceneName}");

        // Cargar aditivamente
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone)
            yield return null;

        yield return null;

        // Encontrar la escena
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            isLoading = false;
            yield break;
        }

        GameObject[] roots = scene.GetRootGameObjects();

        // Encontrar RoomPiece
        RoomPiece piece = null;
        foreach (GameObject root in roots)
        {
            piece = root.GetComponentInChildren<RoomPiece>();
            if (piece != null) break;
        }

        if (piece == null)
        {
            Debug.LogError($"No se encontró RoomPiece en {sceneName}");
            isLoading = false;
            yield break;
        }

        // Calcular offset para alinear
        Vector3 entryWorldPos = piece.GetEntryPoint().position;
        Vector3 offset = nextSpawnPosition - entryWorldPos;

        // Mover todos los roots
        foreach (GameObject root in roots)
            root.transform.position += offset;

        // Actualizar siguiente punto de conexión
        nextSpawnPosition = piece.GetExitPoint().position;

        // Registrar
        LoadedPiece loaded = new LoadedPiece
        {
            sceneName = sceneName,
            piece = piece,
            unloaded = false
        };
        pieces.Add(loaded);

        // Alternar sala/pasillo
        nextIsRoom = !nextIsRoom;
        isLoading = false;

        Debug.Log($"Pieza lista: {sceneName} | Pos: {piece.transform.position} | Total: {pieces.Count}");
    }

    // ============================
    // UNLOADING
    // ============================

    private IEnumerator UnloadOldPieces()
    {
        for (int i = 0; i < playerIndex - 1; i++)
        {
            if (i >= pieces.Count) break;

            LoadedPiece p = pieces[i];
            if (p.unloaded) continue;

            Debug.Log($"Descargando: {p.sceneName}");

            AsyncOperation unload = SceneManager.UnloadSceneAsync(p.sceneName);
            if (unload != null)
            {
                while (!unload.isDone)
                    yield return null;
            }

            p.unloaded = true;
            pieces[i] = p;
        }
    }

    // ============================
    // PICKING
    // ============================

    private string PickNextRoom()
    {
        if (shopScenes.Length > 0 && roomsCompleted > 0 && roomsCompleted % shopEveryXRooms == 0)
            return shopScenes[Random.Range(0, shopScenes.Length)];

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

    private struct LoadedPiece
    {
        public string sceneName;
        public RoomPiece piece;
        public bool unloaded;
    }
}
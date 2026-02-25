// LevelManager.cs (REEMPLAZAR COMPLETO)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("=== SCENES ===")]
    [SerializeField] private string[] combatScenes;
    [SerializeField] private string[] corridorScenes;
    [SerializeField] private string[] shopScenes;

    [Header("=== SPAWN ===")]
    [SerializeField] private Transform firstSpawnPoint;

    [Header("=== SHOP FREQUENCY ===")]
    [SerializeField] private int shopEveryXRooms = 3;

    private List<LoadedPiece> pieces = new List<LoadedPiece>();
    private Vector3 nextSpawnPosition;
    private int playerIndex = -1;
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

        StartCoroutine(LoadNextPiece());
    }

    // ============================
    // EVENTS
    // ============================

    public void OnPlayerEnteredPiece(RoomPiece piece)
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].piece == piece)
            {
                playerIndex = i;
                break;
            }
        }

        Debug.Log($"Player en pieza {playerIndex}: {piece.gameObject.name}");
    }

    public void OnPieceCompleted()
    {
        roomsCompleted++;
        Debug.Log($"Salas completadas: {roomsCompleted}");
    }

    public void OnPlayerEnteredCorridor()
    {
        Debug.Log("Player en pasillo, descargando piezas viejas");
        StartCoroutine(UnloadOldPieces());

        // Cargar la siguiente sala
        LoadNext();
    }

    public void OnPlayerExitedCorridor()
    {
        Debug.Log("Player salió del pasillo");
        StartCoroutine(UnloadOldPieces());
    }

    // Llamado cuando se completa una sala o cuando se necesita la siguiente pieza
    public void LoadNext()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadNextPiece());
        }
    }

    // ============================
    // LOADING
    // ============================

    private IEnumerator LoadNextPiece()
    {
        if (isLoading) yield break;
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

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            isLoading = false;
            yield break;
        }

        GameObject[] roots = scene.GetRootGameObjects();

        RoomPiece piece = null;
        foreach (GameObject root in roots)
        {
            piece = root.GetComponentInChildren<RoomPiece>();
            if (piece != null) break;
        }

        if (piece == null)
        {
            isLoading = false;
            yield break;
        }

        // Alinear
        Vector3 entryLocalOffset = piece.GetEntryPoint().position - piece.transform.position;
        Vector3 rootTargetPos = nextSpawnPosition - entryLocalOffset;
        Vector3 offset = rootTargetPos - piece.transform.position;

        foreach (GameObject root in roots)
            root.transform.position += offset;

        // Rebake NavMesh
        foreach (GameObject root in roots)
        {
            var surfaces = root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>();
            foreach (var surface in surfaces)
            {
                surface.BuildNavMesh();
            }
        }

        nextSpawnPosition = piece.GetExitPoint().position;

        pieces.Add(new LoadedPiece
        {
            sceneName = sceneName,
            piece = piece,
            unloaded = false
        });

        nextIsRoom = !nextIsRoom;
        isLoading = false;

        Debug.Log($"Pieza lista: {sceneName} en Z={piece.GetEntryPoint().position.z:F1} | Total: {pieces.Count}");
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

    private struct LoadedPiece
    {
        public string sceneName;
        public RoomPiece piece;
        public bool unloaded;
    }
}
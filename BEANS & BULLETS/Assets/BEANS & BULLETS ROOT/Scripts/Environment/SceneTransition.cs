using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("ROOM SCENES")]
    [SerializeField] private string[] roomScenes;

    [Header("ANCHOR")]
    [SerializeField] private Transform roomAnchor;

    private string lastRoom = "";
    private string currentLoadedRoom = "";
    private int roomsCompleted;

    private AsyncOperation preloadOperation;
    private string preloadedSceneName = "";
    private bool roomReady;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadFirstRoom();
    }

    private void LoadFirstRoom()
    {
        string chosen = PickRandomRoom();
        StartCoroutine(LoadAndPositionRoom(chosen));
    }

    public void PreloadNextRoom()
    {
        if (preloadOperation != null) return;

        if (!string.IsNullOrEmpty(currentLoadedRoom))
        {
            SceneManager.UnloadSceneAsync(currentLoadedRoom);
            currentLoadedRoom = "";
        }

        preloadedSceneName = PickRandomRoom();
        roomReady = false;

        StartCoroutine(PreloadRoom(preloadedSceneName));

        Debug.Log($"Precargando: {preloadedSceneName}");
    }

    private IEnumerator PreloadRoom(string sceneName)
    {
        preloadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        preloadOperation.allowSceneActivation = false;

        while (preloadOperation.progress < 0.9f)
        {
            yield return null;
        }

        roomReady = true;
        Debug.Log($"Sala precargada: {sceneName}");
    }

    public void ActivatePreloadedRoom()
    {
        if (preloadOperation == null)
        {
            Debug.LogError("No hay sala precargada");
            return;
        }

        StartCoroutine(ActivateRoom());
    }

    private IEnumerator ActivateRoom()
    {
        while (!roomReady)
        {
            Debug.Log("Esperando carga...");
            yield return null;
        }

        preloadOperation.allowSceneActivation = true;

        while (!preloadOperation.isDone)
        {
            yield return null;
        }

        currentLoadedRoom = preloadedSceneName;
        preloadOperation = null;
        preloadedSceneName = "";

        yield return null;

        PositionRoom(currentLoadedRoom);

        roomsCompleted++;
        Debug.Log($"Sala activada: {currentLoadedRoom} | Total: {roomsCompleted}");
    }

    private IEnumerator LoadAndPositionRoom(string sceneName)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!load.isDone)
        {
            yield return null;
        }

        currentLoadedRoom = sceneName;

        yield return null;

        PositionRoom(sceneName);

        Debug.Log($"Primera sala cargada: {sceneName}");
    }

    private void PositionRoom(string sceneName)
    {
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        if (!loadedScene.IsValid()) return;

        GameObject[] rootObjects = loadedScene.GetRootGameObjects();

        // Buscar Room en la sala cargada
        Room room = null;

        foreach (GameObject root in rootObjects)
        {
            room = root.GetComponentInChildren<Room>();
            if (room != null) break;
        }

        if (room == null)
        {
            foreach (GameObject root in rootObjects)
            {
                root.transform.position = roomAnchor.position + root.transform.position;
            }
            return;
        }

        // La entrada de la sala debe coincidir con el RoomAnchor
        Vector3 entryPos = room.GetEntryPoint().position;
        Vector3 offset = roomAnchor.position - entryPos;

        foreach (GameObject root in rootObjects)
        {
            root.transform.position += offset;
        }

        Debug.Log($"Sala alineada por puerta de entrada");
    }

    private string PickRandomRoom()
    {
        if (roomScenes.Length == 0)
        {
            Debug.LogError("No hay salas configuradas");
            return "";
        }

        string chosen = "";
        int attempts = 0;

        do
        {
            chosen = roomScenes[Random.Range(0, roomScenes.Length)];
            attempts++;
        }
        while (chosen == lastRoom && roomScenes.Length > 1 && attempts < 10);

        lastRoom = chosen;
        return chosen;
    }

    public bool IsRoomReady()
    {
        return roomReady;
    }

    public int GetRoomsCompleted()
    {
        return roomsCompleted;
    }
}
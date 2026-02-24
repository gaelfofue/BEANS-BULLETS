using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("SCENES")]
    [SerializeField] private string[] combatScenes;
    [SerializeField] private string[] shopScenes;

    [Header("CORRIDOR")]
    [SerializeField] private Transform corridorStart;
    [SerializeField] private Transform corridorEnd;

    [Header("SHOP FREQUENCY")]
    [SerializeField] private int shopEveryXRooms = 3;

    private string currentLoadedRoom = "";
    private string lastRoom = "";
    private int roomsCompleted;
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
        // Timer pausado al inicio
        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        // Cargar primera sala
        StartCoroutine(LoadRoom(PickNextRoom()));
    }

    #region TRIGGERS

    // Player sale de la sala → teleport al pasillo
    public void OnPlayerExitRoom()
    {
        Debug.Log("Player salió de la sala → teleport al pasillo");

        // Pausar timer en el pasillo
        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        // Teleportar al inicio del pasillo
        TeleportPlayer(corridorStart.position);

        // Descargar sala actual y cargar siguiente
        StartCoroutine(UnloadAndLoadNext());
    }

    // Player llega al final del pasillo → activar sala si está lista
    public void OnPlayerReachCorridorEnd()
    {
        if (!roomReady)
        {
            Debug.Log("Sala aún cargando, esperando...");
            StartCoroutine(WaitForRoomAndContinue());
            return;
        }

        Debug.Log("Player llegó al final del pasillo, sala lista");
    }

    #endregion

    #region TELEPORT

    private void TeleportPlayer(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = position;
    }

    #endregion

    #region SCENE LOADING

    private IEnumerator LoadRoom(string sceneName)
    {
        roomReady = false;

        Debug.Log($"Cargando: {sceneName}");

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!load.isDone)
            yield return null;

        currentLoadedRoom = sceneName;

        yield return null;

        AlignRoom(sceneName);

        roomReady = true;

        Debug.Log($"Sala lista: {sceneName}");
    }

    private IEnumerator UnloadAndLoadNext()
    {
        roomReady = false;

        // Descargar sala actual
        if (!string.IsNullOrEmpty(currentLoadedRoom))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentLoadedRoom);
            while (unload != null && !unload.isDone)
                yield return null;

            currentLoadedRoom = "";
        }

        // Resetear triggers
        ResetAllZoneTriggers();

        // Cargar siguiente
        string next = PickNextRoom();
        yield return StartCoroutine(LoadRoom(next));

        roomsCompleted++;
        Debug.Log($"Salas completadas: {roomsCompleted}");
    }

    private IEnumerator WaitForRoomAndContinue()
    {
        while (!roomReady)
        {
            yield return null;
        }

        Debug.Log("Sala terminó de cargar, player puede continuar");
    }

    #endregion

    #region ALIGN

    private void AlignRoom(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) return;

        Room room = null;
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            room = root.GetComponentInChildren<Room>();
            if (room != null) break;
        }

        if (room == null || room.GetEntryPoint() == null)
        {
            foreach (GameObject root in roots)
                root.transform.position += corridorEnd.position;
            return;
        }

        Vector3 entryPos = room.GetEntryPoint().position;
        Vector3 offset = corridorEnd.position - entryPos;

        foreach (GameObject root in roots)
            root.transform.position += offset;

        Debug.Log($"Sala alineada con final del pasillo");
    }

    #endregion

    #region ROOM SELECTION

    private string PickNextRoom()
    {
        if (shopScenes.Length > 0 && roomsCompleted > 0 && roomsCompleted % shopEveryXRooms == 0)
        {
            return shopScenes[Random.Range(0, shopScenes.Length)];
        }

        if (combatScenes.Length == 0)
        {
            Debug.LogError("No hay salas configuradas");
            return "";
        }

        string chosen = "";
        int attempts = 0;

        do
        {
            chosen = combatScenes[Random.Range(0, combatScenes.Length)];
            attempts++;
        }
        while (chosen == lastRoom && combatScenes.Length > 1 && attempts < 10);

        lastRoom = chosen;
        return chosen;
    }

    #endregion

    #region HELPERS

    private void ResetAllZoneTriggers()
    {
        ZoneTrigger[] triggers = FindObjectsOfType<ZoneTrigger>();
        foreach (ZoneTrigger t in triggers)
            t.ResetTrigger();
    }

    public bool IsRoomReady() { return roomReady; }
    public int GetRoomsCompleted() { return roomsCompleted; }

    #endregion
}
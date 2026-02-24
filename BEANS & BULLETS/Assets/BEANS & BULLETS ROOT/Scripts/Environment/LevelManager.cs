using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("SCENES")]
    [SerializeField] private string[] roomScenes;

    [Header("CORRIDOR REFERENCES")]
    [SerializeField] private Transform corridorExit;
    [SerializeField] private Transform corridorEntry;
    [SerializeField] private Door corridorEntryDoor;
    [SerializeField] private Door corridorExitDoor;

    [Header("TRIGGERS")]
    [SerializeField] private BoxCollider corridorTrigger;
    [SerializeField] private BoxCollider exitRoomTrigger;
    [SerializeField] private BoxCollider enterRoomTrigger;

    private string currentLoadedRoom = "";
    private string lastRoom = "";
    private int roomsCompleted;

    // Preload
    private AsyncOperation preloadOp;
    private bool roomReady;
    private bool playerInCorridor;

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

        // Puertas del pasillo: entrada abierta, salida cerrada
        if (corridorEntryDoor != null) corridorEntryDoor.Unlock();
        if (corridorExitDoor != null) corridorExitDoor.Lock();

        // Cargar primera sala
        StartCoroutine(LoadFirstRoom());
    }

    private void Update()
    {
        // Mientras el player está en el pasillo, 
        // desbloquear salida cuando la sala esté lista
        if (playerInCorridor && roomReady)
        {
            if (corridorExitDoor != null && corridorExitDoor.IsLocked())
            {
                corridorExitDoor.Unlock();
                Debug.Log("Sala lista - puerta pasillo desbloqueada");
            }
        }
    }

    #region TRIGGERS
    public void OnPlayerEnterCorridor()
    {
        if (playerInCorridor) return;
        playerInCorridor = true;

        Debug.Log("Player entró al pasillo");

        // Cerrar entrada del pasillo
        if (corridorEntryDoor != null) corridorEntryDoor.Lock();

        // Timer pausado
        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);

        // Si la sala ya está lista, abrir salida
        if (roomReady && corridorExitDoor != null)
            corridorExitDoor.Unlock();
    }

    public void OnPlayerExitCorridor()
    {
        Debug.Log("Player salió del pasillo hacia la sala");

        // Cerrar salida del pasillo detrás del player
        if (corridorExitDoor != null) corridorExitDoor.Lock();
    }

    public void OnPlayerExitRoom()
    {
        Debug.Log("Player salió de la sala");

        // Teleportar al inicio del pasillo
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = corridorEntry.position;

        // Resetear pasillo
        playerInCorridor = false;
        if (corridorEntryDoor != null) corridorEntryDoor.Unlock();
        if (corridorExitDoor != null) corridorExitDoor.Lock();

        // Descargar sala actual y precargar siguiente
        StartCoroutine(UnloadAndPreload());
    }

    #endregion

    #region LOADING

    private IEnumerator LoadFirstRoom()
    {
        string chosen = PickRandomRoom();

        AsyncOperation load = SceneManager.LoadSceneAsync(chosen, LoadSceneMode.Additive);
        while (!load.isDone)
            yield return null;

        currentLoadedRoom = chosen;
        yield return null;

        AlignRoom(chosen);
        roomReady = true;

        // Primera sala lista, abrir pasillo
        if (corridorExitDoor != null)
            corridorExitDoor.Unlock();

        Debug.Log($"Primera sala cargada: {chosen}");
    }

    private IEnumerator UnloadAndPreload()
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

        // Cargar nueva sala
        string chosen = PickRandomRoom();

        preloadOp = SceneManager.LoadSceneAsync(chosen, LoadSceneMode.Additive);
        preloadOp.allowSceneActivation = false;

        // Esperar a que esté lista
        while (preloadOp.progress < 0.9f)
            yield return null;

        // Activar
        preloadOp.allowSceneActivation = true;
        while (!preloadOp.isDone)
            yield return null;

        currentLoadedRoom = chosen;
        preloadOp = null;

        yield return null;

        AlignRoom(chosen);

        roomReady = true;
        roomsCompleted++;

        Debug.Log($"Nueva sala lista: {chosen} | Total: {roomsCompleted}");
    }

    private void AlignRoom(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) return;

        // Buscar Room en la escena cargada
        Room room = null;
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            room = root.GetComponentInChildren<Room>();
            if (room != null) break;
        }

        if (room == null || room.GetEntryPoint() == null)
        {
            // Sin entry point, poner directo en corridorExit
            foreach (GameObject root in roots)
            {
                root.transform.position += corridorExit.position;
            }
            return;
        }

        // Alinear: el entryPoint de la sala debe coincidir con corridorExit
        Vector3 entryWorldPos = room.GetEntryPoint().position;
        Vector3 offset = corridorExit.position - entryWorldPos;

        foreach (GameObject root in roots)
        {
            root.transform.position += offset;
        }

        Debug.Log($"Sala alineada. Offset: {offset}");
    }

    private string PickRandomRoom()
    {
        if (roomScenes.Length == 0) return "";

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

    #endregion
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("SCENES")]
    [SerializeField] private string[] combatScenes;
    [SerializeField] private string[] corridorScenes;
    [SerializeField] private string[] shopScenes;

    [Header("FIRST ROOM SPAWN")]
    [SerializeField] private Transform firstRoomSpawn;

    [Header("SHOP FREQUENCY")]
    [SerializeField] private int shopEveryXRooms = 3;

    // Tracking de escenas cargadas
    private string currentRoomScene = "";
    private string currentCorridorScene = "";
    private string previousScene = "";

    // Referencias a los scripts de sala/pasillo activos
    private Room currentRoom;
    private Corridor currentCorridor;

    // Donde conectar la siguiente pieza
    private Transform nextConnectionPoint;

    private string lastRoom = "";
    private string lastCorridor = "";
    private int roomsCompleted;

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

        nextConnectionPoint = firstRoomSpawn;

        // Cargar primera sala
        StartCoroutine(LoadScene(PickNextRoom(), SceneType.Room));
    }

    #region EVENTS FROM TRIGGERS

    // Player cruza la puerta de entrada de la sala
    public void OnPlayerEnterRoom()
    {
        Debug.Log(">>> Player ENTRÓ a la sala");

        if (currentRoom != null)
            currentRoom.ActivateRoom();

        // Descargar el pasillo anterior (el player ya no lo ve)
        if (!string.IsNullOrEmpty(previousScene))
        {
            StartCoroutine(UnloadScene(previousScene));
            previousScene = "";
        }
    }

    // Player cruza la puerta de salida de la sala
    public void OnPlayerExitRoom()
    {
        Debug.Log(">>> Player SALIÓ de la sala");

        if (GameTimer.Instance != null)
            GameTimer.Instance.SetPaused(true);
    }

    // Player entra al pasillo
    public void OnPlayerEnterCorridor()
    {
        Debug.Log(">>> Player ENTRÓ al pasillo");

        // Descargar la sala anterior
        if (!string.IsNullOrEmpty(previousScene))
        {
            StartCoroutine(UnloadScene(previousScene));
            previousScene = "";
        }
    }

    // Player sale del pasillo (entra a la siguiente sala)
    public void OnPlayerExitCorridor()
    {
        Debug.Log(">>> Player SALIÓ del pasillo");
    }

    // Sala completada → empezar a cargar pasillo
    public void OnRoomCompleted()
    {
        Debug.Log(">>> Sala completada, cargando pasillo");

        roomsCompleted++;

        // El exitPoint de la sala es donde conecta el pasillo
        if (currentRoom != null)
            nextConnectionPoint = currentRoom.GetExitPoint();

        // Marcar sala actual como "anterior" (se descargará cuando entre al pasillo)
        previousScene = currentRoomScene;
        currentRoomScene = "";

        // Cargar pasillo
        StartCoroutine(LoadScene(PickNextCorridor(), SceneType.Corridor));
    }

    // Pasillo cargado → cargar siguiente sala al final
    private void OnCorridorLoaded()
    {
        Debug.Log(">>> Pasillo cargado, cargando siguiente sala");

        if (currentCorridor != null)
            nextConnectionPoint = currentCorridor.GetExitPoint();

        // Marcar pasillo como "anterior"
        previousScene = currentCorridorScene;
        currentCorridorScene = "";

        // Cargar sala
        string nextRoom = PickNextRoom();
        StartCoroutine(LoadScene(nextRoom, SceneType.Room));
    }

    #endregion

    #region SCENE LOADING

    private enum SceneType { Room, Corridor }

    private IEnumerator LoadScene(string sceneName, SceneType type)
    {
        Debug.Log($"Cargando {type}: {sceneName}");

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!load.isDone)
            yield return null;

        yield return null;

        // Encontrar y alinear
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) yield break;

        GameObject[] roots = scene.GetRootGameObjects();

        if (type == SceneType.Room)
        {
            currentRoomScene = sceneName;
            currentRoom = FindInRoots<Room>(roots);

            if (currentRoom != null && currentRoom.GetEntryPoint() != null)
            {
                AlignScene(roots, currentRoom.GetEntryPoint());
            }

            Debug.Log($"Sala lista: {sceneName}");
        }
        else
        {
            currentCorridorScene = sceneName;
            currentCorridor = FindInRoots<Corridor>(roots);

            if (currentCorridor != null && currentCorridor.GetEntryPoint() != null)
            {
                AlignScene(roots, currentCorridor.GetEntryPoint());
            }

            Debug.Log($"Pasillo listo: {sceneName}");

            // Pasillo cargado → cargar sala al final
            OnCorridorLoaded();
        }
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) yield break;

        Debug.Log($"Descargando: {sceneName}");

        AsyncOperation unload = SceneManager.UnloadSceneAsync(sceneName);
        if (unload != null)
        {
            while (!unload.isDone)
                yield return null;
        }
    }

    #endregion

    #region ALIGN

    private void AlignScene(GameObject[] roots, Transform entryPoint)
    {
        if (nextConnectionPoint == null) return;

        Vector3 offset = nextConnectionPoint.position - entryPoint.position;

        foreach (GameObject root in roots)
            root.transform.position += offset;

        Debug.Log($"Escena alineada. Offset: {offset}");
    }

    #endregion

    #region PICKING

    private string PickNextRoom()
    {
        // Toca tienda?
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
        if (scenes.Length == 0)
        {
            Debug.LogError("No hay escenas configuradas");
            return "";
        }

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

    #endregion

    #region HELPERS

    private T FindInRoots<T>(GameObject[] roots) where T : Component
    {
        foreach (GameObject root in roots)
        {
            T component = root.GetComponentInChildren<T>();
            if (component != null) return component;
        }
        return null;
    }

    public int GetRoomsCompleted() { return roomsCompleted; }

    #endregion
}
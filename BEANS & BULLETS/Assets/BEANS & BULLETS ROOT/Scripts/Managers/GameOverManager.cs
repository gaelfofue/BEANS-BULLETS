using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string mainSceneName;

    [Header("BSOD")]
    [SerializeField] private CanvasGroup bsodOverlay;
    [SerializeField] private TextMeshProUGUI bsodText;

    [Header("Timing")]
    [SerializeField] private float slowMoDuration = 1.5f;
    [SerializeField] private float slowMoScale = 0.2f;

    [Header("Camera Death Animation")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float fallAngle = 80f;
    [SerializeField] private float fallHeight = 1.5f;

    [Header("Player Reference")]
    [SerializeField] private GameObject player;

    private bool gameOverTriggered = false;
    private bool waitingForInput = false;

    private void Start()
    {
        if (bsodOverlay != null)
        {
            bsodOverlay.alpha = 0f;
            bsodOverlay.gameObject.SetActive(false);
            // Importante: No bloquear raycasts cuando está invisible
            bsodOverlay.blocksRaycasts = false;
            bsodOverlay.interactable = false;
        }
    }

    // SOLUCIÓN ROBUSTA: OnGUI funciona SIEMPRE, incluso con timeScale = 0
    private void OnGUI()
    {
        if (!waitingForInput) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown || e.type == EventType.MouseDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                QuitGame();
            }
            else if (e.type == EventType.KeyDown || e.type == EventType.MouseDown)
            {
                // Evitar que teclas de sistema reinicien por accidente
                if (e.keyCode != KeyCode.None || e.type == EventType.MouseDown)
                {
                    RestartGame();
                }
            }
        }
    }

    // Backup por si OnGUI no es de tu gusto (menos confiable con timeScale 0)
    private void Update()
    {
        if (!waitingForInput) return;

        // Método alternativo usando Unscaled time para delays si los necesitas
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        else if (Input.GetKeyDown(KeyCode.E) ||
                 Input.GetKeyDown(KeyCode.Space) ||
                 Input.GetKeyDown(KeyCode.Return) ||
                 Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        // Pausar el run timer
        HUDController hud = FindFirstObjectByType<HUDController>();
        if (hud != null)
            hud.PauseRunTimer();

        DisablePlayerControl();
        StartCoroutine(GameOverSequence());
    }

    private void DisablePlayerControl()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj;
        }

        if (player == null) return;

        var fpController = player.GetComponent<PlayerMovement>();
        if (fpController != null)
            fpController.enabled = false;

        var gunSystem = player.GetComponentInChildren<GunSystem>();
        if (gunSystem != null)
            gunSystem.enabled = false;

        var interact = player.GetComponent<PlayerInteract>();
        if (interact != null)
            interact.enabled = false;

        var shopInteract = player.GetComponent<ShopScreenInteraction>();
        if (shopInteract != null)
            shopInteract.enabled = false;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private IEnumerator GameOverSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = slowMoScale;

        if (cameraHolder == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraHolder = mainCam.transform.parent != null
                    ? mainCam.transform.parent
                    : mainCam.transform;
        }

        if (cameraHolder != null)
            yield return StartCoroutine(CameraFallAnimation());
        else
            yield return new WaitForSecondsRealtime(slowMoDuration);

        BuildBSODText();

        // Activar BSOD con configuración correcta
        bsodOverlay.gameObject.SetActive(true);
        bsodOverlay.alpha = 1f;
        // Permitir que los inputs pasen a través (para que OnGUI funcione)
        // o bloquearlos si tienes botones UI. Para input global, déjalo en false.
        bsodOverlay.blocksRaycasts = false;
        bsodOverlay.interactable = false;

        Time.timeScale = 0f;
        waitingForInput = true;

        Debug.Log("Game Over - Waiting for input...");
    }

    private IEnumerator CameraFallAnimation()
    {
        Vector3 startLocalPos = cameraHolder.localPosition;
        Quaternion startLocalRot = cameraHolder.localRotation;

        Vector3 endLocalPos = startLocalPos + new Vector3(0f, -fallHeight, 0f);
        Quaternion endLocalRot = startLocalRot * Quaternion.Euler(0f, 0f, fallAngle);

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            // Usar unscaledDeltaTime porque timeScale está en slowMo
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fallDuration;
            float easedT = t * t;

            cameraHolder.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, easedT);
            cameraHolder.localRotation = Quaternion.Slerp(startLocalRot, endLocalRot, easedT);

            yield return null;
        }

        cameraHolder.localPosition = endLocalPos;
        cameraHolder.localRotation = endLocalRot;
        yield return new WaitForSecondsRealtime(0.3f);
    }

    private void BuildBSODText()
    {
        HUDController hud = FindFirstObjectByType<HUDController>();
        int kills = hud != null ? hud.GetKillCount() : 0;
        float runTime = hud != null ? hud.GetRunTime() : 0f;
        int rooms = LevelManager.Instance != null
            ? LevelManager.Instance.GetRoomsCompleted() : 0;

        int minutes = Mathf.FloorToInt(runTime / 60f);
        int seconds = Mathf.FloorToInt(runTime % 60f);

        bsodText.text =
            "   Bean & Bullets OS v1.0\n" +
            "\n" +
            "   A fatal exception 0x0000DEAD has occurred at\n" +
            "   TIMER:NULL in BEAN.EXE\n" +
            "\n" +
            "   The current bean has been terminated.\n" +
            "\n" +
            "   * * * S T A T S * * *\n" +
            "\n" +
            "   KILLS .............. " + kills.ToString("D6") + "\n" +
            "   TIME ............... " + minutes.ToString("00") + ":" + seconds.ToString("00") + "\n" +
            "   ROOMS .............. " + rooms.ToString("D4") + "\n" +
            "\n" +
            "   * * * * * * * * * * *\n" +
            "\n" +
            "   Press E / SPACE / CLICK to reboot.\n" +
            "   Press ESC to shut down.\n" +
            "\n" +
            "   Press any key to continue _";
    }

    private void RestartGame()
    {
        if (!waitingForInput) return; // Evitar dobles llamadas

        waitingForInput = false;
        Debug.Log("Restarting game...");

        Time.timeScale = 1f;
        StartCoroutine(FullRestart());
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator FullRestart()
    {
        int sceneCount = SceneManager.sceneCount;
        for (int i = sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == mainSceneName) continue;
            if (scene == SceneManager.GetActiveScene()) continue;

            AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
            if (unload != null)
            {
                while (!unload.isDone)
                    yield return null;
            }
        }

        SceneManager.LoadScene(mainSceneName);
    }
}
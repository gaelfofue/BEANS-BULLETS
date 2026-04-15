using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Deja vacío para recargar la escena actual")]
    [SerializeField] private string mainSceneName = ""; // 🆕 Opcional

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
            bsodOverlay.blocksRaycasts = false;
            bsodOverlay.interactable = false;
        }

        // 🆕 Auto-detectar escena si no está asignada
        if (string.IsNullOrEmpty(mainSceneName))
        {
            mainSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[GAME OVER] Auto-detected scene: {mainSceneName}");
        }
    }

    private void OnGUI()
    {
        if (!waitingForInput) return;

        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                QuitGame();
            }
            else if (e.keyCode != KeyCode.None)
            {
                RestartGame();
            }
        }
        else if (e.type == EventType.MouseDown)
        {
            RestartGame();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

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

        bsodOverlay.gameObject.SetActive(true);
        bsodOverlay.alpha = 1f;
        bsodOverlay.blocksRaycasts = false;
        bsodOverlay.interactable = false;

        Time.timeScale = 0f;
        waitingForInput = true;

        Debug.Log("Game Over - Press any key to restart");
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
        if (!waitingForInput) return;

        waitingForInput = false;
        Debug.Log($"[RESTART] Reloading {mainSceneName}...");

        Time.timeScale = 1f;

        // 🆕 Método simple y directo
        SceneManager.LoadScene(mainSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("[QUIT] Exiting...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 🗑️ ELIMINADO: FullRestart con lógica innecesaria
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;
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
        bsodOverlay.alpha = 0f;
        bsodOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
            {
                RestartGame();
            }
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

        // Desactivar control del player
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

        // Desactivar el script de movimiento
        var fpController = player.GetComponent<PlayerMovement>();
        if (fpController != null)
            fpController.enabled = false;

        // Desactivar el disparo
        var gunSystem = player.GetComponentInChildren<GunSystem>();
        if (gunSystem != null)
            gunSystem.enabled = false;

        // Desactivar interacción
        var interact = player.GetComponent<PlayerInteract>();
        if (interact != null)
            interact.enabled = false;

        // Desactivar la interacción de tienda
        var shopInteract = player.GetComponent<ShopScreenInteraction>();
        if (shopInteract != null)
            shopInteract.enabled = false;

        // Congelar el rigidbody
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
        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // SlowMo + Caída de cámara simultáneos
        Time.timeScale = slowMoScale;

        // Encontrar la cámara si no está asignada
        if (cameraHolder == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraHolder = mainCam.transform.parent != null
                    ? mainCam.transform.parent
                    : mainCam.transform;
        }

        // Animación de caída
        if (cameraHolder != null)
            yield return StartCoroutine(CameraFallAnimation());
        else
            yield return new WaitForSecondsRealtime(slowMoDuration);

        // Preparar BSOD
        BuildBSODText();

        // Aparecer BSOD de golpe
        bsodOverlay.gameObject.SetActive(true);
        bsodOverlay.alpha = 1f;

        // Congelar completamente
        Time.timeScale = 0f;

        waitingForInput = true;
    }

    private IEnumerator CameraFallAnimation()
    {
        Vector3 startLocalPos = cameraHolder.localPosition;
        Quaternion startLocalRot = cameraHolder.localRotation;

        // Caer hacia la derecha y abajo
        Vector3 endLocalPos = startLocalPos + new Vector3(0f, -fallHeight, 0f);
        Quaternion endLocalRot = startLocalRot * Quaternion.Euler(0f, 0f, fallAngle);

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fallDuration;

            // Ease-in: empieza lento, acelera (como gravedad)
            float easedT = t * t;

            cameraHolder.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, easedT);
            cameraHolder.localRotation = Quaternion.Slerp(startLocalRot, endLocalRot, easedT);

            yield return null;
        }

        cameraHolder.localPosition = endLocalPos;
        cameraHolder.localRotation = endLocalRot;

        // Pequeña pausa en el suelo antes del BSOD
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
            "   Press any key to reboot.\n" +
            "   Press ESC to shut down.\n" +
            "\n" +
            "   Press any key to continue _";
    }

    private void RestartGame()
    {
        waitingForInput = false;
        gameOverTriggered = false;

        // Restaurar tiempo
        Time.timeScale = 1f;

        // Descargar TODAS las escenas aditivas antes de recargar
        StartCoroutine(FullRestart());
    }

    private IEnumerator FullRestart()
    {
        // Descargar todas las escenas aditivas
        int sceneCount = SceneManager.sceneCount;
        for (int i = sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // No descargar la escena principal
            if (scene.name == mainSceneName) continue;
            if (scene == SceneManager.GetActiveScene()) continue;

            AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
            if (unload != null)
            {
                while (!unload.isDone)
                    yield return null;
            }
        }

        // Recargar la escena principal limpia
        SceneManager.LoadScene(mainSceneName);
    }
}
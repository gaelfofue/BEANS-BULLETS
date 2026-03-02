using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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
    [SerializeField] private float fadeDuration = 0.5f;

    private bool gameOverTriggered = false;
    private bool waitingForInput = false;

    private void Start()
    {
        bsodOverlay.alpha = 0f;
        bsodOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (waitingForInput)
        {
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
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        HUDController hud = FindObjectOfType<HUDController>();
        if (hud != null) hud.PauseRunTimer();

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // SlowMo
        Time.timeScale = slowMoScale;
        float elapsed = 0f;
        while (elapsed < slowMoDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Preparar BSOD text antes de mostrar
        BuildBSODText();

        // Aparecer de golpe (como BSOD real)
        bsodOverlay.gameObject.SetActive(true);
        bsodOverlay.alpha = 1f;

        // Congelar
        Time.timeScale = 0f;

        waitingForInput = true;
    }

    private void BuildBSODText()
    {
        HUDController hud = FindObjectOfType<HUDController>();

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }
}
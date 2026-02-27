using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string mainSceneName = "MainScene";

    [Header("UI References")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI roomsText;

    [Header("Timing")]
    [SerializeField] private float slowMoDuration = 1.5f;
    [SerializeField] private float slowMoScale = 0.2f;
    [SerializeField] private float fadeDuration = 0.8f;

    // Stats
    private int totalKills = 0;
    private float totalTimeSurvived = 0f;

    // State
    private bool gameOverTriggered = false;
    private bool waitingForInput = false;

    private void Start()
    {
        fadeOverlay.alpha = 0f;
        fadeOverlay.gameObject.SetActive(false);
        statsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!gameOverTriggered)
        {
            totalTimeSurvived += Time.deltaTime;
        }

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

    public void RegisterKill()
    {
        totalKills++;
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = slowMoScale;
        float elapsed = 0f;
        while (elapsed < slowMoDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeOverlay.gameObject.SetActive(true);
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 1f;

        Time.timeScale = 0f;

        ShowStats();
    }

    private void ShowStats()
    {
        statsPanel.SetActive(true);

        killsText.text = "KILLS: " + totalKills;

        int minutes = Mathf.FloorToInt(totalTimeSurvived / 60f);
        int seconds = Mathf.FloorToInt(totalTimeSurvived % 60f);
        timeText.text = "TIME: " + minutes.ToString("00") + ":" + seconds.ToString("00");

        int rooms = LevelManager.Instance != null
            ? LevelManager.Instance.GetRoomsCompleted()
            : 0;
        roomsText.text = "ROOMS: " + rooms;

        waitingForInput = true;
    }

    private void RestartGame()
    {
        waitingForInput = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("References")]
    public Image timerBar;
    public TextMeshProUGUI timerText;  // NUEVO

    [Header("Colors")]
    public Color safeColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    [Header("Thresholds")]
    public float warningPercent = 0.5f;
    public float dangerPercent = 0.25f;

    [Header("Pulse Effect")]
    public float pulseSpeed = 4f;
    private bool isPulsing = false;

    public void UpdateTimer(float percent)
    {
        percent = Mathf.Clamp01(percent);

        RectTransform rt = timerBar.GetComponent<RectTransform>();
        rt.localScale = new Vector3(percent, 1, 1);

        // Actualizar texto
        if (timerText != null && GameTimer.Instance != null)
        {
            int seconds = Mathf.CeilToInt(GameTimer.Instance.GetCurrentTime());
            timerText.text = seconds.ToString();
        }

        if (percent > warningPercent)
        {
            timerBar.color = safeColor;
            if (timerText != null) timerText.color = safeColor;
            isPulsing = false;
        }
        else if (percent > dangerPercent)
        {
            timerBar.color = warningColor;
            if (timerText != null) timerText.color = warningColor;
            isPulsing = false;
        }
        else
        {
            timerBar.color = dangerColor;
            if (timerText != null) timerText.color = dangerColor;
            isPulsing = true;
        }
    }

    void Update()
    {
        if (isPulsing && timerBar != null)
        {
            float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            Color c = timerBar.color;
            c.a = alpha;
            timerBar.color = c;
        }
    }

    public void OnGameOver()
    {
        Debug.Log("SHOW GAME OVER SCREEN");
    }
}
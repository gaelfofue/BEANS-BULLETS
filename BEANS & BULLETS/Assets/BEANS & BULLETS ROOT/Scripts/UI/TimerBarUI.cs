using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TimerBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject barPanel;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Colors")]
    [SerializeField] private Color colorSafe = new Color(0f, 1f, 0.26f, 1f);
    [SerializeField] private Color colorWarning = new Color(1f, 0.84f, 0f, 1f);
    [SerializeField] private Color colorDanger = new Color(1f, 0f, 0.25f, 1f);

    [Header("Danger Blink")]
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private float dangerThreshold = 0.3f;

    [Header("Visibility")]
    [SerializeField] private bool hideWhenPaused = true;

    private bool isBlinking = false;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.onTimeChanged.AddListener(UpdateBar);
            GameTimer.Instance.onTimerStart.AddListener(OnTimerStart);
            GameTimer.Instance.onTimerStop.AddListener(OnTimerStop);
            GameTimer.Instance.onTimerDeath.AddListener(OnTimerDeath);
        }

        ResetBar();

        if (hideWhenPaused)
            barPanel?.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.onTimeChanged.RemoveListener(UpdateBar);
            GameTimer.Instance.onTimerStart.RemoveListener(OnTimerStart);
            GameTimer.Instance.onTimerStop.RemoveListener(OnTimerStop);
            GameTimer.Instance.onTimerDeath.RemoveListener(OnTimerDeath);
        }
    }

    void Update()
    {
        if (isBlinking && fillImage != null)
        {
            float blink = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            fillImage.color = Color.Lerp(colorDanger, Color.white, blink * 0.4f);
        }

        // Actualizar texto
        if (timerText != null && GameTimer.Instance != null)
        {
            int seconds = Mathf.CeilToInt(GameTimer.Instance.GetCurrentTime());
            timerText.text = seconds.ToString();
        }
    }

    private void OnTimerStart()
    {
        if (barPanel != null)
            barPanel.SetActive(true);
    }

    private void OnTimerStop()
    {
        if (hideWhenPaused && barPanel != null)
            barPanel.SetActive(false);

        isBlinking = false;
    }

    private void OnTimerDeath()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = colorDanger;
        }

        isBlinking = false;
    }

    private void UpdateBar(float ratio)
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.Clamp01(ratio);

        if (ratio > 0.6f)
        {
            fillImage.color = colorSafe;
            isBlinking = false;
        }
        else if (ratio > dangerThreshold)
        {
            fillImage.color = colorWarning;
            isBlinking = false;
        }
        else
        {
            isBlinking = true;
        }
    }

    public void ResetBar()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = colorSafe;
        }

        isBlinking = false;

        if (hideWhenPaused && barPanel != null)
            barPanel.SetActive(false);
    }
}
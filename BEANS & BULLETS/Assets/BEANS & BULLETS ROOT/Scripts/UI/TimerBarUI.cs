using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Awake()
    {
        SetupFillImage();
        ResetBarImmediate();

        if (hideWhenPaused && barPanel != null)
            barPanel.SetActive(false);
    }

    void Start()
    {
        SubscribeToTimer();
        SyncFromTimer();
    }

    void OnDestroy()
    {
        UnsubscribeFromTimer();
    }

    void Update()
    {
        if (isBlinking && fillImage != null)
        {
            float blink = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            fillImage.color = Color.Lerp(colorDanger, Color.white, blink * 0.4f);
        }

        if (timerText != null && GameTimer.Instance != null)
        {
            int seconds = Mathf.CeilToInt(GameTimer.Instance.GetCurrentTime());
            timerText.text = seconds.ToString();
        }
    }

    private void SetupFillImage()
    {
        if (fillImage == null) return;

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        // Esto evita que arranque visualmente negro
        fillImage.fillAmount = 1f;
        fillImage.color = colorSafe;
    }

    private void SubscribeToTimer()
    {
        if (GameTimer.Instance == null) return;

        GameTimer.Instance.onTimeChanged.AddListener(UpdateBar);
        GameTimer.Instance.onTimerStart.AddListener(OnTimerStart);
        GameTimer.Instance.onTimerStop.AddListener(OnTimerStop);
        GameTimer.Instance.onTimerDeath.AddListener(OnTimerDeath);
    }

    private void UnsubscribeFromTimer()
    {
        if (GameTimer.Instance == null) return;

        GameTimer.Instance.onTimeChanged.RemoveListener(UpdateBar);
        GameTimer.Instance.onTimerStart.RemoveListener(OnTimerStart);
        GameTimer.Instance.onTimerStop.RemoveListener(OnTimerStop);
        GameTimer.Instance.onTimerDeath.RemoveListener(OnTimerDeath);
    }

    private void SyncFromTimer()
    {
        if (GameTimer.Instance == null)
        {
            ResetBarImmediate();
            return;
        }

        float ratio = GameTimer.Instance.GetTimePercent();
        UpdateBar(ratio);

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(GameTimer.Instance.GetCurrentTime());
            timerText.text = seconds.ToString();
        }

        if (barPanel != null)
        {
            bool shouldShow = !hideWhenPaused || GameTimer.Instance.IsRunning();
            barPanel.SetActive(shouldShow);
        }

        if (GameTimer.Instance.IsDead())
            OnTimerDeath();
    }

    private void OnTimerStart()
    {
        if (barPanel != null)
            barPanel.SetActive(true);

        SyncFromTimer();
    }

    private void OnTimerStop()
    {
        if (hideWhenPaused && barPanel != null)
            barPanel.SetActive(false);

        isBlinking = false;
        SyncFromTimer();
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

        ratio = Mathf.Clamp01(ratio);
        fillImage.fillAmount = ratio;

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
            fillImage.color = colorDanger;
            isBlinking = true;
        }
    }

    private void ResetBarImmediate()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = colorSafe;
        }

        if (timerText != null && GameTimer.Instance != null)
        {
            timerText.text = Mathf.CeilToInt(GameTimer.Instance.GetCurrentTime()).ToString();
        }

        isBlinking = false;
    }
    public void ResetBar() //Perezon cambiar todo el script nuevamente solo porque el script de muerte ya no contecta con esto
    {
        ResetBarImmediate();
    }
}
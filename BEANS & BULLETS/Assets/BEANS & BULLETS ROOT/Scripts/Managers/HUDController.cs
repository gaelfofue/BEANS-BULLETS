using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerDigitsText;

    [Header("Timer Bar")]
    [SerializeField] private RectTransform timerBarFill;
    [SerializeField] private Image timerBarImage;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Room Progress")]
    [SerializeField] private TextMeshProUGUI roomProgressText;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Upgrade Slots")]
    [SerializeField] private Image slotIcon1;
    [SerializeField] private Image slotIcon2;
    [SerializeField] private Image slotIcon3;
    [SerializeField] private TextMeshProUGUI slotLabel1;
    [SerializeField] private TextMeshProUGUI slotLabel2;
    [SerializeField] private TextMeshProUGUI slotLabel3;

    [Header("Timer Colors")]
    [SerializeField] private Color colorSafe = new Color(0f, 1f, 0.255f);
    [SerializeField] private Color colorWarning = new Color(1f, 0.843f, 0f);
    [SerializeField] private Color colorDanger = new Color(1f, 0f, 0.251f);

    [Header("Settings")]
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private int roomsPerCycle = 3;

    private int killCount = 0;

    private void Update()
    {
        UpdateTimerDisplay();
        UpdateTimerBar();
        UpdateRoomProgress();
    }

    private void UpdateTimerDisplay()
    {
        if (GameTimer.Instance == null) return;

        float current = GameTimer.Instance.GetCurrentTime();
        float ratio = GameTimer.Instance.GetTimePercent();

        int totalSeconds = Mathf.FloorToInt(current);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int ms = Mathf.FloorToInt((current - totalSeconds) * 100f);

        timerDigitsText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, ms);
        timerDigitsText.color = GetTimerColor(ratio);
    }

    private void UpdateTimerBar()
    {
        if (GameTimer.Instance == null) return;

        float ratio = GameTimer.Instance.GetTimePercent();

        if (timerBarFill != null)
        {
            Vector3 scale = timerBarFill.localScale;
            scale.x = Mathf.Clamp01(ratio);
            timerBarFill.localScale = scale;
        }

        if (timerBarImage != null)
            timerBarImage.color = GetTimerColor(ratio);
    }

    private Color GetTimerColor(float ratio)
    {
        if (ratio > 0.6f)
            return colorSafe;
        else if (ratio > 0.3f)
            return colorWarning;
        else
        {
            float blink = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            return Color.Lerp(colorDanger, Color.white, blink * 0.3f);
        }
    }

    private void UpdateRoomProgress()
    {
        if (LevelManager.Instance == null) return;

        int totalRooms = LevelManager.Instance.GetRoomsCompleted();
        int inCycle = totalRooms % roomsPerCycle;
        int round = (totalRooms / roomsPerCycle) + 1;

        if (roomProgressText != null)
            roomProgressText.text = inCycle + " / " + roomsPerCycle;
        if (roundText != null)
            roundText.text = "ROUND " + round;
    }

    public void RegisterKill()
    {
        killCount++;
        if (scoreText != null)
            scoreText.text = killCount.ToString("D8");
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = current + " / " + max;
    }

    public void SetUpgrade(int slot, Sprite icon, string label)
    {
        switch (slot)
        {
            case 0:
                if (slotIcon1 != null) { slotIcon1.sprite = icon; slotIcon1.color = Color.white; }
                if (slotLabel1 != null) slotLabel1.text = label;
                break;
            case 1:
                if (slotIcon2 != null) { slotIcon2.sprite = icon; slotIcon2.color = Color.white; }
                if (slotLabel2 != null) slotLabel2.text = label;
                break;
            case 2:
                if (slotIcon3 != null) { slotIcon3.sprite = icon; slotIcon3.color = Color.white; }
                if (slotLabel3 != null) slotLabel3.text = label;
                break;
        }
    }

    public void ClearUpgrades()
    {
        Color empty = new Color(1f, 1f, 1f, 0.15f);
        if (slotIcon1 != null) { slotIcon1.sprite = null; slotIcon1.color = empty; }
        if (slotIcon2 != null) { slotIcon2.sprite = null; slotIcon2.color = empty; }
        if (slotIcon3 != null) { slotIcon3.sprite = null; slotIcon3.color = empty; }
        if (slotLabel1 != null) slotLabel1.text = "FIRE MODE";
        if (slotLabel2 != null) slotLabel2.text = "BULLET TYPE";
        if (slotLabel3 != null) slotLabel3.text = "MODIFIER";
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Run Timer")]
    [SerializeField] private TextMeshProUGUI runTimerText;

    [Header("Combat Timer Bar")]
    [SerializeField] private GameObject combatTimerPanel;
    [SerializeField] private RectTransform combatTimerFill;
    [SerializeField] private Image combatTimerImage;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Progress")]
    [SerializeField] private TextMeshProUGUI roomProgressText;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Upgrade Slots")]
    [SerializeField] private Image slotIcon1;
    [SerializeField] private Image slotIcon2;
    [SerializeField] private Image slotIcon3;

    [Header("Combat Timer Colors")]
    [SerializeField] private Color colorSafe = new Color(0f, 1f, 0.255f);
    [SerializeField] private Color colorWarning = new Color(1f, 0.843f, 0f);
    [SerializeField] private Color colorDanger = new Color(1f, 0f, 0.251f);

    [Header("Settings")]
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private int roomsPerCycle = 3;

    // Interno
    private int killCount = 0;
    private float runTimer = 0f;
    private bool runTimerPaused = false;

    // 🆕 Cache para evitar llamadas constantes
    private float lastTimerRatio = 1f;

    private void Update()
    {
        UpdateRunTimer();
        UpdateCombatTimer();
        UpdateRoomProgress();
    }

    // ==================
    // RUN TIMER (tiempo total de partida)
    // ==================

    private void UpdateRunTimer()
    {
        if (runTimerPaused) return;

        runTimer += Time.deltaTime;

        int totalSeconds = Mathf.FloorToInt(runTimer);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int ms = Mathf.FloorToInt((runTimer - totalSeconds) * 100f);

        runTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, ms);
    }

    public void PauseRunTimer()
    {
        runTimerPaused = true;
    }

    public float GetRunTime()
    {
        return runTimer;
    }

    // ==================
    // COMBAT TIMER (barra inferior, solo en combate)
    // ==================

    private void UpdateCombatTimer()
    {
        if (GameTimer.Instance == null)
        {
            if (combatTimerPanel != null)
                combatTimerPanel.SetActive(false);
            return;
        }

        bool isRunning = GameTimer.Instance.IsRunning();
        bool isDead = GameTimer.Instance.IsDead();

        // Mostrar solo si está corriendo
        if (combatTimerPanel != null)
            combatTimerPanel.SetActive(isRunning);

        if (!isRunning || isDead) return;

        float ratio = GameTimer.Instance.GetTimePercent();

        // 🆕 MÉTODO PARA IMÁGENES: Cambiar ancho del RectTransform
        if (combatTimerFill != null)
        {
            // Opción A: Si la barra está anclada a la izquierda
            RectTransform rt = combatTimerFill;
            float maxWidth = 200f; // ⚠️ AJUSTA ESTO AL ANCHO MÁXIMO DE TU BARRA
            rt.sizeDelta = new Vector2(maxWidth * ratio, rt.sizeDelta.y);

            // Opción B: Si usas Scale (menos recomendado pero funciona)
            // Vector3 scale = combatTimerFill.localScale;
            // scale.x = Mathf.Clamp01(ratio);
            // combatTimerFill.localScale = scale;
        }

        // Color (esto sí funciona con Image)
        if (combatTimerImage != null)
            combatTimerImage.color = GetCombatColor(ratio);
    }

    private Color GetCombatColor(float ratio)
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

    // ==================
    // SCORE
    // ==================

    public void RegisterKill()
    {
        killCount++;
        if (scoreText != null)
            scoreText.text = killCount.ToString("D8");
    }

    public int GetKillCount()
    {
        return killCount;
    }

    // ==================
    // ROOM PROGRESS
    // ==================

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

    // ==================
    // AMMO
    // ==================

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = current + " / " + max;
    }

    // ==================
    // UPGRADES
    // ==================

    public void SetUpgrade(int slot, Sprite icon)
    {
        Image target = null;
        switch (slot)
        {
            case 0: target = slotIcon1; break;
            case 1: target = slotIcon2; break;
            case 2: target = slotIcon3; break;
        }

        if (target != null)
        {
            target.sprite = icon;
            target.color = Color.white;
        }
    }

    public void ClearUpgrades()
    {
        Color empty = new Color(1f, 1f, 1f, 0.15f);
        if (slotIcon1 != null) { slotIcon1.sprite = null; slotIcon1.color = empty; }
        if (slotIcon2 != null) { slotIcon2.sprite = null; slotIcon2.color = empty; }
        if (slotIcon3 != null) { slotIcon3.sprite = null; slotIcon3.color = empty; }
    }
}
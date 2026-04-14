using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Run Timer")]
    [SerializeField] private TextMeshProUGUI runTimerText;

    [Header("Combat Timer Bar")]
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
    [SerializeField] private Color colorPaused = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Header("Settings")]
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private int roomsPerCycle = 3;

    // Interno
    private int killCount = 0;
    private float runTimer = 0f;
    private bool runTimerPaused = false;
    private bool wasDead = false;

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

        runTimer += Time.unscaledDeltaTime;

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

    public int GetKillCount()
    {
        return killCount;
    }

    // ==================
    // COMBAT TIMER (barra inferior, solo en combate)
    // ==================

    private void UpdateCombatTimer()
    {
        if (GameTimer.Instance == null) return;

        bool isDead = GameTimer.Instance.IsDead();
        bool inCombat = GameTimer.Instance.IsRunning() && !isDead;

        // IMPORTANTE: Siempre actualizar el ratio para evitar el "salto"
        float ratio = GameTimer.Instance.GetTimePercent();

        // Escalar barra SIEMPRE (nunca desactivar el objeto)
        if (combatTimerFill != null)
        {
            Vector3 scale = combatTimerFill.localScale;
            scale.x = Mathf.Clamp01(ratio);
            combatTimerFill.localScale = scale;
        }

        // Color según estado
        if (combatTimerImage != null)
        {
            combatTimerImage.color = GetCombatColor(ratio, inCombat, isDead);
        }

        // Detectar muerte para congelar
        if (isDead && !wasDead)
        {
            runTimerPaused = true;
        }
        wasDead = isDead;
    }

    private Color GetCombatColor(float ratio, bool inCombat, bool isDead)
    {
        if (isDead) return colorDanger;
        if (!inCombat) return colorPaused; // Gris en tienda/pasillo

        if (ratio > 0.6f)
            return colorSafe;
        else if (ratio > 0.3f)
            return colorWarning;
        else
        {
            // Peligro con parpadeo (usa unscaledTime para que funcione con timeScale 0)
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
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Run Timer")]
    [SerializeField] private TextMeshProUGUI runTimerText;

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

    [Header("Settings")]
    [SerializeField] private int roomsPerCycle = 3;

    private int killCount = 0;
    private float runTimer = 0f;
    private bool runTimerPaused = false;

    private void Update()
    {
        UpdateRunTimer();
        UpdateRoomProgress();
    }

    private void UpdateRunTimer()
    {
        if (runTimerPaused) return;

        runTimer += Time.deltaTime;

        int totalSeconds = Mathf.FloorToInt(runTimer);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int ms = Mathf.FloorToInt((runTimer - totalSeconds) * 100f);

        if (runTimerText != null)
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

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = current + " / " + max;
    }

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
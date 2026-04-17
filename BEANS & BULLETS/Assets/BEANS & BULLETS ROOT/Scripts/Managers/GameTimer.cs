using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxTime = 30f;
    public float startingTime = 30f;

    [Header("Time Rewards")]
    public float timePerKill = 3f;
    public float timePerHit = 0.5f;

    [Header("Drain")]
    public float drainSpeed = 1f;

    [Header("State")]
    [SerializeField] private float currentTime;
    private bool isRunning = false;
    private bool isDead = false;

    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;
    public UnityEvent<float> onTimerChanged;

    public static GameTimer Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentTime = startingTime;
        // ✅ NO LLAMAR StartTimer() aquí
        // El timer empieza pausado y se activa al entrar a la primera sala de combate
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning || isDead) return;

        currentTime -= drainSpeed * Time.deltaTime;
        onTimerChanged?.Invoke(currentTime / maxTime);

        if (currentTime <= 0)
        {
            currentTime = 0;
            isDead = true;
            onTimerEnd?.Invoke();

            GameOverManager gom = FindFirstObjectByType<GameOverManager>();
            if (gom != null)
                gom.TriggerGameOver();
            Debug.Log($"[TIMER] Time: {currentTime:F2} / {maxTime:F2} | Running: {isRunning} | Percent: {GetTimePercent():F2}");
        }
    }

    public void StartTimer()
    {
        isRunning = true;
        isDead = false;
        onTimerStart?.Invoke();
    }

    public void PauseTimer() { isRunning = false; }

    public void ResumeTimer()
    {
        if (isDead) return;
        isRunning = true;
    }

    public void SetPaused(bool paused)
    {
        if (isDead) return;
        isRunning = !paused;
    }

    public void AddKillTime()
    {
        if (isDead) return;

        float previousTime = currentTime;
        currentTime += timePerKill;
        currentTime = Mathf.Min(currentTime, maxTime);

        Debug.Log($"[TIMER] AddKillTime: {previousTime:F1}s → {currentTime:F1}s (+{timePerKill}s)");
    }

    public void AddHitTime()
    {
        if (isDead) return;
        currentTime += timePerHit;
        currentTime = Mathf.Min(currentTime, maxTime);
    }

    public void RemoveTime(float amount)
    {
        if (isDead) return;
        currentTime -= amount;
        if (currentTime < 0f) currentTime = 0f;
    }

    public void AddCustomTime(float amount)
    {
        if (isDead) return;
        currentTime += amount;
        currentTime = Mathf.Min(currentTime, maxTime);
    }

    public void SetTimePerKill(float value) { timePerKill = value; }
    public void SetTimePerHit(float value) { timePerHit = value; }
    public void SetDrainSpeed(float value) { drainSpeed = value; }
    public void SetMaxTime(float value) { maxTime = value; }

    public float GetTimePercent() { return currentTime / maxTime; }
    public float GetCurrentTime() { return currentTime; }
    public bool IsDead() { return isDead; }
    public bool IsRunning() { return isRunning; }
}
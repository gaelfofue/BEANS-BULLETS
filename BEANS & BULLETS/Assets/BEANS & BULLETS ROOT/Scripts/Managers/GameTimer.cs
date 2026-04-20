using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Timer Configuration")]
    [SerializeField] private float maxTime = 30f;
    [SerializeField] private float drainSpeed = 1f;

    [Header("Time Rewards")]
    [SerializeField] private float timePerKill = 3f;
    [SerializeField] private float timePerHit = 0.5f;

    [Header("State (Read Only)")]
    [SerializeField] private float currentTime;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool isDead = false;

    [Header("Events")]
    public UnityEvent onTimerStart;
    public UnityEvent onTimerStop;
    public UnityEvent onTimerDeath;
    public UnityEvent<float> onTimeChanged; // Envía ratio 0-1

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!isRunning || isDead) return;

        currentTime -= drainSpeed * Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;

        float ratio = GetTimePercent();
        onTimeChanged?.Invoke(ratio);

        if (currentTime <= 0f && !isDead)
        {
            Die();
        }
    }

    public void StartTimer()
    {
        if (isDead) return;
        isRunning = true;
        onTimerStart?.Invoke();
        Debug.Log($"[TIMER] STARTED | Current: {currentTime:F1}s");
    }

    public void StopTimer()
    {
        isRunning = false;
        onTimerStop?.Invoke();
        Debug.Log($"[TIMER] STOPPED | Frozen at: {currentTime:F1}s");
    }

    public void ResetTimer()
    {
        currentTime = maxTime;
        isDead = false;
        isRunning = false;
        onTimeChanged?.Invoke(1f);
        Debug.Log($"[TIMER] RESET | Time: {currentTime:F1}s");
    }

    public void AddKillTime()
    {
        if (isDead) return;
        float before = currentTime;
        currentTime = Mathf.Min(currentTime + timePerKill, maxTime);
        Debug.Log($"[TIMER] KILL +{timePerKill}s | {before:F1}s → {currentTime:F1}s");
        onTimeChanged?.Invoke(GetTimePercent());
    }

    public void AddHitTime()
    {
        if (isDead) return;
        currentTime = Mathf.Min(currentTime + timePerHit, maxTime);
        onTimeChanged?.Invoke(GetTimePercent());
    }

    public void AddCustomTime(float amount)
    {
        if (isDead) return;
        currentTime = Mathf.Min(currentTime + amount, maxTime);
        onTimeChanged?.Invoke(GetTimePercent());
    }

    public void RemoveTime(float amount)
    {
        if (isDead) return;
        currentTime = Mathf.Max(currentTime - amount, 0f);
        onTimeChanged?.Invoke(GetTimePercent());
    }

    public void SetMaxTime(float newMax)
    {
        float ratio = GetTimePercent();
        maxTime = newMax;
        currentTime = ratio * maxTime;
        Debug.Log($"[TIMER] MaxTime changed to {maxTime}s | Current: {currentTime:F1}s");
        onTimeChanged?.Invoke(ratio);
    }

    public void SetDrainSpeed(float speed) => drainSpeed = speed;
    public void SetTimePerKill(float time) => timePerKill = time;
    public void SetTimePerHit(float time) => timePerHit = time;

    public float GetCurrentTime() => currentTime;
    public float GetMaxTime() => maxTime;
    public float GetTimePercent() => maxTime > 0 ? currentTime / maxTime : 0f;
    public bool IsRunning() => isRunning;
    public bool IsDead() => isDead;

    private void Die()
    {
        isDead = true;
        isRunning = false;
        currentTime = 0f;
        onTimerDeath?.Invoke();
        Debug.Log("[TIMER] DEATH");

        GameOverManager gom = FindFirstObjectByType<GameOverManager>();
        if (gom != null)
            gom.TriggerGameOver();
    }
}
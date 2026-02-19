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

    // Singleton
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
        StartTimer();
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
            Debug.Log("GAME OVER!");
        }
    }

    #region CONTROL

    public void StartTimer()
    {
        isRunning = true;
        isDead = false;
        onTimerStart?.Invoke();
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    #endregion

    #region ADD TIME

    public void AddKillTime()
    {
        if (isDead) return;
        currentTime += timePerKill;
        currentTime = Mathf.Min(currentTime, maxTime);
    }

    public void AddHitTime()
    {
        if (isDead) return;
        currentTime += timePerHit;
        currentTime = Mathf.Min(currentTime, maxTime);
    }

    public void AddCustomTime(float amount)
    {
        if (isDead) return;
        currentTime += amount;
        currentTime = Mathf.Min(currentTime, maxTime);
    }

    #endregion

    #region MUTACIONES

    public void SetTimePerKill(float value) { timePerKill = value; }
    public void SetTimePerHit(float value) { timePerHit = value; }
    public void SetDrainSpeed(float value) { drainSpeed = value; }
    public void SetMaxTime(float value) { maxTime = value; }

    #endregion

    #region GETTERS

    public float GetTimePercent() { return currentTime / maxTime; }
    public float GetCurrentTime() { return currentTime; }
    public bool IsDead() { return isDead; }
    public bool IsRunning() { return isRunning; }

    #endregion
}
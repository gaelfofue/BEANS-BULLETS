using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Header("TRIGGER CONFIG")]
    [SerializeField] private TriggerType triggerType;

    private bool triggered;

    private enum TriggerType
    {
        EnterCorridor,
        ExitCorridor,
        ExitRoom
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        switch (triggerType)
        {
            case TriggerType.EnterCorridor:
                LevelManager.Instance.OnPlayerEnterCorridor();
                break;

            case TriggerType.ExitCorridor:
                LevelManager.Instance.OnPlayerExitCorridor();
                break;

            case TriggerType.ExitRoom:
                LevelManager.Instance.OnPlayerExitRoom();
                break;
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}
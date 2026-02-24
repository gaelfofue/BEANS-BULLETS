using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Header("TRIGGER CONFIG")]
    [SerializeField] private TriggerType triggerType;

    private bool triggered;

    private enum TriggerType
    {
        ExitRoom,
        CorridorEnd
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        switch (triggerType)
        {
            case TriggerType.ExitRoom:
                LevelManager.Instance.OnPlayerExitRoom();
                break;

            case TriggerType.CorridorEnd:
                LevelManager.Instance.OnPlayerReachCorridorEnd();
                break;
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}
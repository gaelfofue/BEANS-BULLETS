using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [Header("EXIT CONFIG")]
    [SerializeField] private ExitAction action;

    private bool triggered;

    private enum ExitAction
    {
        ActivateRoom,
        TeleportToCorridor
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        switch (action)
        {
            case ExitAction.ActivateRoom:
                SceneTransition.Instance.ActivatePreloadedRoom();
                break;

            case ExitAction.TeleportToCorridor:
                TeleportToCorridor(other.gameObject);
                SceneTransition.Instance.PreloadNextRoom();
                break;
        }
    }

    private void TeleportToCorridor(GameObject player)
    {
        GameObject entry = GameObject.Find("CorridorEntry");

        if (entry == null)
        {
            Debug.LogError("No se encontró CorridorEntry");
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = entry.transform.position;

        // Resetear el pasillo para que funcione de nuevo
        CorridorManager corridor = FindObjectOfType<CorridorManager>();
        if (corridor != null)
        {
            corridor.ResetCorridor();
        }
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}
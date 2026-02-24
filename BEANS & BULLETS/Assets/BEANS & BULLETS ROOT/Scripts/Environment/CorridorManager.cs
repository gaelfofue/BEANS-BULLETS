// CorridorManager.cs

using UnityEngine;

public class CorridorManager : MonoBehaviour
{
    [Header("CORRIDOR")]
    [SerializeField] private Door entryDoor;
    [SerializeField] private Door exitDoor;
    [SerializeField] private ExitTrigger exitTrigger;

    private bool playerInCorridor;

    private void Start()
    {
        // Al inicio la puerta de entrada está abierta para que el player salga
        if (entryDoor != null) entryDoor.SetLocked(false);

        // La de salida depende de si la sala está lista
        CheckExitDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerEnteredCorridor();
    }

    private void PlayerEnteredCorridor()
    {
        if (playerInCorridor) return;
        playerInCorridor = true;

        Debug.Log("Player entró al pasillo");

        if (entryDoor != null)
        {
            entryDoor.ForceClose();
            entryDoor.SetLocked(true);
        }

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.SetPaused(true);
        }

        CheckExitDoor();
    }

    private void Update()
    {
        if (!playerInCorridor) return;

        CheckExitDoor();
    }

    private void CheckExitDoor()
    {
        if (exitDoor == null) return;
        if (!exitDoor.IsLocked()) return;

        if (SceneTransition.Instance != null && SceneTransition.Instance.IsRoomReady())
        {
            exitDoor.SetLocked(false);
            Debug.Log("Sala lista, puerta de salida desbloqueada");
        }
    }

    public void ResetCorridor()
    {
        playerInCorridor = false;

        if (entryDoor != null) entryDoor.SetLocked(false);
        if (exitDoor != null) exitDoor.SetLocked(true);
        if (exitTrigger != null) exitTrigger.ResetTrigger();

        Debug.Log("Pasillo reseteado");
    }
}
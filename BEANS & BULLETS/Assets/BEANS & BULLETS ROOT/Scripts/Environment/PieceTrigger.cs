using UnityEngine;

public class PieceTrigger : MonoBehaviour
{
    private RoomPiece myPiece;
    private bool triggered;

    private void Start()
    {
        // Buscar el RoomPiece en el padre
        myPiece = GetComponentInParent<RoomPiece>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (myPiece != null)
        {
            Debug.Log($"Player entró a: {myPiece.gameObject.name}");
            myPiece.Activate();
            LevelManager.Instance.OnPlayerEnteredPiece(myPiece);
        }
    }
}
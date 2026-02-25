// PieceTrigger.cs

using UnityEngine;

public class PieceTrigger : MonoBehaviour
{
    private RoomPiece myPiece;
    private bool triggered;

    private void Start()
    {
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

            // Si es una sala y la pieza anterior era un pasillo, avisar
            if (myPiece.GetPieceType() != RoomPiece.PieceType.Corridor)
            {
                LevelManager.Instance.OnPlayerExitedCorridor();
            }

            myPiece.Activate();
            LevelManager.Instance.OnPlayerEnteredPiece(myPiece);
        }
    }
}
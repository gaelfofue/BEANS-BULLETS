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
            LevelManager.Instance.OnPlayerEnteredPiece(myPiece);
        }
    }
}
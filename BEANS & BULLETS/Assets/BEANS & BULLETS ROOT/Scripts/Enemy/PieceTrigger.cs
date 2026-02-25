// PieceTrigger.cs

using UnityEngine;

public class PieceTrigger : MonoBehaviour
{
    private RoomPiece myPiece;
    private bool triggered;

    private void Start()
    {
        myPiece = GetComponentInParent<RoomPiece>();
        Debug.Log($"PieceTrigger Start | myPiece: {(myPiece != null ? myPiece.gameObject.name : "NULL")}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"PieceTrigger tocado por: {other.gameObject.name} | Tag: {other.tag}");

        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (myPiece != null)
        {
            Debug.Log($"Activando pieza: {myPiece.gameObject.name}");
            myPiece.Activate();
            LevelManager.Instance.OnPlayerEnteredPiece(myPiece);
        }
        else
        {
            Debug.Log("ERROR: myPiece es NULL");
        }
    }
}
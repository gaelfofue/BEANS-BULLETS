using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public Transform player;
    public Transform orientation;

    void LateUpdate()
    {
        // Copiar posición del player
        transform.position = player.position;

        // Copiar rotación horizontal de orientation
        transform.rotation = Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);
    }
}
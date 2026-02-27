using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public Transform player;
    public Transform orientation;

    void LateUpdate()
    {
        transform.position = player.position;
        transform.rotation = Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);
    }
}
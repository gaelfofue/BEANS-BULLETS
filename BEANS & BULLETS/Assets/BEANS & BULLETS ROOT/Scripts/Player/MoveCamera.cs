using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.position = player.position;
    }
}
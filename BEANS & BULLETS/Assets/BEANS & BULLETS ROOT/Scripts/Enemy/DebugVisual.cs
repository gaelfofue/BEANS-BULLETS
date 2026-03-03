using UnityEngine;

public class EnemyDebugVisual : MonoBehaviour
{
    void Update()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        // Línea ROJA del enemy al player
        Debug.DrawLine(transform.position, player.position, Color.red);

        // Línea VERDE hacia abajo (ground check)
        Debug.DrawRay(transform.position, Vector3.down * 5f, Color.green);

        // Línea AZUL: dirección forward del enemy
        Debug.DrawRay(transform.position, transform.forward * 3f, Color.blue);
    }

    void OnGUI()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        CharacterController cc = GetComponent<CharacterController>();

        string info = $"ENEMY POS: {transform.position}\n";

        if (player != null)
            info += $"PLAYER POS: {player.position}\n";

        if (cc != null)
            info += $"CC GROUNDED: {cc.isGrounded}\n";

        info += $"ENEMY Y: {transform.position.y:F2}\n";

        if (player != null)
            info += $"PLAYER Y: {player.position.y:F2}\n";

        // Raycast down from enemy
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 20f))
            info += $"GROUND BELOW ENEMY: {hit.point.y:F2} ({hit.collider.name})\n";
        else
            info += "NO GROUND BELOW ENEMY\n";

        // Raycast down from player
        if (player != null)
        {
            if (Physics.Raycast(player.position, Vector3.down, out hit, 20f))
                info += $"GROUND BELOW PLAYER: {hit.point.y:F2} ({hit.collider.name})\n";
            else
                info += "NO GROUND BELOW PLAYER\n";
        }

        GUI.Label(new Rect(10, 10, 500, 200), info);
    }
}
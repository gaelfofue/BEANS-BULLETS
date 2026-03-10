using UnityEngine;

public class ShootDebugLine : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float lineDuration = 0.5f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color missColor = Color.yellow;

    /// <summary>
    /// Llamar después de cada disparo para dibujar la línea.
    /// </summary>
    public void DrawShot(Vector3 origin, Vector3 direction, float range, bool didHit, Vector3 hitPoint)
    {
        if (didHit)
        {
            // Línea hasta el punto de impacto
            Debug.DrawLine(origin, hitPoint, hitColor, lineDuration);
        }
        else
        {
            // Línea hasta el rango máximo
            Debug.DrawLine(origin, origin + direction * range, missColor, lineDuration);
        }
    }
}
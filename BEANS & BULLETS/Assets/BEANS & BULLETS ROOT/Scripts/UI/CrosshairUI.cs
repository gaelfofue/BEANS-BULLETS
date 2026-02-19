using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair")]
    public Image crosshair;
    public float normalSize = 20f;
    public float shootSize = 30f;      // Se agranda al disparar
    public float shrinkSpeed = 8f;

    [Header("Hitmarker")]
    public Image hitmarker;
    public float hitmarkerDuration = 0.1f;
    private float hitmarkerTimer = 0f;

    // Private
    private RectTransform crosshairRect;

    void Start()
    {
        crosshairRect = crosshair.GetComponent<RectTransform>();
        crosshairRect.sizeDelta = new Vector2(normalSize, normalSize);

        if (hitmarker != null)
            hitmarker.enabled = false;
    }

    void Update()
    {
        // Crosshair vuelve a tamaño normal
        Vector2 current = crosshairRect.sizeDelta;
        Vector2 target = new Vector2(normalSize, normalSize);
        crosshairRect.sizeDelta = Vector2.Lerp(current, target, Time.deltaTime * shrinkSpeed);

        // Hitmarker timer
        if (hitmarkerTimer > 0)
        {
            hitmarkerTimer -= Time.deltaTime;
            if (hitmarkerTimer <= 0 && hitmarker != null)
            {
                hitmarker.enabled = false;
            }
        }
    }

    // Llamar al disparar
    public void OnShoot()
    {
        crosshairRect.sizeDelta = new Vector2(shootSize, shootSize);
    }

    // Llamar al acertar a un enemigo
    public void OnHit()
    {
        if (hitmarker == null) return;

        hitmarker.enabled = true;
        hitmarkerTimer = hitmarkerDuration;
    }
}
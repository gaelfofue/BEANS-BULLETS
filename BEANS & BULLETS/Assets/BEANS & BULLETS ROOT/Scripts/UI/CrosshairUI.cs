using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair")]
    public RawImage crosshair;        // Cambiado a RawImage
    public float normalSize = 20f;
    public float shootSize = 30f;
    public float shrinkSpeed = 8f;

    [Header("Hitmarker")]
    public RawImage hitmarker;         // Cambiado a RawImage
    public float hitmarkerDuration = 0.15f;
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
        crosshairRect.sizeDelta = Vector2.Lerp(
            current, target, Time.deltaTime * shrinkSpeed
        );

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

    public void OnShoot()
    {
        crosshairRect.sizeDelta = new Vector2(shootSize, shootSize);
    }

    public void OnHit()
    {
        if (hitmarker == null) return;

        hitmarker.enabled = true;
        hitmarkerTimer = hitmarkerDuration;
    }
}